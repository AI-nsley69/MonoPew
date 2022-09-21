using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame
{
    public class Bullet
    {
        // Field for the texture of the bullet
        public Texture2D Texture;
        // Field for the hitbox of the bullet
        public Rectangle Hitbox;
        // Field for the Vector2 position of the bullet
        public Vector2 Pos = new Vector2(0,0);
        // Field for the Vector2 speed of the bullet
        public Vector2 Speed = new Vector2(0, -9.0f);

        public Bullet(Vector2 shipPos)
        {
            this.Pos = shipPos;
        }
        
        public void UpdateHitbox(Texture2D texture) {
            var posX = Convert.ToInt32(Math.Round(this.Pos.X));
            var posY = Convert.ToInt32(Math.Round(this.Pos.Y));
            this.Hitbox = new Rectangle(posX, posY, texture.Width, texture.Height);
        }
        
    }
}