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
        public static async Task simulateRouteAsync(User user)
        {
            
            if (MainWindow.waypoints == null) return;

            foreach (var loc in MainWindow.waypoints)
            {
                await Task.Delay(1000);
                user.userPin.Location = loc;
                user.userInfo.isNew = false;
                user.userInfo.isLeaving = false;
                user.userInfo.isUpdated = true;
                user.userInfo.location = loc;
                user.userInfo.lastActivityTime = System.DateTime.Now;
                MainWindow.mainWindowReference.myPin.Location = loc;

                NetworkingHandler.Publisher.publish(user.userInfo);


                MapPolyline temporaryLine = MainWindow.receivedRoute;
                //Linia kodu powyżej pozwala na upiększenie drogi. Nasza linia nie mryga:-)
                await NetworkingHandler.HandleMap.GetRoute(user.userInfo.location, MainWindow.mainWindowReference.destinationPin.Location, "AkighsMVKBS9_moe1f5jUMph7JzLnYAcKhUCpiz5UwutaaQW7iCmQNMmKnomLqJ9", MainWindow.mainWindowReference.mainMap);
                MainWindow.mainWindowReference.mainMap.Children.Remove(temporaryLine);

            }
            MainWindow.mainWindowReference.mainMap.Children.Remove(MainWindow.mainWindowReference.destinationPin);


        }
    }
}
