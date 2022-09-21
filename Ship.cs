using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame
{
    public class Ship
    {
        // Field for the texture of the ship
        public Texture2D Texture;
        // Rectangle of the ship
        public Rectangle Hitbox;
        // Field for the Vector2 position of the ship
        public Vector2 Pos = new Vector2(0,0);
        // Field for the Vector2 speed of the ship
        public Vector2 Speed;
        // Field for color on ship
        public Color Color = Color.White;

        public int Lives = 0;
        public int CurrentLevel = 1;
        public int BulletCooldown = 0;
        public int Points = 0;

        public void UpdateHitbox() {
            var posX = Convert.ToInt32(Math.Round(this.Pos.X));
            var posY = Convert.ToInt32(Math.Round(this.Pos.Y));
            this.Hitbox = new Rectangle(posX, posY, this.Texture.Width, this.Texture.Height);
        }
    }
}