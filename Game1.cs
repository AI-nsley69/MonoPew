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

        // Initialze bool for gameover/menu
        private bool isPaused = true;

        // Initialize the ship
        private Ship MainShip = new Ship();
        private PowerUpLogic PowerUpLogic = new PowerUpLogic();
        private EnemyLogic EnemyLogic = new EnemyLogic();

        // Initialize fonts
        private SpriteFont GameFont;
        private SpriteFont MenuFont;

        // Initialize entity lists
        /*
        private List<Bullet> Bullets = new List<Bullet>();
        private List<PowerUp> PowerUps = new List<PowerUp>();
        private List<Enemy> Enemies = new List<Enemy>();
        private List<Boss> Bosses = new List<Boss>(); */
        // index: bullet, powerup, enemy, bosses

        private (List<Bullet> Bullets, List<PowerUp> PowerUps, List<Enemy> Enemies, List<Boss> Bosses) Entities = (
            new List<Bullet>(),
            new List<PowerUp>(),
            new List<Enemy>(), 
            new List<Boss>()
        );

        // Intialize entity textures
        private Textures Textures = new Textures();
        
        // Initialize sounds
        private Sounds Sounds = new Sounds();

        // Initialize random variable
        private Random Rand = new Random();


        private void RemoveFrom<T>(List<T> list, T element) 
        {
            for (int i = list.Count; i-- > 0;)
            {
                if (!list[i].Equals(element)) continue;
                list.RemoveAt(i);
                break;
            }
        }
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        private void SetSpawnPoint()
        {
            MainShip.Pos = new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 1.5f);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 900;
            _graphics.PreferredBackBufferHeight = 500;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
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

        private void SummonBullet()
        {
            if (MainShip.BulletCooldown > 0) return;
            var bulletPos = new Vector2(MainShip.Pos.X + (MainShip.Texture.Width / 2), MainShip.Pos.Y + Textures.Bullet.Height);
            Entities.Bullets.Add(new Bullet(bulletPos));
            MainShip.BulletCooldown = 15;
            Sounds.Shoot.Play();
        }

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
            
            foreach (var e in Entities.Enemies.ToList()
                         .Where(e => !(e.Pos.Y < Window.ClientBounds.Height - Textures.Enemy.Height)))
            {
                Entities.Enemies.Remove(e);
                MainShip.Lives -= 1;
                MainShip.Color = Color.Red;
            }

            foreach (var b in Entities.Bullets.ToList().Where(b => !(b.Pos.Y > 0)))
            {
                Entities.Bullets.Remove(b);
            }

            Entities.Bosses.ForEach(b =>
            {
                if (b.Pos.X < 0) b.Speed.X = 7f + MainShip.CurrentLevel;
                else if (b.Pos.X > Window.ClientBounds.Width - Textures.Boss.Width) b.Speed.X = -7f - MainShip.CurrentLevel;
            });
        }

        private bool CheckCollision(Rectangle ship, Rectangle other)
        {
            return ship.Intersects(other);
        }

        private void UpdateHitboxes()
        {
            // Update hitbox on its own to prevent unnecessary calculations
            MainShip.UpdateHitbox();
            Entities.Bullets.ForEach(b => b.UpdateHitbox(Textures.Bullet));
            Entities.Enemies.ForEach(e => e.UpdateHitbox(Textures.Enemy));
            Entities.PowerUps.ForEach(p => p.UpdateHitbox(Textures.PowerUp));
            Entities.Bosses.ForEach(b => b.UpdateHitbox(Textures.Boss));
            // Check if ship collides with powerup
            for (var i = Entities.PowerUps.Count; i-- > 0;)
            {
                var p = Entities.PowerUps[i];
                if (!CheckCollision(MainShip.Hitbox, p.Hitbox)) continue;
                RemoveFrom(Entities.PowerUps, p);
                MainShip.Points += 10 * MainShip.CurrentLevel;
                if (MainShip.Lives < 3) MainShip.Lives += 1;
            }
            /*
            foreach (var p in Entities.PowerUps.ToList())
            {
                if (CheckCollision(MainShip.Hitbox, p.Hitbox)) continue;
                Entities.PowerUps.Remove(p);
                MainShip.Points += 10 * MainShip.CurrentLevel;
                if (MainShip.Lives < 3) MainShip.Lives += 1;
            } */
            // Check if bullet hits boss or enemy
            for (var i = Entities.Bullets.Count; i-- > 0;)
            {
                var b = Entities.Bullets[i];
                for (var n = Entities.Enemies.Count; n-- > 0;)
                {
                    var e = Entities.Enemies[n];
                    if (!CheckCollision(b.Hitbox, e.Hitbox)) continue;
                    RemoveFrom(Entities.Bullets, b);
                    RemoveFrom(Entities.Enemies, e);
                    MainShip.Points += 50 * MainShip.CurrentLevel;
                }

                for (var n = Entities.Bosses.Count; n-- > 0;)
                {
                    var boss = Entities.Bosses[n];
                    if (!CheckCollision(b.Hitbox, boss.Hitbox)) continue;
                    RemoveFrom(Entities.Bullets, b);
                    boss.HitPoints -= 1;
                }
            }
            // Check if enemy collides with ship
            for (var i = Entities.Enemies.Count; i-- > 0;)
            {
                var e = Entities.Enemies[i];
                if (!CheckCollision(MainShip.Hitbox, e.Hitbox)) continue;
                RemoveFrom(Entities.Enemies, e);
                MainShip.Lives -= 1;
            }
            // Check if boss collides with ship
            for (var i = Entities.Bosses.Count; i-- > 0;)
            {
                var b = Entities.Bosses[i];
                if (!CheckCollision(MainShip.Hitbox, b.Hitbox)) continue;
                MainShip.Lives -= 1;
            }
        }

        private void UpdateBulletEnemyPositionAbstractFactoryLocalizerInstanceMethodAbstraction()
        {
            MainShip.BulletCooldown -= 1;
            Entities.Bullets.ForEach(b => b.Pos += b.Speed);
            Entities.Enemies.ForEach(e => e.Pos += EnemyLogic.Speed);
        }

        private void OnPaused(GameTime gameTime)
        {
            // Remove all entities
            Entities.Bullets.ToList().ForEach(b => RemoveFrom(Entities.Bullets, b));
            Entities.Enemies.ToList().ForEach(e => RemoveFrom(Entities.Enemies, e));
            Entities.PowerUps.ToList().ForEach(p => RemoveFrom(Entities.PowerUps, p));
            Entities.Bosses.ToList().ForEach(b => RemoveFrom(Entities.Bosses, b));
            // Check if enter was pressed
            var keyboardState = Keyboard.GetState();
            if (!keyboardState.IsKeyDown(Keys.Enter)) return;
            isPaused = false;
            // Mainship variables
            if (MainShip.Lives <= 0)
            {
                MainShip.Lives = 3;
                MainShip.CurrentLevel = 1;
                MainShip.Points = 0;
            }
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
            if (MainShip.Lives <= 0) isPaused = true;
        }

        private void SummonBoss(GameTime gameTime)
        {
            var tmpPos = new Vector2(Window.ClientBounds.Width / 2 + Textures.Boss.Width, 30);
            Entities.Bosses.Add(new Boss(tmpPos, 1 + 2 * MainShip.CurrentLevel));
            Entities.Bosses[0].Speed.X = 7f + MainShip.CurrentLevel;
            EnemyLogic.LastSpawned = gameTime.TotalGameTime;
        }

        private void BossLogic(GameTime gameTime)
        {
            if (!(EnemyLogic.Spawned >= EnemyLogic.BossRequirement)) return;
            if (!(Entities.Bosses.Count > 0)) SummonBoss(gameTime);
            if (Entities.Bosses[0].HitPoints <= 0)
            {
                isPaused = true;
                MainShip.CurrentLevel += 1;
            }
            Entities.Bosses[0].Pos += Entities.Bosses[0].Speed;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            CheckHealth();
            
            if (isPaused)
            {
                OnPaused(gameTime);
                return;
            }
            
            // Boss logic
            BossLogic(gameTime);
            // Check to see if the game should spawn enemies
            SpawnEnemies(gameTime);
            // Check to see if the game should spawn powerups
            SpawnPowerUps();
            // Update bullets and enemy positions
            UpdateBulletEnemyPositionAbstractFactoryLocalizerInstanceMethodAbstraction();
            // Update all of the hitboxes
            UpdateHitboxes();
            // Check the player input and do stuff accordingly
            CheckPlayerInput();
            // Check boundaries on ship, enemies and bullet
            CheckBounds();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkOrchid);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            _spriteBatch.Draw(MainShip.Texture, MainShip.Pos, MainShip.Color);
            MainShip.Color = Color.White;
            if (isPaused)
            {
                if (MainShip.Lives < 1)
                {
                    _spriteBatch.DrawString(MenuFont,
                        MainShip.CurrentLevel == 1 ? "Press Enter to start!\nUse Arrow Keys to move\nUse Spacebar to shoot" : "Game Over!\nPress Enter to restart\nPoints: " + MainShip.Points,
                        new Vector2(Window.ClientBounds.Width / 10f, 50), Color.Red);
                }
                else
                {
                    _spriteBatch.DrawString(MenuFont, "Congratulations!\nYou have beaten the boss!\nPress Enter\nto proceed to level " + MainShip.CurrentLevel,
                        new Vector2(Window.ClientBounds.Width / 12f, 50), Color.Green);
                }

            }
            else
            {
                _spriteBatch.DrawString(GameFont, "Points: " + MainShip.Points, new Vector2(10, 10), Color.Purple);
                _spriteBatch.DrawString(GameFont, "Lives: " + MainShip.Lives, new Vector2(10, 30), Color.PaleVioletRed);
                _spriteBatch.DrawString(GameFont, "Level: " + MainShip.CurrentLevel, new Vector2(10, 50), Color.LimeGreen);
            }
            // Draw entities
            Entities.Bullets.ForEach(b => _spriteBatch.Draw(Textures.Bullet, b.Pos, Color.White));
            Entities.Enemies.ForEach(e => _spriteBatch.Draw(Textures.Enemy, e.Pos, Color.White));
            Entities.PowerUps.ForEach(p => _spriteBatch.Draw(Textures.PowerUp, p.Pos, Color.White));
            Entities.Bosses.ForEach(b => _spriteBatch.Draw(Textures.Boss, b.Pos, Color.White));
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}