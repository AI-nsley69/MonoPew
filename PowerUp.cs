using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame
{
    public class PowerUp
    {
        // Field for the Vector2 position of the power up
        public Vector2 Pos;

        public Rectangle Hitbox;

        public PowerUp(Vector2 pos)
        {
            this.Pos = pos;
        }

        public void UpdateHitbox(Texture2D texture) {
            var posX = Convert.ToInt32(Math.Round(this.Pos.X));
            var posY = Convert.ToInt32(Math.Round(this.Pos.Y));
            this.Hitbox = new Rectangle(posX, posY, texture.Width, texture.Height);
        }
    }
}