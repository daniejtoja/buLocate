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
        /// 14. isGPSBeingSimulated - kontrolka do blokady dodawania pinów gdy symulujemy poruszanie się
        /// </summary>
        public static MainWindow MainWindowReference { get; private set; }
        private bool IsConnected { get; set; } = false;
        private bool IsAnimalPinPlaced { get; set; } = false;
        private bool IsLocationPinPlaced { get; set; } = false;
        private bool IsRouteDisplayed { get; set; } = false;
        public Pushpin DestinationPin { get; private set; } = null;
        public Pushpin MyPin { get; private set; } = null;
        public User InfoOfMe { get; private set; }
        public static MapPolyline ReceivedRoute { get; set; } = null;
        public List<User> CollectionOfUsers { get; } = new List<User>();
        public ControlTemplate PinTemplate { get; private set; }
        public static LocationCollection Waypoints { get; set; }
        public static IBus RabbitBus { get; set; }
        private Timer AfkTimer { get; set; }
        public bool IsGPSBeingSimulated { private get; set; } = false;
        


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
            MainWindowReference = this;
            PinTemplate = (ControlTemplate)FindResource("PushpinControlTemplate");
            RabbitBus = RabbitHutch.CreateBus("host=sparrow.rmq.cloudamqp.com;virtualHost=ryrrglkj;username=ryrrglkj;password=XMSeAe8LnWckqsGxlNGNb5ShUEW22HcK");
        }

        /// <summary>
        /// Logika przycisku "Connect"
        /// Funkcja odpowiadająca za łączenie z wysyłaniem i odbieraniem z Rabbita.
        /// Dodatkowa logika związana z rozłączeniem się i wyłączeniem apki 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void ClickConnect(object sender, RoutedEventArgs e)
        {
            if (!IsConnected)
            {
                if (!CheckConditions())
                    return;
                if (!IsAnimalPinPlaced)
                {
                    MessageBox.Show("Pin not placed. Please place a pin on a map before connecting.", "Pin not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                usernameBox.IsEnabled = false;
                typeBox.IsEnabled = false;
                IsConnected = true;
                Publisher.Publish(InfoOfMe.UserInfo);
                Subscriber.Subscribe();
                connButton.Content = "Disconnect";
                AfkTimer = new System.Threading.Timer(tmr => HandleAFKs.DeleteAfks(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

            }
            else
            {
                if (MessageBox.Show("Do you really want to disconnect and exit?", "Should I?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    InfoOfMe.UserInfo.IsLeaving = true;
                    InfoOfMe.UserInfo.IsNew = false;
                    InfoOfMe.UserInfo.IsUpdated = false;
                    Publisher.Publish(InfoOfMe.UserInfo);
                    RabbitBus.Dispose();
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

        private void MapDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (!IsConnected)
                AddAnimal(sender, e);
            else
                AddPin(sender, e);
        }


        /// <summary>
        /// Na podstawie lokalizacji myszki na mapie, dodawany jest do niej pin. Sprawdzane są także warunki czy istnieje już jakiś inny pin destynacyjny.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private async void AddPin(object sender, MouseButtonEventArgs e)
        {
            if (IsGPSBeingSimulated) return;

            if (ReceivedRoute != null)
            {
                mainMap.Children.Remove(ReceivedRoute);
                IsRouteDisplayed = false;
            }

            if (IsLocationPinPlaced)
            {
                mainMap.Children.Remove(DestinationPin);
                IsLocationPinPlaced = false;
            }
            if (IsRouteDisplayed)
            {
                mainMap.Children.Remove(ReceivedRoute);
                IsRouteDisplayed = false;
            }



            Point mousePos = e.GetPosition((IInputElement)sender);
            Location pinLoc = mainMap.ViewportPointToLocation(mousePos);

            DestinationPin = new Pushpin
            {
                Location = pinLoc,
                Tag = "pin",
                Template = (ControlTemplate)FindResource("PushpinControlTemplate")
            };
            mainMap.Children.Add(DestinationPin);
            IsLocationPinPlaced = true;

            await GetDirections();
            await GPSHandler.GpsHandler.SimulateRouteAsync(InfoOfMe);



        }

        /// <summary>
        /// Funkcja tworzy na mapie pin z wybranym zwierzakiem.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddAnimal(object sender, MouseButtonEventArgs e)
        {

            if (!CheckConditions())
                return;



            bool newUser = true;
            bool isBeingUpdated = false;
            if (IsAnimalPinPlaced)
            {
                mainMap.Children.Remove(MyPin);
                IsAnimalPinPlaced = false;
                newUser = false;
                isBeingUpdated = true;
            }


            Point mousePos = e.GetPosition((IInputElement)sender);
            Location pinLoc = mainMap.ViewportPointToLocation(mousePos);

            if (usernameBox.Text.Contains(" "))
                usernameBox.Text = usernameBox.Text.ToString().Replace(@" ", @"_");

            MyPin = new Pushpin
            {
                Location = pinLoc,
                Tag = ((ComboBoxItem)typeBox.SelectedItem).Name.ToString(),
                Name = String.Format("{0}_{1}_{2}", usernameBox.Text, DateTime.Now.Millisecond, Guid.NewGuid().ToString().Replace(@"-",@"_")),
                Template = (ControlTemplate)FindResource("PushpinControlTemplate")
            };
            mainMap.Children.Add(MyPin);
            UserInfo toAdd = new UserInfo
            {
                Nickname = MyPin.Name,
                AnimalType = (String)MyPin.Tag,
                UserLocation = MyPin.Location,
                LastActivityTime = System.DateTime.Now,
                IsNew = newUser,
                IsUpdated = isBeingUpdated,
                IsLeaving = false

            };


            InfoOfMe = new User
            {
                UserInfo = toAdd,
                UserPin = MyPin
            };
            CollectionOfUsers.Add(InfoOfMe);

            IsAnimalPinPlaced = true;

        }


        /// <summary>
        /// Wywołanie metody, która rysuje na mapie trase (Routing)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task GetDirections()
        {
            if (IsRouteDisplayed) return;
            else
                await HandleMap.GetRoute(MyPin.Location, DestinationPin.Location, "AkighsMVKBS9_moe1f5jUMph7JzLnYAcKhUCpiz5UwutaaQW7iCmQNMmKnomLqJ9", mainMap);


            IsRouteDisplayed = true;
        }

        /// <summary>
        /// Ten kod wykonuje się przy naciśnięciu X
        /// Wysyłamy do Rabbita informacje o wyjściu, aby usunąć naszą ikonkę z mapy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseApp(object sender, CancelEventArgs e)
        {
            InfoOfMe.UserInfo.IsLeaving = true;
            InfoOfMe.UserInfo.IsNew = false;
            InfoOfMe.UserInfo.IsUpdated = false;
            NetworkingHandler.Publisher.Publish(InfoOfMe.UserInfo);
            RabbitBus.Dispose();
            AfkTimer.Dispose();
        }

        /// <summary>
        /// Funkcja sprawdzająca warunki do połączenia się:
        /// 1. Użytkownik ma nick
        /// 2. Użytkownik ma wybrany typ (zwierzaka)
        /// 3. Użytkownik wybrał miejsce startowe na mapie
        /// </summary>
        /// <returns></returns>
        public bool CheckConditions()
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
