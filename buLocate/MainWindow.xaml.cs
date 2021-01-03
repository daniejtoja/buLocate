using buLocate.NetworkingHandler;
using buLocate.UserHandler;
using EasyNetQ;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace buLocate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 1. mainWindowReference - Referencja do okienka - ułatwiony dostęp z zewnętrznych klas
        /// 2. isConnected - Kontrolka sprawdzająca czy użytkownik jest podłączony
        /// 3. isAnimalPinPlaced - Kontrolka sprawdzająca czy użytkownik postawił swojego pina
        /// 4. isRouteDisplayed - Kontrolka sprawdzająca czy wyświetlana jest trasa na mapie
        /// 5. destinationPin - Pushpin z destynacją
        /// 6. myPin - Pushpin z naszą lokalizacją
        /// 7. infoOfMe - Referencja do obiektu zawierającego informacje o użytkowniku - ten obiekt jest przekazywany w przypadku publisha.
        /// 8. receivedRoute - Referencja do drogi na mapie - ułatwia dostęp z zewnętrznych klas
        /// 9. collectionOfUsers - Kolekcja z wszystkimi obecnymi użytkownikami
        /// 10. pinTemplate - Referencja do ControlTemplate, który pozwala mi dodawać ikonki
        /// 11. waypoints - punkty po drodze - ułatwia symulacje danych GPS
        /// 12. rabbitBus - Szyna wysyłająca i odbierająca dane z rabbita.
        /// 13. afkTimer - Timer dla funkcji usuwającej osoby AFK.
        /// </summary>
        public static MainWindow mainWindowReference { get; private set; }
        private bool isConnected { get; set; } = false;
        private bool isAnimalPinPlaced { get; set; } = false;
        private bool isLocationPinPlaced { get; set; } = false;
        private bool isRouteDisplayed { get; set; } = false;
        public Pushpin destinationPin { get; private set; } = null;
        public Pushpin myPin { get; private set; } = null;
        public User infoOfMe { get; private set; }
        public static MapPolyline receivedRoute { get; set; } = null;
        public List<User> collectionOfUsers { get; } = new List<User>();
        public ControlTemplate pinTemplate { get; private set; }
        public static LocationCollection waypoints { get; set; }
        public static IBus rabbitBus { get; set; }
        private Timer afkTimer { get; set; }


        /// <summary>
        /// Inicjalizacja aplikacji,
        /// mainWindowReference = referencja do obecnego okna
        /// providerMap = referencja do mapy z okna
        /// pinTemplate = referencja do ControlTemplate z DynamicResource
        /// rabbitBus = połączenie z szyną w Rabbicie
        /// </summary>

        public MainWindow()
        {
            InitializeComponent();
            mainWindowReference = this;
            pinTemplate = (ControlTemplate)FindResource("PushpinControlTemplate");
            rabbitBus = RabbitHutch.CreateBus("host=sparrow.rmq.cloudamqp.com;virtualHost=ryrrglkj;username=ryrrglkj;password=XMSeAe8LnWckqsGxlNGNb5ShUEW22HcK");

        }

        /// <summary>
        /// Logika przycisku "Connect"
        /// Funkcja odpowiadająca za łączenie z wysyłaniem i odbieraniem z Rabbita.
        /// Dodatkowa logika związana z rozłączeniem się i wyłączeniem apki 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void clickConnect(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                if (!checkConditions())
                    return;
                if (!isAnimalPinPlaced)
                {
                    MessageBox.Show("Pin not placed. Please place a pin on a map before connecting.", "Pin not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                usernameBox.IsEnabled = false;
                typeBox.IsEnabled = false;
                isConnected = true;
                Publisher.publish(infoOfMe.userInfo);
                Subscriber.subscribe();
                connButton.Content = "Disconnect";
                afkTimer = new System.Threading.Timer(tmr => HandleAFKs.deleteAfks(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            }
            else
            {
                if (MessageBox.Show("Do you really want to disconnect and exit?", "Should I?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    infoOfMe.userInfo.isLeaving = true;
                    infoOfMe.userInfo.isNew = false;
                    infoOfMe.userInfo.isUpdated = false;
                    Publisher.publish(infoOfMe.userInfo);
                    rabbitBus.Dispose();
                    connButton.Content = "Connect";
                    System.Windows.Application.Current.Shutdown();
                }


            }
        }

        /// <summary>
        /// Tu dzieje się magia po dwukliku na mapie.
        /// e.Handled = true - mówimy domyślnej funkcji dwukliku, że już się nim zajmujemy i dzięki temu nie wystąpi domyślna funkcja
        /// dla dwukliku na mapie, czyli zoom.
        /// Jeżeli użytkownik nie jest połączony, to na mapie zostanie dodany pin z wybranym zwierzakiem
        /// Jeżeli użytkownik jest połączony, to na mapie zostanie dodany pin destynacji
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void mapDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (!isConnected)
                addAnimal(sender, e);
            else
                addPin(sender, e);
        }


        /// <summary>
        /// Na podstawie lokalizacji myszki na mapie, dodawany jest do niej pin. Sprawdzane są także warunki czy istnieje już jakiś inny pin destynacyjny.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private async void addPin(object sender, MouseButtonEventArgs e)
        {

            if (receivedRoute != null)
            {
                mainMap.Children.Remove(receivedRoute);
                isRouteDisplayed = false;
            }

            if (isLocationPinPlaced)
            {
                mainMap.Children.Remove(destinationPin);
                isLocationPinPlaced = false;
            }
            if (isRouteDisplayed)
            {
                mainMap.Children.Remove(receivedRoute);
                isRouteDisplayed = false;
            }



            Point mousePos = e.GetPosition((IInputElement)sender);
            Location pinLoc = mainMap.ViewportPointToLocation(mousePos);

            destinationPin = new Pushpin
            {
                Location = pinLoc,
                Tag = "pin",
                Template = (ControlTemplate)FindResource("PushpinControlTemplate")
            };
            mainMap.Children.Add(destinationPin);
            isLocationPinPlaced = true;

            await getDirections();

            await GPSHandler.GpsHandler.simulateRouteAsync(infoOfMe);



        }

        /// <summary>
        /// Funkcja tworzy na mapie pin z wybranym zwierzakiem.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addAnimal(object sender, MouseButtonEventArgs e)
        {

            if (!checkConditions())
                return;



            bool newUser = true;
            bool isBeingUpdated = false;
            if (isAnimalPinPlaced)
            {
                mainMap.Children.Remove(myPin);
                isAnimalPinPlaced = false;
                newUser = false;
                isBeingUpdated = true;
            }


            Point mousePos = e.GetPosition((IInputElement)sender);
            Location pinLoc = mainMap.ViewportPointToLocation(mousePos);

            myPin = new Pushpin
            {
                Location = pinLoc,
                Tag = ((ComboBoxItem)typeBox.SelectedItem).Name.ToString(),
                Name = usernameBox.Text,
                Template = (ControlTemplate)FindResource("PushpinControlTemplate")
            };
            mainMap.Children.Add(myPin);
            UserInfo toAdd = new UserInfo
            {
                nickname = myPin.Name,
                animalType = (String)myPin.Tag,
                location = myPin.Location,
                lastActivityTime = System.DateTime.Now,
                isNew = newUser,
                isUpdated = isBeingUpdated,
                isLeaving = false

            };


            infoOfMe = new User
            {
                userInfo = toAdd,
                userPin = myPin
            };
            collectionOfUsers.Add(infoOfMe);

            isAnimalPinPlaced = true;

        }


        /// <summary>
        /// Wywołanie metody, która rysuje na mapie trase (Routing)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task getDirections()
        {
            if (isRouteDisplayed) return;
            else
                await HandleMap.GetRoute(myPin.Location, destinationPin.Location, "AkighsMVKBS9_moe1f5jUMph7JzLnYAcKhUCpiz5UwutaaQW7iCmQNMmKnomLqJ9", mainMap);


            isRouteDisplayed = true;
        }

        /// <summary>
        /// Ten kod wykonuje się przy naciśnięciu X
        /// Wysyłamy do Rabbita informacje o wyjściu, aby usunąć naszą ikonkę z mapy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseApp(object sender, CancelEventArgs e)
        {
            infoOfMe.userInfo.isLeaving = true;
            infoOfMe.userInfo.isNew = false;
            infoOfMe.userInfo.isUpdated = false;
            NetworkingHandler.Publisher.publish(infoOfMe.userInfo);
            rabbitBus.Dispose();
            afkTimer.Dispose();
        }

        /// <summary>
        /// Funkcja sprawdzająca warunki do połączenia się:
        /// 1. Użytkownik ma nick
        /// 2. Użytkownik ma wybrany typ (zwierzaka)
        /// 3. Użytkownik wybrał miejsce startowe na mapie
        /// </summary>
        /// <returns></returns>
        public bool checkConditions()
        {
            if (usernameBox.Text.Equals("Enter username...") || usernameBox.Text.Equals(""))
            {
                MessageBox.Show("Username not specified. Please specify username before connecting.", "Username not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (typeBox.SelectedItem == null)
            {
                MessageBox.Show("Animal type not specified. Please specify animal type before connecting.", "Animal type not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;

        }
    }
}
