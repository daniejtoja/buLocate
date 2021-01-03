using BingMapsRESTToolkit;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using buLocate;

namespace Networking
{
    class HandleMap
    {

        public static MainWindow actualWindow;
        public static async void GetRoute(Microsoft.Maps.MapControl.WPF.Location fwp, Microsoft.Maps.MapControl.WPF.Location swp, String key, Map map)
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


                MainWindow.receivedRoute = new MapPolyline()
                {
                    Locations = locs,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 2
                };

                map.Children.Add(MainWindow.receivedRoute);

            }


        }

        public static void addUserPin(UserInfo user)
        {
            if (actualWindow.infoOfMe.userInfo.nickname.Equals(user.nickname)) return;

            Pushpin temporary = new Pushpin
            {
                Location = user.location,
                Tag = user.animalType,
                Name = user.nickname,
                Template = actualWindow.pinTemplate
            };

            actualWindow.collectionOfUsers.Add(new User()
            {
                userInfo = user,
                userPin = temporary
            });

            actualWindow.addToMap(temporary);

        }

        public static void updateUserPin(UserInfo user)
        {
            if (actualWindow.infoOfMe.userInfo.nickname.Equals(user.nickname)) return;

            foreach (var obj in ((MainWindow)System.Windows.Application.Current.MainWindow).collectionOfUsers)
            {
                if (obj.userInfo.nickname == user.nickname)
                {
                    obj.userInfo.location = user.location;
                    obj.userPin.Location = user.location;
                }

            }
        }

        public static void deleteUserPin(UserInfo user)
        {
            int foundIndex = -1;
            for (int i = 0; i < ((MainWindow)System.Windows.Application.Current.MainWindow).collectionOfUsers.Count; ++i)
            {
                if (((MainWindow)System.Windows.Application.Current.MainWindow).collectionOfUsers.ElementAt(i).userInfo.nickname.Equals(user.nickname)){
                    foundIndex = i;
                    break;
                }
            }

            actualWindow.removeFromMap((((MainWindow)System.Windows.Application.Current.MainWindow).collectionOfUsers.ElementAt(foundIndex).userPin));
            ((MainWindow)System.Windows.Application.Current.MainWindow).collectionOfUsers.Remove(((MainWindow)System.Windows.Application.Current.MainWindow).collectionOfUsers.ElementAt(foundIndex));



        }

        public static void addLocalUser(MainWindow mainWindow, UserInfo user)
        {
            actualWindow = mainWindow;
            addUserPin(user);
        }


    }
}
