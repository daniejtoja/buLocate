using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace buLocate.UserHandler
{
    /// <summary>
    /// Klasa pomocnicza do DistanceCalc
    /// Opisuje prędkość z jaką powinno poruszać się każde zwierze
    /// Niestety za późno o tym pomyślałem
    /// </summary>
    static class AnimalSpeed
    {
        private static readonly int bird = 40;
        private static readonly int bull = 15;
        private static readonly int cat = 25;
        private static readonly int dog = 30;
        private static readonly int cow = 10;

        public static int GetUserSpeed(User user)
        {
            switch (user.UserInfo.AnimalType)
            {
                case "bird":
                    return bird;
                case "bull":
                    return bull;
                case "cat":
                    return cat;
                case "dog":
                    return dog;
                case "cow":
                default:
                    return cow;
            }
            
        }
    }
}
