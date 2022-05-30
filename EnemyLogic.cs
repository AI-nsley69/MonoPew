using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame
{
    public class EnemyLogic
    {
        public int Spawned = 0;
        public int BossRequirement;
        public TimeSpan LastSpawned;
        public Vector2 Speed = new Vector2(0, 2);
    }
}