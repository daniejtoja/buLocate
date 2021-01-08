using BingMapsRESTToolkit;
using buLocate.UserHandler;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace buLocate.NetworkingHandler
{
    public static class HandleMap
    {
        /// <summary>
        /// Logika routingu.
        /// Skorzystałem z BingMapsRESTToolkit
        /// </summary>
        /// <param name="fwp">FirstWaypoint - Lokalizacja jednego pina - tego ze zwierzakiem</param>
        /// <param name="swp">SecondWaypoint - Lokalizacja drugiego pina - destynacji</param>
        /// <param name="key">Klucz do map bing</param>
        /// <param name="map">Referencja do mapy, aby dodać do niej drogę</param>
        /// <returns></returns>
        public static async Task GetRoute(Microsoft.Maps.MapControl.WPF.Location fwp, Microsoft.Maps.MapControl.WPF.Location swp, String key, Map map)
        {

            var request = new RouteRequest()
            {
                Waypoints = new List<SimpleWaypoint>()
                {
                    new SimpleWaypoint(fwp.Latitude, fwp.Longitude),
                    new SimpleWaypoint(swp.Latitude, swp.Longitude)
                },
                BingMapsKey = key,
                RouteOptions = new RouteOptions()
                {
                    TravelMode = TravelModeType.Driving,
                    Optimize = RouteOptimizationType.Time,
                    RouteAttributes = new List<RouteAttributeType>(){
                        RouteAttributeType.RoutePath
                    }
                }
            };

            var response = await request.Execute();

            if (response != null &&
                response.ResourceSets != null &&
                response.ResourceSets.Length > 0 &&
                response.ResourceSets[0].Resources != null &&
                response.ResourceSets[0].Resources.Length > 0)
            {
                var result = response.ResourceSets[0].Resources[0] as Route;
                double[][] routePath = result.RoutePath.Line.Coordinates;
                LocationCollection locs = new LocationCollection();

                for (int i = 0; i < routePath.Length; ++i)
                    if (routePath[i].Length >= 2)
                        locs.Add(new Microsoft.Maps.MapControl.WPF.Location(routePath[i][0], routePath[i][1]));

                MainWindow.Waypoints = new LocationCollection();
                MainWindow.Waypoints = locs;

                MainWindow.ReceivedRoute = new MapPolyline()
                {
                    Locations = locs,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 2
                };

                map.Children.Add(MainWindow.ReceivedRoute);


            }



        }

        /// <summary>
        /// Logika dodawania użytkowników na podstawie wiadomości z Rabbita
        /// Ignorujemy siebie
        /// </summary>
        /// <param name="user">Użytkownik, którego chcemy dodać</param>
        public static void AddUserPin(UserInfo user)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                if (MainWindow.MainWindowReference.InfoOfMe.UserInfo.Nickname.Equals(user.Nickname)) return;

                Pushpin temporary = new Pushpin
                {
                    Location = user.UserLocation,
                    Tag = user.AnimalType,
                    Name = user.Nickname,
                    Template = MainWindow.MainWindowReference.PinTemplate
                };

                MainWindow.MainWindowReference.CollectionOfUsers.Add(new User()
                {
                    UserInfo = user,
                    UserPin = temporary
                });

                MainWindow.MainWindowReference.mainMap.Children.Add(temporary);
            }));


        }

        /// <summary>
        /// Logika aktualizacji użytkowników na podstawie wiadomości z Rabbita
        /// Ignorujemy siebie
        /// </summary>
        /// <param name="user">Użytkownik, którego chcemy edytować</param>
        public static void UpdateUserPin(UserInfo user)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                if (MainWindow.MainWindowReference.InfoOfMe.UserInfo.Nickname.Equals(user.Nickname)) return;

                foreach (var usr in MainWindow.MainWindowReference.CollectionOfUsers)
                {
                    if (usr.UserInfo.Nickname == user.Nickname)
                    {
                        usr.UserInfo.UserLocation = user.UserLocation;
                        usr.UserPin.Location = user.UserLocation;
                        usr.UserInfo.LastActivityTime = System.DateTime.Now;
                    }

                }
            }));


        }

        /// <summary>
        /// Logika usuwania pinów na podstawie kolejki. 
        /// Tych, których nie ma ignorujemy.
        /// </summary>
        /// <param name="user"></param>

        public static void DeleteUserPin(UserInfo user)
        {

            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                int foundIndex = -1;
                for (int i = 0; i < MainWindow.MainWindowReference.CollectionOfUsers.Count; ++i)
                {
                    if (MainWindow.MainWindowReference.CollectionOfUsers.ElementAt(i).UserInfo.Nickname.Equals(user.Nickname))
                    {
                        foundIndex = i;
                        break;
                    }
                }
                if (foundIndex == -1) return;
                MainWindow.MainWindowReference.mainMap.Children.Remove(MainWindow.MainWindowReference.CollectionOfUsers.ElementAt(foundIndex).UserPin);
                MainWindow.MainWindowReference.CollectionOfUsers.Remove(MainWindow.MainWindowReference.CollectionOfUsers.ElementAt(foundIndex));

            }));



        }


    }
}
