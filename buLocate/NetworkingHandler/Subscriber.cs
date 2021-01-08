using EasyNetQ;

namespace buLocate.NetworkingHandler
{
    public static class Subscriber
    {
        /// <summary>
        /// Subskrypcja kolejki Rabbita i wybranie funkcji, która ma się zajmować wiadomością.
        /// </summary>
        public static void Subscribe()
        {
            MainWindow.RabbitBus.PubSub.SubscribeAsync<UserInfo>("NewLocations", message => HandleMessageReceived.Handle(message));
        }

    }
}
