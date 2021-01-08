using Microsoft.Maps.MapControl.WPF;

namespace buLocate.NetworkingHandler
{
    /// <summary>
    /// 1. Nick użytkownika
    /// 2. Jakim zwierzątkiem jest użytkownik
    /// 3. Jego lokalizacja
    /// 4. Czas ostatniej aktywności
    /// 5. Switch do rozwiązywania kwestii czy użytkownik jest nowy
    /// 6. Switch do rozwiązywania kwestii czy użytkownika trzeba edytować
    /// 7. Switch do rozwiązywania kwestii czy użytkownika trzeba usunąć
    /// </summary>
    public class UserInfo
    {
        public string Nickname { get; set; }
        public string AnimalType { get; set; }
        public Location UserLocation { get; set; }
        public System.DateTime LastActivityTime { get; set; }
        public bool IsNew { get; set; }
        public bool IsUpdated { get; set; }
        public bool IsLeaving { get; set; }
    }
}
