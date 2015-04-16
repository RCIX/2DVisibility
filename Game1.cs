using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using XNAHelpers;

namespace _2DVisbility
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const int TileSize = 16;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        InputHelper input;
        Player player;
        Level level;
        public LineDrawer DebugLineDrawer { get; set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;
            this.IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            input = new InputHelper();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            level = new Level(this);
            player = new Player(input, Content, level);
            level.Player = player;
            DebugLineDrawer = new LineDrawer(GraphicsDevice, spriteBatch);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            input.Update();
            if (input.ExitRequested)
                this.Exit();

            if (input.IsNewPress(Keys.Q))
            {
                Player.VisionRadius += 1;
            }
            if (input.IsNewPress(Keys.A))
            {
                Player.VisionRadius -= 1;
            } 
            if (input.IsNewPress(Keys.W))
            {
                Player.ConeWidth += (5f / 180f) * (float)Math.PI;
            }
            if (input.IsNewPress(Keys.S))
            {
                Player.ConeWidth -= (5f / 180f) * (float)Math.PI;
            }



            level.PlayerOrientation = input.MousePosition - (player.Position * Game1.TileSize);
            player.Update();
            level.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.FrontToBack, SaveStateMode.None);
            level.Draw(spriteBatch);
            player.Draw(spriteBatch);
            spriteBatch.End();
            DebugLineDrawer.QueueLine(new Line2D
            {
                Start = player.Position * TileSize,
                End = input.MousePosition,
                Color = Color.Yellow
            });
            DebugLineDrawer.Draw();
            level.HasDrawn = false;
    
            base.Draw(gameTime);
        }
    }
}
