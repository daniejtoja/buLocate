using System;

namespace buLocate.NetworkingHandler
{
    static class HandleAFKs
    {
        /// <summary>
        /// Usuwamy użytkowników, którzy od 5 minut nie zmienili swojego położenia.
        /// </summary>
        public static void DeleteAfks()
        {
            foreach (var item in MainWindow.MainWindowReference.CollectionOfUsers)
            {
                if (TimeSpan.Compare(DateTime.Now.Subtract(item.UserInfo.LastActivityTime), TimeSpan.FromMinutes(5)) > 1)
                    HandleMap.DeleteUserPin(item.UserInfo);
            }
        }

    }
}
