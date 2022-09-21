using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        // Initialze state
        enum gameState
        {
            Paused,
            Running,
            GameOver
        }
        private gameState GameState = gameState.Paused;
        // Initialize the ship
        private Ship MainShip = new Ship();
        private PowerUpLogic PowerUpLogic = new PowerUpLogic();
        private EnemyLogic EnemyLogic = new EnemyLogic();
        // Initialize fonts
        private SpriteFont GameFont;
        private SpriteFont MenuFont;
        // Initialize entity HashSets
        private (HashSet<Bullet> Bullets, HashSet<PowerUp> PowerUps, HashSet<Enemy> Enemies, HashSet<Boss> Bosses) Entities = (
            new HashSet<Bullet>(),
            new HashSet<PowerUp>(),
            new HashSet<Enemy>(), 
            new HashSet<Boss>()
        );
        // Intialize entity textures
        private Textures Textures = new Textures();
        // Initialize sounds
        private Sounds Sounds = new Sounds();
        // Initialize random variable
        private Random Rand = new Random();
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        // Method to easily set spawn point
        private void SetSpawnPoint()
        {
            MainShip.Pos = new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 1.5f);
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 900;
            _graphics.PreferredBackBufferHeight = 500;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // Font
            GameFont = Content.Load<SpriteFont>("Fonts/GameFont");
            MenuFont = Content.Load<SpriteFont>("Fonts/MenuFont");
            // Textures
            MainShip.Texture = Content.Load<Texture2D>("Sprites/Ship");
            Textures.Bullet = Content.Load<Texture2D>("Misc/Bullet");
            Textures.PowerUp = Content.Load<Texture2D>("Misc/Coin");
            Textures.Boss = Content.Load<Texture2D>("Sprites/Boss");
            Textures.Enemy = Content.Load<Texture2D>("Sprites/Enemy");
            // Sound
            Sounds.Shoot = Content.Load<SoundEffect>("Sounds/uwu");
        }
        // Summon a bullet at the correct position
        private void SummonBullet()
        {
            if (MainShip.BulletCooldown > 0) return;
            var bulletPos = new Vector2(MainShip.Pos.X + (MainShip.Texture.Width / 2), MainShip.Pos.Y + Textures.Bullet.Height);
            Entities.Bullets.Add(new Bullet(bulletPos));
            MainShip.BulletCooldown = 15;
            Sounds.Shoot.Play();
        }
        // Check the player input for movement speed and summoning a bullet
        private void CheckPlayerInput()
        {
            var keyboardState = Keyboard.GetState();
            // Change the ship's position if correct key is pressed
            if (keyboardState.IsKeyDown(Keys.Right)) MainShip.Pos.X += MainShip.Speed.X;
            if (keyboardState.IsKeyDown(Keys.Left)) MainShip.Pos.X += MainShip.Speed.X * -1;
            if (keyboardState.IsKeyDown(Keys.Up)) MainShip.Pos.Y += MainShip.Speed.Y * -1;
            if (keyboardState.IsKeyDown(Keys.Down)) MainShip.Pos.Y += MainShip.Speed.Y;
            // Call summon bullet if space is pressed down
            if (keyboardState.IsKeyDown(Keys.Space)) SummonBullet();
        }
        // Check enemy boundary
        private void CheckBounds()
        {
            // Check if the ship hits the side, if so, invert the ship's speed
            float rightBound = Window.ClientBounds.Width - MainShip.Texture.Width;
            float leftBound = 0;
            if (MainShip.Pos.X > rightBound) MainShip.Pos.X = rightBound;
            if (MainShip.Pos.X < leftBound) MainShip.Pos.X = leftBound;
            // Check if the ship hits the top or bottom, if so, invert the ship's speed
            float lowerBound = Window.ClientBounds.Height - MainShip.Texture.Height;
            float upperBound = 0;
            if (MainShip.Pos.Y > lowerBound) MainShip.Pos.Y = lowerBound;
            if (MainShip.Pos.Y < upperBound) MainShip.Pos.Y = upperBound;
            // Check if enemies have travelled all the way, if so decrement the ship lives
            foreach (var e in Entities.Enemies.Where(e => e.Pos.Y > Window.ClientBounds.Height - Textures.Enemy.Height)) MainShip.Lives -= 1;
            Entities.Enemies.RemoveWhere(e => e.Pos.Y > Window.ClientBounds.Height - Textures.Enemy.Height);
            // Remove bullets out of bounds
            Entities.Bullets.RemoveWhere(b => b.Pos.Y < 0);
            // Movement for boss
            foreach (var b in Entities.Bosses)
            {
                if (b.Pos.X < 0) b.Speed.X = 7f + MainShip.CurrentLevel;
                else if (b.Pos.X > Window.ClientBounds.Width - Textures.Boss.Width) b.Speed.X = -7f - MainShip.CurrentLevel;
            }
        }

        private bool CheckCollision(Rectangle ship, Rectangle other)
        {
            return ship.Intersects(other);
        }

        private void UpdateHitboxes()
        {
            // Tuplet for lists where we remove entities
            (List<Bullet> Bullets, List<PowerUp> PowerUps, List<Enemy> Enemies) ToRemove = (
                new List<Bullet>(),
                new List<PowerUp>(),
                new List<Enemy>()
            );
            // Update hitbox on its own to prevent unnecessary calculations
            MainShip.UpdateHitbox();
            foreach (var b in Entities.Bullets) b.UpdateHitbox(Textures.Bullet);
            foreach (var e in Entities.Enemies) e.UpdateHitbox(Textures.Enemy);
            foreach (var p in Entities.PowerUps) p.UpdateHitbox(Textures.PowerUp);
            foreach (var b in Entities.Bosses) b.UpdateHitbox(Textures.Boss);
            // Check if ship collides with powerup
            foreach (var p in Entities.PowerUps.Where(p => CheckCollision(MainShip.Hitbox, p.Hitbox)))
            {
                ToRemove.PowerUps.Add(p);
                MainShip.Points += 10 * MainShip.CurrentLevel;
                if (MainShip.Lives < 3) MainShip.Lives += 1;
            }
            // Check if boss hits bullet
            foreach (var b in Entities.Bullets)
            {
                // Enemy check
                foreach (var e in Entities.Enemies.Where(e => CheckCollision(b.Hitbox, e.Hitbox)))
                {
                    ToRemove.Bullets.Add(b);
                    ToRemove.Enemies.Add(e);
                    MainShip.Points += 50 * MainShip.CurrentLevel;
                }
                // Boss check
                foreach (var n in Entities.Bosses.Where(n => CheckCollision(b.Hitbox, n.Hitbox)))
                {
                    ToRemove.Bullets.Add(b);
                    n.HitPoints -= 1;
                }
            }
            // Check if enemy collides with ship
            foreach (var e in Entities.Enemies.Where(e => CheckCollision(MainShip.Hitbox, e.Hitbox)))
            {
                MainShip.Lives -= 1;
            }
            Entities.Enemies.RemoveWhere(e => CheckCollision(MainShip.Hitbox, e.Hitbox));
            // Check if boss collides with ship
            foreach (var b in Entities.Bosses.Where(b => CheckCollision(MainShip.Hitbox, b.Hitbox)))
            {
                MainShip.Lives -= 1;
            }
            // Delete all entities
            ToRemove.Bullets.ForEach(b => Entities.Bullets.Remove(b));
            ToRemove.Enemies.ForEach(e => Entities.Enemies.Remove(e));
            ToRemove.PowerUps.ForEach(p => Entities.PowerUps.Remove(p));
        }

        private void UpdatePosition()
        {
            MainShip.BulletCooldown -= 1;
            foreach (var b in Entities.Bullets) b.Pos += b.Speed;
            foreach (var e in Entities.Enemies) e.Pos += EnemyLogic.Speed;
            foreach (var b in Entities.Bosses) b.Pos += b.Speed;
        }

        private void OnPaused(GameTime gameTime)
        {
            // Remove all entities
            Entities.Bullets.Clear();
            Entities.Enemies.Clear();
            Entities.PowerUps.Clear();
            Entities.Bosses.Clear();
            // Check if enter was pressed
            var keyboardState = Keyboard.GetState();
            if (!keyboardState.IsKeyDown(Keys.Enter)) return;
            if (GameState == gameState.Paused) OnNextLevel();
            if (GameState == gameState.GameOver) OnGameOver();
            GameState = gameState.Running;
            MainShip.Speed = new Vector2(6.5f + (MainShip.CurrentLevel / 8), 6.5f + (MainShip.CurrentLevel / 10));
            // Enemies variables
            EnemyLogic.BossRequirement = 3 + (2 * MainShip.CurrentLevel);
            EnemyLogic.LastSpawned = gameTime.TotalGameTime;
            EnemyLogic.Spawned = 0;
            EnemyLogic.Speed.Y = 1.9f + (MainShip.CurrentLevel / 4);
            // PowerUp variables
            PowerUpLogic.Max = 3 + (MainShip.CurrentLevel / 2);
            PowerUpLogic.Spawned = 0;
            PowerUpLogic.LastPickUp = MainShip.Points;
            PowerUpLogic.PointsInterval = 100 * MainShip.CurrentLevel; 
            SetSpawnPoint();
        }
        private void OnNextLevel()
        {
            MainShip.CurrentLevel += 1;
        }
        
        private void OnGameOver()
        {
            // Mainship variables
            MainShip.Lives = 3;
            MainShip.CurrentLevel = 0;
            MainShip.Points = 0;
        }

        private void SummonEnemy()
        {
            var pos = new Vector2(Rand.Next(Textures.Enemy.Width, Window.ClientBounds.Width - Textures.Enemy.Width)
                , 0);
            Entities.Enemies.Add(new Enemy(pos));
        }

        private void SpawnEnemies(GameTime gameTime)
        {
            var deltaDuration = gameTime.TotalGameTime.TotalMilliseconds - EnemyLogic.LastSpawned.TotalMilliseconds;
            if (!(deltaDuration >= 1500 - (MainShip.CurrentLevel * 10))) return;
            EnemyLogic.LastSpawned = gameTime.TotalGameTime;
            SummonEnemy();
            EnemyLogic.Spawned += 1;
        }

        private void SummonPowerUp()
        {
            var pos = new Vector2(Rand.Next(0, Window.ClientBounds.Width - Textures.PowerUp.Width),
                Rand.Next(Window.ClientBounds.Height / 2, Window.ClientBounds.Height - Textures.PowerUp.Height));
            Entities.PowerUps.Add(new PowerUp(pos));
            PowerUpLogic.Spawned += 1;
            PowerUpLogic.LastPickUp = MainShip.Points;
        }

        private void SpawnPowerUps()
        {
            if (PowerUpLogic.Spawned >= PowerUpLogic.Max) return;
            if (!(MainShip.Points - PowerUpLogic.LastPickUp > PowerUpLogic.PointsInterval)) return;
            SummonPowerUp();
        }

        private void CheckHealth()
        {
            if (MainShip.Lives <= 0) GameState = gameState.GameOver;
        }

        private void SummonBoss(GameTime gameTime)
        {
            var tmpPos = new Vector2(Window.ClientBounds.Width / 2 + Textures.Boss.Width, 30);
            Entities.Bosses.Add(new Boss(tmpPos, 1 + 2 * MainShip.CurrentLevel));
            foreach (var b in Entities.Bosses) b.Speed.X = 7f + MainShip.CurrentLevel;
            EnemyLogic.LastSpawned = gameTime.TotalGameTime;
        }

        private void BossLogic(GameTime gameTime)
        {
            if (!(EnemyLogic.Spawned >= EnemyLogic.BossRequirement)) return;
            if (!(Entities.Bosses.Count > 0)) SummonBoss(gameTime);
            foreach (var b in Entities.Bosses.Where(b => b.HitPoints <= 0)) GameState = gameState.Paused;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (GameState)
            {
                // Paused / next level
                case gameState.Paused:
                {
                    OnPaused(gameTime);
                    break;
                }
                // Running
                case gameState.Running:
                {
                    // Check ship health
                    CheckHealth();
                    // Boss logic
                    BossLogic(gameTime);
                    // Check to see if the game should spawn enemies
                    SpawnEnemies(gameTime);
                    // Check to see if the game should spawn powerups
                    SpawnPowerUps();
                    // Update bullets and enemy positions
                    UpdatePosition();
                    // Update all of the hitboxes
                    UpdateHitboxes();
                    // Check the player input and do stuff accordingly
                    CheckPlayerInput();
                    // Check boundaries on ship, enemies and bullet
                    CheckBounds();
                    break;
                }
                // On Gameover
                case gameState.GameOver:
                {
                    OnPaused(gameTime);
                    break;
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkOrchid);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            _spriteBatch.Draw(MainShip.Texture, MainShip.Pos, MainShip.Color);
            MainShip.Color = Color.White;
            switch (GameState)
            {
                // Paused/next level
                case gameState.Paused:
                {
                    if (MainShip.Lives <= 0) _spriteBatch.DrawString(MenuFont, "Press Enter to start!\nUse Arrow Keys to move\nSpacebar to shoot",
                        new Vector2(Window.ClientBounds.Width / 12f, 50), Color.MediumVioletRed);
                    else _spriteBatch.DrawString(MenuFont, "Congratulations!\nYou have beaten the boss!\nPress Enter\nto proceed to level " + (MainShip.CurrentLevel + 1),
                        new Vector2(Window.ClientBounds.Width / 12f, 50), Color.Green);
                    break;
                }
                // Running
                case gameState.Running:
                {
                    // Draw information like points, lives and current level
                    _spriteBatch.DrawString(GameFont, "Points: " + MainShip.Points, new Vector2(10, 10), Color.Purple);
                    _spriteBatch.DrawString(GameFont, "Lives: " + MainShip.Lives, new Vector2(10, 30), Color.PaleVioletRed);
                    _spriteBatch.DrawString(GameFont, "Level: " + MainShip.CurrentLevel, new Vector2(10, 50), Color.LimeGreen);
                    // Draw entities
                    foreach (var b in Entities.Bullets) _spriteBatch.Draw(Textures.Bullet, b.Pos, Color.White);
                    foreach (var e in Entities.Enemies) _spriteBatch.Draw(Textures.Enemy, e.Pos, Color.White);
                    foreach (var p in Entities.PowerUps) _spriteBatch.Draw(Textures.PowerUp, p.Pos, Color.White);
                    foreach (var b in Entities.Bosses) _spriteBatch.Draw(Textures.Boss, b.Pos, Color.White);
                    break;
                }
                // Game Over
                case gameState.GameOver:
                {
                    _spriteBatch.DrawString(MenuFont, "Game Over!\nPress Enter to restart\nYour points: " + MainShip.Points, new Vector2(Window.ClientBounds.Width / 12f, 50), Color.Red);
                    break;
                }
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}