using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame
{
    public class Boss
    {
        // Field for the Vector2 position of the power up
        public Vector2 Pos;

        public Rectangle Hitbox;
        
        public Vector2 Speed = new Vector2(8f, 0);

        public int HitPoints;

        public Boss(Vector2 pos, int hitPoints)
        {
            this.Pos = pos;
            this.HitPoints = hitPoints;
        }

        public void UpdateHitbox(Texture2D texture) {
            var posX = Convert.ToInt32(Math.Round(this.Pos.X));
            var posY = Convert.ToInt32(Math.Round(this.Pos.Y));
            this.Hitbox = new Rectangle(posX, posY, texture.Width, texture.Height);
        }
    }
}