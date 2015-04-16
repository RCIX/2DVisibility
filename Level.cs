using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAHelpers;
using Microsoft.Xna.Framework.Content;

namespace _2DVisbility
{
    class Level
    {
        private bool[,] _cellsAreInvisible;
        private bool[,] _cellsAreOpaque;
        private bool[,] _cellsAreDiscovered;

        private Texture2D _invisibleTexture;
        private Texture2D _visibleTexture;
        private Texture2D _opaqueTexture;

        private Vector2 _levelSize;

        private Game1 _game;

        public Player Player { get; set; }
        public Vector2 PlayerOrientation { get; set; }
        public bool HasDrawn { get; set; }

        public Level(Game1 game)
        {
            Texture2D levelInfo = game.Content.Load<Texture2D>("Level");
            Color[] levelData = new Color[levelInfo.Width * levelInfo.Height];
            levelInfo.GetData<Color>(levelData);
            PlayerOrientation = new Vector2(-5);

            _game = game;

            _levelSize = new Vector2(levelInfo.Width, levelInfo.Height);

            _cellsAreInvisible = new bool[(int)_levelSize.X, (int)_levelSize.Y];
            _cellsAreOpaque = new bool[(int)_levelSize.X, (int)_levelSize.Y];
            _cellsAreDiscovered = new bool[(int)_levelSize.X, (int)_levelSize.Y];


            for (int x = 0; x < levelInfo.Width; x++)
            {
                for (int y = 0; y < levelInfo.Height; y++)
                {
                    if (levelData[x + y * levelInfo.Width] == Color.White)
                    {
                        _cellsAreOpaque[x, y] = true;
                    }
                }
            }

            _invisibleTexture = game.Content.Load<Texture2D>("Invisible");
            _visibleTexture = game.Content.Load<Texture2D>("Visible");
            _opaqueTexture = game.Content.Load<Texture2D>("Opaque");
        }

        public void Update()
        {
            ResetAllCells();
            List<Vector2> validCells = GetAllValidCells(Player.Position);
            foreach (Vector2 cell in validCells)
            {
                List<Vector2> intersectingCells = GetCellsIntersectingWithLine(Player.Position, cell);
                List<Vector2> solidIntersections = intersectingCells.Where((testCell) => _cellsAreOpaque[(int)testCell.X, (int)testCell.Y]).ToList();

                Vector2 targetLine = cell - Player.Position;
                targetLine.Normalize();
                Vector2 normalizedOrientation = PlayerOrientation;
                normalizedOrientation.Normalize();
                float playerRadians = (float)Math.Atan2(normalizedOrientation.Y, normalizedOrientation.X);
                float coneLeftRadians = playerRadians - (Player.ConeWidth / 2);
                float coneRightRadians = playerRadians + (Player.ConeWidth / 2);

                float targetRadians = (float)Math.Atan2(targetLine.Y, targetLine.X);

                bool isInLoS = PlayerOrientation != new Vector2(-5) &&
                    MathFunctions.Wrap(targetRadians - playerRadians, -(float)Math.PI, (float)Math.PI) < Player.ConeWidth / 2 &&
                    MathFunctions.Wrap(targetRadians - playerRadians, -(float)Math.PI, (float)Math.PI) > -(Player.ConeWidth / 2);

                if (!HasDrawn)
                {
                    Vector2 coneLeftVector = new Vector2(
                        (float)Math.Cos(coneLeftRadians),
                        (float)Math.Sin(coneLeftRadians)) * Player.VisionRadius;
                    Vector2 coneRightVector = new Vector2(
                        (float)Math.Cos(coneRightRadians),
                        (float)Math.Sin(coneRightRadians)) * Player.VisionRadius;
                    _game.DebugLineDrawer.QueueLine(new Line2D
                    {
                        Start = (Player.Position * Game1.TileSize),
                        End = (Player.Position + coneLeftVector) * Game1.TileSize,
                        Color = Color.Yellow,
                    });
                    _game.DebugLineDrawer.QueueLine(new Line2D
                    {
                        Start = (Player.Position * Game1.TileSize),
                        End = (Player.Position + coneRightVector) * Game1.TileSize,
                        Color = Color.Yellow,
                    });
                    HasDrawn = true;
                }

                if (solidIntersections.Count > 0 &&
                    !(solidIntersections.Count == 1 && solidIntersections[0] == cell))
                {
                    _cellsAreInvisible[(int)cell.X, (int)cell.Y] = true;
                }
                else if (isInLoS)
                {
                    _cellsAreInvisible[(int)cell.X, (int)cell.Y] = false;
                    _cellsAreDiscovered[(int)cell.X, (int)cell.Y] = true;
                }
            }
            //perform visibility calcs here
        }

        private void ResetAllCells()
        {
            for (int x = 0; x < _levelSize.X; x++)
            {
                for (int y = 0; y < _levelSize.Y; y++)
                {
                    _cellsAreInvisible[x, y] = true;
                }
            }
        }

        private List<Vector2> GetAllValidCells(Vector2 basePosition)
        {
            List<Vector2> retList = new List<Vector2>();

            for (int x = 0; x < _levelSize.X; x++)
            {
                for (int y = 0; y < _levelSize.Y; y++)
                {
                    if ((new Vector2(x + 0.5f, y + 0.5f) - basePosition).Length() < Player.VisionRadius)
                    {
                        retList.Add(new Vector2(x, y));
                    }
                }
            }
            return retList;
        }

        //Bresenham's line algorithm
        private List<Vector2> GetCellsIntersectingWithLine(Vector2 start, Vector2 end)
        {
            List<Vector2> retList = new List<Vector2>();

            bool lineIsSteep = Math.Abs(end.Y - start.Y) > Math.Abs(end.X - start.X);
            if (lineIsSteep) //swap x and y to make line not steep
            {
                Vector2 temp = start;
                temp.X = start.Y;
                temp.Y = start.X;
                start = temp;

                temp = end;
                temp.X = end.Y;
                temp.Y = end.X;
                end = temp;
            }
            //swap start and end
            if (start.X > end.X)
            {
                Vector2 temp = end;
                end = start;
                start = temp;
            }
            int deltaX = (int)(end.X - start.X);
            int deltaY = (int)Math.Abs(end.Y - start.Y);

            int error = deltaX / 2;

            int yStep = start.Y < end.Y ? 1 : -1;
            int y = (int)start.Y;

            for (int x = (int)start.X; x < end.X; x++)
            {
                if (lineIsSteep)
                {
                    retList.Add(new Vector2(y, x));
                }
                else
                {
                    retList.Add(new Vector2(x, y));
                }
                error -= deltaY;
                if (error < 0)
                {
                    y += yStep;
                    error += deltaX;
                }
            }
            return retList;
        }

        public void Draw(SpriteBatch batch)
        {
            for (int x = 0; x < _levelSize.X; x++)
            {
                for (int y = 0; y < _levelSize.Y; y++)
                {
                    Texture2D drawTex;

                    if (_cellsAreInvisible[x, y])
                    {
                        drawTex = _invisibleTexture;
                    }
                    else
                    {
                        drawTex = _visibleTexture;
                    }

                    if (_cellsAreOpaque[x, y])
                    {
                        drawTex = _opaqueTexture;
                    }

                    if (_cellsAreDiscovered[x, y])
                    {
                        batch.Draw(drawTex, new Vector2(x * Game1.TileSize, y * Game1.TileSize), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public bool GetCellIsOpaque(Vector2 position)
        {
            return _cellsAreOpaque[(int)position.X, (int)position.Y];
        }
    }
}
