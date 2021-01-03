using System.Linq;


namespace buLocate.NetworkingHandler
{
    static class HandleMessageReceived
    {

        /// <summary>
        /// Logika przetwarzania wiadomości z Rabbita
        /// </summary>
        /// <param name="user">Wiadomość z Rabbita dotycząca użytkownika</param>
        public static void handle(UserInfo user)
        {

            if (user.isNew)
                HandleMap.addUserPin(user);
            else if (user.isUpdated)
            {
                if (MainWindow.mainWindowReference.collectionOfUsers.Any(item => item.userInfo.nickname == user.nickname))
                    HandleMap.updateUserPin(user);
                else
                    HandleMap.addUserPin(user);
            }

            else if (user.isLeaving)
                HandleMap.deleteUserPin(user);




        }
    }
}
