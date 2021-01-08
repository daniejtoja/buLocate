using buLocate.UserHandler;
using Microsoft.Maps.MapControl.WPF;
using System.Threading.Tasks;

namespace buLocate.GPSHandler
{
    public static class GpsHandler
    {
        /// <summary>
        /// Logika symulacji pozycji GPS.
        /// 3,5s, aby wyruszyć
        /// co 1s zmiana pozycji
        /// po dotarciu na miejsce usuwamy pin.
        /// </summary>
        /// <param name="user"></param>
        public static async Task SimulateRouteAsync(User user)
        {
            
            
            if (MainWindow.Waypoints == null) return;

            MainWindow.MainWindowReference.IsGPSBeingSimulated = true;

            foreach (var loc in MainWindow.Waypoints)
            {
                await Task.Delay(1000);
                user.UserPin.Location = loc;
                user.UserInfo.IsNew = false;
                user.UserInfo.IsLeaving = false;
                user.UserInfo.IsUpdated = true;
                user.UserInfo.UserLocation = loc;
                user.UserInfo.LastActivityTime = System.DateTime.Now;
                MainWindow.MainWindowReference.MyPin.Location = loc;

                NetworkingHandler.Publisher.Publish(user.UserInfo);


                MapPolyline temporaryLine = MainWindow.ReceivedRoute;
                //Linia kodu powyżej pozwala na upiększenie drogi. Nasza linia nie mryga:-)
                await NetworkingHandler.HandleMap.GetRoute(user.UserInfo.UserLocation, MainWindow.MainWindowReference.DestinationPin.Location, "AkighsMVKBS9_moe1f5jUMph7JzLnYAcKhUCpiz5UwutaaQW7iCmQNMmKnomLqJ9", MainWindow.MainWindowReference.mainMap);
                MainWindow.MainWindowReference.mainMap.Children.Remove(temporaryLine);

            }
            MainWindow.MainWindowReference.mainMap.Children.Remove(MainWindow.MainWindowReference.DestinationPin);
            MainWindow.MainWindowReference.IsGPSBeingSimulated = false;
            MainWindow.MainWindowReference.mainMap.Children.Remove(MainWindow.ReceivedRoute);

        }
    }
}
