using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame
{
    public class PowerUpLogic
    {
        public int Spawned = 0;
        public int Max;
        public int LastPickUp;
        public int PointsInterval;
    }
}