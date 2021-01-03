using Microsoft.Maps.MapControl.WPF;

namespace buLocate
{
    /// <summary>
    /// Opakowanie
    /// Do jednego obiektu mamy przypisanego użytkownika i jego pina.
    /// </summary>
    public class User
    {
        public buLocate.NetworkingHandler.UserInfo userInfo { get; set; }
        public Pushpin userPin { get; set; }

    }
}
