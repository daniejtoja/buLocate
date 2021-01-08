using EasyNetQ;


namespace buLocate.NetworkingHandler
{
    static public class Publisher
    {
        /// <summary>
        /// Asynchroniczne wysyłanie do kolejki.
        /// </summary>
        /// <param name="userInfo">To co chcemy wysłać</param>
        public static void Publish(UserInfo userInfo)
        {
            MainWindow.RabbitBus.PubSub.PublishAsync(userInfo, "NewLocations");
        }
    }
}
