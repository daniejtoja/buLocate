using System;

namespace buLocate.NetworkingHandler
{
    static class HandleAFKs
    {
        /// <summary>
        /// Usuwamy użytkowników, którzy od 5 minut nie zmienili swojego położenia.
        /// </summary>
        public static void deleteAfks()
        {
            foreach (var item in MainWindow.mainWindowReference.collectionOfUsers)
            {
                if (TimeSpan.Compare(DateTime.Now.Subtract(item.userInfo.lastActivityTime), TimeSpan.FromMinutes(5)) > 1)
                    HandleMap.deleteUserPin(item.userInfo);
            }
        }

    }
}
