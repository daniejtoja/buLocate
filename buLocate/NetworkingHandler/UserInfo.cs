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
        public string nickname { get; set; }
        public string animalType { get; set; }
        public Location location { get; set; }
        public System.DateTime lastActivityTime { get; set; }
        public bool isNew { get; set; }
        public bool isUpdated { get; set; }
        public bool isLeaving { get; set; }
    }
}
