using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Reflection.Metadata.BlobBuilder;

namespace PixelGame
{
    public class Game1 : Game
    {
        #region Variable Definition

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Random random = new Random();
        uint gameTick;

        //Textures
        Texture2D Texture_Default;

        //World / Tiles
        int TileWidth;
        int TileHeight;
        int WorldWidth;
        int WorldHeight;
        private List<List<Tile>> World = new List<List<Tile>>();

        //Screen
        int ScreenWidth;
        int ScreenHeight;

        //Camera
        int CameraOffset_X;
        int CameraOffset_Y;
        int CameraLoadBound_X_Left;
        int CameraLoadBound_X_Right;
        int CameraLoadBound_Y_Left;
        int CameraLoadBound_Y_Right;

        //Player
        Player Player = new Player();

        #endregion

        #region Initialize

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1800;
            _graphics.PreferredBackBufferHeight = 1000;
            _graphics.ApplyChanges();

            IsFixedTimeStep = true;//false;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d); //60);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            gameTick = 0;


            WorldWidth = 200;
            WorldHeight = 200;
            TileWidth = 16;
            TileHeight = 16;

            ScreenWidth = _graphics.PreferredBackBufferWidth / TileWidth;
            ScreenHeight = _graphics.PreferredBackBufferHeight / TileHeight;
            CameraOffset_X = 0;
            CameraOffset_Y = 0;

            Player = new Player()
            {
                Width = 160, // 32
                Height = 160, // 48

                Health = 100,
                Health_Max = 200,

                Breath = 1000,
                Breath_Max = 1000,

                JumpHeight = 12,
                Speed_Base = 1 / 3,
                Speed_Max = 1 / 2
            };
            Player.x = (_graphics.PreferredBackBufferWidth / 2) - (Player.Width / 2);
            Player.y = (_graphics.PreferredBackBufferHeight / 2) - (Player.Height / 2);


            Camera_SetOffset();
            GenerateMap();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture_Default = Content.Load<Texture2D>("Texture_Default");
        }

        #endregion

        /* Block Type Ids | Variable Types
         * 
         *  BLOCK IDs
         * null == air
         * 0 == Barrier
         * 1 == Default
         * 2 == Dirt
         * 3 == Grass
         * 4 == Stone
         * 5 == Sand
         * 6 == Water
         * 
         *  INTEGRAL VERIABLE TYPES
         * sbyte: -128 to 127
         * byte:     0 to 255
         * short: -32768 to 32767
         * ushort:     0 to 65645
         * int: -2,147,483,648 to 2,147,483,647
         * uint:             0 to 4,294,967,295
         * long: -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807
         * 
        */

        /////////////////////////////////////////

        #region Terrain Generation

        private void GenerateMap()
        {
            GenerateMap_Base();
            GenerateMap_Border();
        }

        private void GenerateMap_Base()
        {
            for (int y = 0; y < WorldHeight; y++)
            {
                World.Add(new List<Tile>());
                for (int x = 0; x < WorldWidth; x++)
                {
                    World[y].Add(null);
                }
            }
        }
        private void GenerateMap_Border()
        {
            ushort BorderWidth = 20;
            for (ushort y = 0; y < World.Count(); y++)
            {
                for (ushort x = 0; x < World[0].Count(); x++)
                {
                    if ((x < BorderWidth || x > World[0].Count() - BorderWidth - 1 ||
                        y < BorderWidth || y > World[0].Count() - BorderWidth - 1) &&
                        random.Next(0, 15) != 0)
                    {
                        World[y][x] = new Tile()
                        {
                            Type = 1, //Default
                            TextureTag = 0,
                            WallType = 0, //none
                            WallTextureTag = 0
                        };
                    }
                }
            }
        }

        #endregion

        /////////////////////////////////////////

        #region Collision Detection

        private int GetPhysicsType(int BlockId)
        {
            //Liquids: 1
            //Solids: 2

            if (BlockId == -1)
            {
                return 0;
            }
            if (BlockId == 6)
            {
                //return 1;
            }
            return 2;
        }

        private int CheckPointCollision(int x, int y)
        {
            if (World[y / TileHeight][x / TileWidth] != null)
            {
                return (int)(World[y / TileHeight][x / TileWidth].Type);
            }

            return -1;
        }
        private int CheckLineCollision(int x1, int y1, int x2, int y2, int CheckDistance)
        {
            int CollisionType = -1;

            //Horizontal Line
            if (x1 != x2)
            {
                for (int x = x1; x < x2; x += CheckDistance)
                {
                    if (GetPhysicsType(CheckPointCollision(x, y1)) > CollisionType)
                    {
                        CollisionType = CheckPointCollision(x, y1);
                    }
                }
                if ((x1 - x2) % CheckDistance != 0)
                {
                    if (GetPhysicsType(CheckPointCollision(x2, y1)) > CollisionType)
                    {
                        CollisionType = CheckPointCollision(x2, y1);
                    }
                }
            }

            //Vertical Line
            else if (y1 != y2)
            {
                for (int y = y1; y < y2; y += CheckDistance)
                {
                    if (GetPhysicsType(CheckPointCollision(x1, y)) > CollisionType)
                    {
                        CollisionType = CheckPointCollision(x1, y);
                    }
                }
                if ((y1 - y2) % CheckDistance != 0)
                {
                    if (GetPhysicsType(CheckPointCollision(x1, y2)) > CollisionType)
                    {
                        CollisionType = CheckPointCollision(x1, y2);
                    }
                }
            }

            return CollisionType;
        }
        private (int, int) CheckCubeCollision(int x1, int y1, int x2, int y2, string Direction, int CubeCheckDistance)
        {
            int Distance = 0;
            int CollisionType = -1;
            int CollisionToCheck = -1;


            for (int Offset = 0; Offset < CubeCheckDistance; Offset++)
            {
                if (Direction == "Down")
                {
                    CollisionToCheck = CheckLineCollision(x1, y1 + Offset, x2, y2 + Offset, TileWidth);
                }
                else if (Direction == "Up")
                {
                    CollisionToCheck = CheckLineCollision(x1, y1 - Offset, x2, y2 - Offset, TileWidth);
                }
                else if (Direction == "Left")
                {
                    CollisionToCheck = CheckLineCollision(x1 - Offset, y1, x2 - Offset, y2, TileWidth);
                }
                else if (Direction == "Right")
                {
                    CollisionToCheck = CheckLineCollision(x1 + Offset, y1, x2 + Offset, y2, TileWidth);
                }

                if (GetPhysicsType(CollisionToCheck) == 2)
                {
                    return (Distance, CollisionToCheck);
                }

                if (GetPhysicsType(CollisionToCheck) > GetPhysicsType(CollisionType))
                {
                    CollisionType = CollisionToCheck;
                }

                Distance++;
            }

            return (Distance, CollisionType);
        }

        #endregion

        #region Camera

        private void Execute_BlockLoadBoundary()
        {
            //Left
            //X
            if (CameraOffset_X < 0)
            {
                CameraLoadBound_X_Left = 0;
            }
            else
            {
                CameraLoadBound_X_Left = CameraOffset_X / TileWidth;
            }
            //Y
            if (CameraOffset_Y < 0)
            {
                CameraLoadBound_Y_Left = 0;
            }
            else
            {
                CameraLoadBound_Y_Left = CameraOffset_Y / TileHeight;
            }


            //Right
            //X
            if (CameraOffset_X / TileWidth + ScreenWidth - 1 > World[0].Count() - 3)
            {
                CameraLoadBound_X_Right = World[0].Count() - 1;
            }
            else
            {
                CameraLoadBound_X_Right = CameraOffset_X / TileWidth + ScreenWidth + 2;

            }
            //Y
            if (CameraOffset_Y / TileHeight + ScreenHeight - 1 > World.Count() - 3)
            {
                CameraLoadBound_Y_Right = World.Count() - 1;
            }
            else
            {
                CameraLoadBound_Y_Right = CameraOffset_Y / TileHeight + ScreenHeight + 2;
            }
        }

        private void Camera_SetOffset()
        {
            CameraOffset_X = Player.x - ((_graphics.PreferredBackBufferWidth - Player.Width) / 2);
            CameraOffset_Y = Player.y - ((_graphics.PreferredBackBufferHeight - Player.Height) / 2);
        }

        #endregion

        #region Player Movement

        private void Execute_PlayerMomentum_Vertical()
        {
            //Collision type directly bellow player is checked. If there is nothing, the players vertical momentum increases (downward)
            int GravityEffectInterval = 2;

            if (gameTick % GravityEffectInterval == 0)
            {
                if (GetPhysicsType(CheckLineCollision(Player.x, Player.y + Player.Height + 1,
                                                    Player.x + Player.Width, Player.y + Player.Height + 1,
                                                    TileWidth)) == 0)
                {
                    Player.Momentum_Vertical += 1;
                }
            }

            // Going Down
            if (Player.Momentum_Vertical > 0)
            {
                (int, int) CollisionDetails = CheckCubeCollision(Player.x, Player.y + Player.Height,
                                                                    Player.x + Player.Width, Player.y + Player.Height,
                                                                    "Down", Player.Momentum_Vertical);

                Player.y += CollisionDetails.Item1;
                CameraOffset_Y += CollisionDetails.Item1;

                // Solid
                if (GetPhysicsType(CollisionDetails.Item2) == 2)
                {
                    Player.Momentum_Vertical = 0;
                }
            }
            
            // Going Up
            else if (Player.Momentum_Vertical < 0)
            {
                (int, int) CollisionDetails = CheckCubeCollision(Player.x, Player.y - 1,
                                                                    Player.x + Player.Width, Player.y - 1,
                                                                    "Up", -Player.Momentum_Vertical);

                Player.y -= CollisionDetails.Item1;
                CameraOffset_Y -= CollisionDetails.Item1;

                // Solid
                if (GetPhysicsType(CollisionDetails.Item2) == 2)
                {
                    Player.Momentum_Vertical = 0;
                }
            }
        }
        private void Execute_PlayerMomentum_Horizontal()
        {
            // Going Left
            if (Player.Momentum_Horizontal < 0)
            {
                (int, int) CollisionDetails = CheckCubeCollision(Player.x - 1, Player.y,
                                                                    Player.x - 1, Player.y + Player.Height,
                                                                    "Left", -Player.Momentum_Horizontal);

                Player.x -= CollisionDetails.Item1;
                CameraOffset_X -= CollisionDetails.Item1;

                //Soid
                if (GetPhysicsType(CollisionDetails.Item2) == 2)
                {
                    Player.Momentum_Horizontal = 0;
                }
            }
            //Right
            if (Player.Momentum_Horizontal > 0)
            {
                (int, int) CollisionDetails = CheckCubeCollision(Player.x + Player.Width + 1, Player.y,
                                                                    Player.x + Player.Width + 1, Player.y + Player.Height,
                                                                    "Right", Player.Momentum_Horizontal);

                Player.x -= CollisionDetails.Item1;
                CameraOffset_X -= CollisionDetails.Item1;

                //Soid
                if (GetPhysicsType(CollisionDetails.Item2) == 2)
                {
                    Player.Momentum_Horizontal = 0;
                }
            }
        }
        private void PlayerMovementHandler()
        {
            Execute_PlayerMomentum_Vertical();
            Execute_PlayerMomentum_Horizontal();
        }

        #endregion

        /////////////////////////////////////////

        #region Fundamentals

        private Rectangle getRect(int x, int y)
        {
            //return new Rectangle((int)(x * TileWidth - Player.x), (int)(y * TileHeight - Player.y), TileWidth, TileHeight);
            return new Rectangle((x * TileWidth) - CameraOffset_X, (y * TileHeight) - CameraOffset_Y, TileWidth, TileHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            PlayerMovementHandler();
            Execute_BlockLoadBoundary();

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                Player.Momentum_Vertical = -TileHeight;
            }

            gameTick++;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightSkyBlue);

            // BEGIN Draw ----
            _spriteBatch.Begin();

            //Tiles
            for (int y = CameraLoadBound_Y_Left; y < CameraLoadBound_Y_Right; y++)
            {
                for (int x = CameraLoadBound_X_Left; x < CameraLoadBound_X_Right; x++)
                {
                    if (World[y][x] != null)
                    {
                        if (World[y][x].Type == 1) //Default
                        {
                            if (World[y][x].TextureTag == 0)
                            {
                                _spriteBatch.Draw(Texture_Default, getRect(x, y), Color.White);
                            }
                        }
                    }
                }
            }

            //Player
            _spriteBatch.Draw(Texture_Default, new Rectangle(Player.x - CameraOffset_X, Player.y - CameraOffset_Y, Player.Width, Player.Height), Color.White);

            _spriteBatch.End();
            // END Draw ------

            base.Draw(gameTime);
        }

        #endregion
    }
}