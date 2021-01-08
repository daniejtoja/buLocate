using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace buLocate.UserHandler
{
    /// <summary>
    /// Opakowanie
    /// Do jednego obiektu mamy przypisanego użytkownika i jego pina.
    /// </summary>
    public class User
    {
        public buLocate.NetworkingHandler.UserInfo UserInfo { get; set; }
        public Microsoft.Maps.MapControl.WPF.Pushpin UserPin { get; set; }

    }
}
