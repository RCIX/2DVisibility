using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNAHelpers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace _2DVisbility
{
    class Player
    {
        public static int VisionRadius = 15;
        public static float ConeWidth = (130f / 180f) * (float)Math.PI;

        public Vector2 Position
        {
            get
            {
                return _position + new Vector2(0.5f);
            }
            set
            {
                _position = value;
            }
        }
        private Vector2 _position;
        private InputHelper _input;
        private Texture2D _playerTexture;
        private Level _level;

        public Player(InputHelper input, ContentManager content, Level level)
        {
            _level = level;
            _input = input;
            _position = new Vector2(10);
            _playerTexture = content.Load<Texture2D>("Player");
        }

        public void Update()
        {
            Vector2 oldPos = _position;
            if (_input.IsCurPress(Keys.Up))
            {
                _position.Y -= 0.15f;
                _position.X += GetCorrectionVector(_position.X);
            }
            if (_input.IsCurPress(Keys.Left))
            {
                _position.X -= 0.15f;
                _position.Y += GetCorrectionVector(_position.Y);
            }
            if (_input.IsCurPress(Keys.Right))
            {
                _position.X += 0.15f;
                _position.Y += GetCorrectionVector(_position.Y);
            }
            if (_input.IsCurPress(Keys.Down))
            {
                _position.Y += 0.15f;
                _position.X += GetCorrectionVector(_position.X);
            }

            if (IsCollidingWithWall())
            {
                _position = oldPos;
            }
        }

        private float GetCorrectionVector(float number)
        {
            float decimalOfNumber = (float)Math.Abs(number - Math.Truncate(number));
            float targetNumber = (float)Math.Round(number * 2) / 2f;
            return MathHelper.Clamp(targetNumber - number, -0.05f, 0.05f);
        }

        private bool IsCollidingWithWall()
        {
            int xMinPos = (int)MathFunctions.RoundDownTo(_position.X, 1);
            int yMinPos = (int)MathFunctions.RoundDownTo(_position.Y, 1);
            int xMaxPos = (int)MathFunctions.RoundDownTo(_position.X + 1f, 1);
            int yMaxPos = (int)MathFunctions.RoundDownTo(_position.Y + 1f, 1);
            if (_level.GetCellIsOpaque(new Vector2(xMinPos, yMinPos)))
            {
                return true;
            }
            if (_level.GetCellIsOpaque(new Vector2(xMaxPos, yMinPos)))
            {
                return true;
            }
            if (_level.GetCellIsOpaque(new Vector2(xMinPos, yMaxPos)))
            {
                return true;
            }
            if (_level.GetCellIsOpaque(new Vector2(xMaxPos, yMaxPos)))
            {
                return true;
            }
            return false;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_playerTexture, _position * Game1.TileSize, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
        }
    }
}
