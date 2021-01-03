using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
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
