using System.Linq;


namespace buLocate.NetworkingHandler
{
    static class HandleMessageReceived
    {

        /// <summary>
        /// Logika przetwarzania wiadomości z Rabbita
        /// </summary>
        /// <param name="user">Wiadomość z Rabbita dotycząca użytkownika</param>
        public static void Handle(UserInfo user)
        {

            if (user.IsNew)
                HandleMap.AddUserPin(user);
            else if (user.IsUpdated)
            {
                if (MainWindow.MainWindowReference.CollectionOfUsers.Any(item => item.UserInfo.Nickname == user.Nickname))
                    HandleMap.UpdateUserPin(user);
                else
                    HandleMap.AddUserPin(user);
            }

            else if (user.IsLeaving)
                HandleMap.DeleteUserPin(user);




        }
    }
}
