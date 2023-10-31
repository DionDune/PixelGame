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

        string GameState;
        List<UIPage> UIPages;
        UIPage UIPage_Current;

        //Textures
        Texture2D Texture_Default;
        Texture2D Texture_White;

        //Colors
        Color Color_Barrier;
        Color Color_Default;
        Color Color_Grass;
        Color Color_Dirt;
        Color Color_Stone;
        Color Color_Sand;
        Color Color_Water;
        Color Color_Bedrock;

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

        //Keys/Mouse
        List<Keys> Keys_BeingPressed = new List<Keys>();
        bool MouseClicking_Left;

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


            GameState = "Start";

            WorldWidth = 800;
            WorldHeight = 800;
            TileWidth = 16;
            TileHeight = 16;
            //Minimum Tile Dims should be 6*6. Recommonded is 16*16

            ScreenWidth = _graphics.PreferredBackBufferWidth / TileWidth;
            ScreenHeight = _graphics.PreferredBackBufferHeight / TileHeight;
            CameraOffset_X = 0;
            CameraOffset_Y = 0;

            Player = new Player()
            {
                Width = 32, // 32
                Height = 48, // 48

                Health = 100,
                Health_Max = 200,
                HealthRedgainAmount = 5,
                HealthRegainInterval = 5,

                Breath = 1000,
                Breath_Max = 1000,
                BreathRegainAmount = 2,
                BreathRegainInterval = 1,

                JumpHeight_Crouch = 8,
                JumpHeight = 12,
                JumpHeight_Shift = 14,

                Speed_Crouch = 1 / 5f,
                Speed_Base = 1 / 3f,
                Speed_Shift = 1 / 2f,

                IsCrouching = false,
                IsShifting = false
            };
            Player.x = (_graphics.PreferredBackBufferWidth / 2) - (Player.Width / 2);
            Player.y = (_graphics.PreferredBackBufferHeight / 2) - (Player.Height / 2);


            Camera_SetOffset();
            GenerateMap_Main();

            Colors_CreateColours();

            UI_GenPages();
            UIPage_Current = UIPages[0];

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture_Default = Content.Load<Texture2D>("Texture_Default");
            Texture_White = Content.Load<Texture2D>("Colour_White");
        }

        private void Colors_CreateColours()
        {
            Color_Barrier = new Color(255, 0, 0);
            Color_Default = new Color(204, 51, 255);
            Color_Dirt = new Color(166, 74, 43);
            Color_Grass = new Color(0, 153, 0);
            Color_Stone = new Color(140, 140, 140);
            Color_Sand = new Color(255, 187, 51);
            Color_Water = new Color(0, 0, 230);
            Color_Bedrock = new Color(255, 255, 255);
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
         * 7 == BedRock
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

        private void GenerateMap_Main()
        {
            int BorderWidth = 20;

            GenerateMap_Base();
            GenerateMap_Border(BorderWidth);
            GenerateMap_GenerateTerrain(BorderWidth, 50);
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
        private void GenerateMap_Border(int BorderWidth)
        {
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

        private List<int> GenerateMap_GenSurfaceHeights()
        {
            int Height_ChangeAmount = 1;

            var GroundHeights = new List<int>();

            GroundHeights.Add(WorldHeight / 3);

            for (int x_pos = 1; x_pos < WorldWidth - 9; x_pos++)
            {
                int heightChangeDirection = random.Next(0, 5);

                // DOWN
                if (heightChangeDirection == 0 && GroundHeights[x_pos - 1] < WorldHeight)
                {
                    if (random.Next(0, 18) == 0) // Changes by 2
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] + 2 * Height_ChangeAmount);
                    }
                    else // Changes by 1
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] + 1 * Height_ChangeAmount);
                    }
                }

                // UP
                else if (heightChangeDirection == 1 && GroundHeights[x_pos - 1] > 5)
                {
                    if (random.Next(0, 18) == 0) // Changes by 2
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] - 2 * Height_ChangeAmount);
                    }
                    else if (random.Next(0, 112) == 0) // Changes by 5
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] - 5 * Height_ChangeAmount);
                    }
                    else // Changes by 1
                    {
                        GroundHeights.Add(GroundHeights[x_pos - 1] - 1 * Height_ChangeAmount);
                    }
                }

                // SAME
                else
                {
                    GroundHeights.Add(GroundHeights[x_pos - 1]);
                }
            }

            return GroundHeights;
        }
        private List<string> GenerateMap_GenBiomes()
        {
            var BiomeRegions = new List<string>();

            string Biome_Default = "Grass";

            string Biome_Type = Biome_Default;
            int Biome_Left = 0;

            for (int x_pos = 0; x_pos < WorldWidth; x_pos++)
            {
                if (random.Next(0, 600) == 0 && Biome_Type != "Sand")
                {
                    Biome_Type = "Sand";
                    Biome_Left = random.Next(200, 500);
                }

                BiomeRegions.Add(Biome_Type);
                if (Biome_Type != Biome_Default)
                {
                    Biome_Left--;
                    if (Biome_Left == 0)
                    {
                        Biome_Type = Biome_Default;
                    }
                }
            }

            return BiomeRegions;
        }

        private void GenerateMap_GenerateTerrain(int BorderWidth, int BedrockStartDepth)
        {
            List<int> GroundHeights = GenerateMap_GenSurfaceHeights();

            // Gens surface terrain
            for (int x_pos = BorderWidth; x_pos < GroundHeights.Count(); x_pos++)
            {
                byte Block_Type = 3; // Grass
                World[GroundHeights[x_pos]][x_pos] = new Tile() { Type = Block_Type };
            }

            // Gens sub terrain
            for (int y_pos = BorderWidth; y_pos < WorldHeight - BorderWidth; y_pos++)
            {
                for (int x_pos = BorderWidth; x_pos < GroundHeights.Count(); x_pos++)
                {
                    if (y_pos > GroundHeights[x_pos])
                    {
                        byte type = 4; // Rock

                        if (y_pos < GroundHeights[x_pos] + 15 + random.Next(-5, 5))
                        {
                            type = 2; // Dirt
                        }
                        if (y_pos > World.Count() - BedrockStartDepth)
                        {
                            if (random.Next(y_pos, World.Count() - (World.Count() - y_pos) / 2) == y_pos)
                            {
                                type = 7; // Bedrock
                            }
                        }
                        World[y_pos][x_pos] = new Tile() { Type = type };
                    }
                }
            }
        }

        #endregion

        /////////////////////////////////////////

        #region UI

        private void UI_GenPages()
        {
            UIPages = new List<UIPage>();


            //Start Button
            UIItem StartButton = new UIItem()
            {
                Type = "Button",

                X = -200,
                Y = - 75,

                Width = 400,
                Height = 150,

                CentreX = -200 + (400 / 2),
                CentreY = -75 + (150 / 2),

                BorderWidth = 5,
                BorderColor = Color.Green,
                BaseColor = Color.PaleGreen,

                Text = new TextElement()
                {
                    Text = "START NEW",
                    Elements = TextCharacter.GetString("START NEW"),
                    ElementSize = 8,
                    Color = Color.Black
                },

                Data = new List<string>() { "Start New" }
            };
            //Start Quit Button
            UIItem StartQuitButton = new UIItem()
            {
                Type = "Button",

                X = -200,
                Y = -75 + 175,

                Width = 400,
                Height = 150,

                CentreX = -200 + (400 / 2),
                CentreY = 100 + (150 / 2),

                BorderWidth = 5,
                BorderColor = Color.DarkRed,
                BaseColor = Color.Red,

                Text = new TextElement()
                {
                    Text = "QUIT",
                    Elements = TextCharacter.GetString("QUIT"),
                    ElementSize = 8,
                    Color = Color.Black
                },

                Data = new List<string>() { "Quit" }
            };

            //Start Page
            UIPages.Add(new UIPage()
            {
                Type = "Start",

                UIItems = new List<UIItem>() { StartButton, StartQuitButton }
            });


            
            //Resume Button
            UIItem ResumeButton = new UIItem()
            {
                Type = "Button",

                X = -200,
                Y = -275,

                Width = 400,
                Height = 150,

                CentreX = -200 + (400 / 2),
                CentreY = -275 + (150 / 2),

                BorderWidth = 5,
                BorderColor = Color.Green,
                BaseColor = Color.PaleGreen,

                Text = new TextElement()
                {
                    Text = "RESUME",
                    Elements = TextCharacter.GetString("RESUME"),
                    ElementSize = 8,
                    Color = Color.Black
                },

                Data = new List<string>() { "Resume" }
            };
            //Pause Quit Button
            UIItem PauseQuitButton = new UIItem()
            {
                Type = "Button",

                X = -200,
                Y = -100,

                Width = 400,
                Height = 150,

                CentreX = -200 + (400 / 2),
                CentreY = -100 + (150 / 2),

                BorderWidth = 5,
                BorderColor = Color.DarkRed,
                BaseColor = Color.Red,

                Text = new TextElement()
                {
                    Text = "QUIT",
                    Elements = TextCharacter.GetString("QUIT"),
                    ElementSize = 8,
                    Color = Color.Black
                },

                Data = new List<string>() { "Quit" }
            };

            //Pause Page
            UIPages.Add(new UIPage()
            {
                Type = "Pause",

                UIItems = new List<UIItem>() { ResumeButton, PauseQuitButton }
            });



            //Health Bar
            UIItem HealthBar = new UIItem()
            {
                Type = "Fillbar",

                Orientation = "Bottom Left",
                X = 25,
                Y = -100,

                Width = 450,
                Height = 75,

                BorderWidth = 5,
                BorderColor = new Color(210, 0, 0),
                SubBorderColor = Color.White,
                BaseColor = Color.Red,
                BorderTransparency = 0.85F,
                BaseTransparency = 1F,

                MinValue = 0,
                MaxValue = Player.Health_Max,
                Value = Player.Health,

                Data = new List<string>() { "Health" }
            };
            //Breath Bar
            UIItem BreathBar = new UIItem()
            {
                Type = "Fillbar",

                Orientation = "Bottom Left",
                X = 25,
                Y = -150,
                Width = 450,
                Height = 45,

                BorderWidth = 5,
                BorderColor = Color.LightBlue,
                SubBorderColor = Color.White,
                BaseColor = Color.Blue,
                BorderTransparency = 0.75F,
                BaseTransparency = 0.85F,

                MinValue = 0,
                MaxValue = Player.Breath_Max,
                Value = Player.Breath,

                Data = new List<string>() { "Breath" }
            };

            //In Game Page
            UIPages.Add(new UIPage()
            {
                Type = "Play",

                UIItems = new List<UIItem>() { HealthBar, BreathBar }
            });
        }

        private void UI_RenderElements(List<UIItem> UIItems)
        {
            foreach (UIItem Item in UIItems)
            {
                int OrientatePosX = _graphics.PreferredBackBufferWidth / 2;
                int OrientatePosY = _graphics.PreferredBackBufferHeight / 2;
                switch (Item.Orientation)
                {
                    case "Bottom Left":
                        OrientatePosX = 0;
                        OrientatePosY = _graphics.PreferredBackBufferHeight;
                        break;
                    case "Left":
                        OrientatePosX = 0;
                        break;
                    case "Top Left":
                        OrientatePosX = 0;
                        OrientatePosY = 0;
                        break;
                    case "Top":
                        OrientatePosY = 0;
                        break;
                    case "Top Right":
                        OrientatePosX = _graphics.PreferredBackBufferWidth;
                        OrientatePosY = 0;
                        break;
                    case "Right":
                        OrientatePosX = _graphics.PreferredBackBufferWidth;
                        break;
                    case "Bottom Right":
                        OrientatePosX = _graphics.PreferredBackBufferWidth;
                        OrientatePosY = _graphics.PreferredBackBufferHeight;
                        break;
                    case "Bottom":
                        OrientatePosY = _graphics.PreferredBackBufferHeight;
                        break;
                }

                int X = OrientatePosX + Item.X;
                int Y = OrientatePosY + Item.Y;
                int CentreX = OrientatePosX + Item.CentreX;
                int CentreY = OrientatePosY + Item.CentreY;

                if (Item.Type == "Button")
                {
                    _spriteBatch.Draw(Texture_White, new Rectangle(X, Y, Item.Width, Item.Height), Item.BorderColor);
                    if (!Item.Highlighted)
                    {
                        _spriteBatch.Draw(Texture_White, new Rectangle(X + Item.BorderWidth, Y + Item.BorderWidth,
                                                                   Item.Width - Item.BorderWidth * 2, Item.Height - Item.BorderWidth * 2), Item.BaseColor);
                    }
                    else
                    {
                        _spriteBatch.Draw(Texture_White, new Rectangle(X + Item.BorderWidth, Y + Item.BorderWidth,
                                                                   Item.Width - Item.BorderWidth * 2, Item.Height - Item.BorderWidth * 2), Item.HighlightedColor);
                    }
                    
                    if (Item.Text != null)
                    {
                        UI_RenderTextElements(Item.Text.Elements, CentreX, CentreY, Item.Text.ElementSize, Item.Text.Color);
                    }
                }
                if (Item.Type == "Fillbar")
                {
                    //Border
                    _spriteBatch.Draw(Texture_White, new Rectangle(X, Y, Item.Width, Item.Height), Item.BorderColor * Item.BorderTransparency);
                    //Inner
                    _spriteBatch.Draw(Texture_White, new Rectangle(X + Item.BorderWidth, Y + Item.BorderWidth,
                                                                   Item.Width - Item.BorderWidth * 2, Item.Height - Item.BorderWidth * 2), 
                                                                   Item.SubBorderColor * Item.BorderTransparency);
                    //Bar
                    _spriteBatch.Draw(Texture_White, new Rectangle(X + Item.BorderWidth, Y + Item.BorderWidth,
                                                                   (int)((Item.Value - Item.MinValue) / (float)Item.MaxValue * (Item.Width - Item.BorderWidth * 2)),
                                                                   Item.Height - Item.BorderWidth * 2), Item.BaseColor * Item.BaseTransparency);
                }
            }
        }
        private void UI_RenderTextElements(List<List<bool>> Elements, int CentreX, int CentreY, int elementSize, Color elementColor)
        {
            int StartX = CentreX - ((Elements[0].Count * elementSize) / 2);
            int StartY = CentreY - ((Elements.Count * elementSize) / 2);

            for (int y = 0; y < Elements.Count; y++)
            {
                for (int x = 0; x < Elements[0].Count; x++)
                {
                    if (Elements[y][x])
                    {
                        _spriteBatch.Draw(Texture_White, new Rectangle(StartX + (x * elementSize), StartY + (y * elementSize), elementSize, elementSize), elementColor);
                    }
                }
            }
        }
        private void UI_ItemToggleHighlight(UIItem item, bool toHighlight)
        {
            if (toHighlight)
            {
                item.Highlighted = true;
            }
            else
            {
                item.Highlighted = false;
            }
        }

        private void UI_ChangePage(string PageType)
        {
            GameState = PageType;

            if (UIPage_Current != null)
            {
                foreach (UIPage page in UIPages)
                {
                    if (page.Type == GameState)
                    {
                        UIPage_Current = page;
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
                for (int x = x1; x <= x2; x += CheckDistance)
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
                for (int y = y1; y <= y2; y += CheckDistance)
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

        private void Camera_SetRenderDistance()
        {
            ScreenWidth = _graphics.PreferredBackBufferWidth / TileWidth + 1;
            ScreenHeight = _graphics.PreferredBackBufferHeight / TileHeight + 1;
        }

        private void Window_ToggleFullScreen()
        {
            if (!_graphics.IsFullScreen)
            {
                _graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height;
                _graphics.ApplyChanges();
            }
            else
            {
                _graphics.PreferredBackBufferWidth = GraphicsDevice.Adapter.CurrentDisplayMode.Width / 3 * 2;
                _graphics.PreferredBackBufferHeight = GraphicsDevice.Adapter.CurrentDisplayMode.Height / 3 * 2;
                _graphics.ApplyChanges();
            }

            _graphics.ToggleFullScreen();

            Camera_SetOffset();
            Camera_SetRenderDistance();
        }

        #endregion

        #region Player Movement

        private void Execute_PlayerMomentum_Vertical()
        {
            //Collision type directly bellow player is checked. If there is nothing, the players vertical momentum increases (downward)
            int GravityEffectInterval = 2;

            if (gameTick % GravityEffectInterval == 0 && !Player.IsFlying)
            {
                if (GetPhysicsType(CheckLineCollision(Player.x, Player.y + Player.Height,
                                                    Player.x + (Player.Width - 1), Player.y + Player.Height,
                                                    TileWidth)) == 0)
                {
                    Player.Momentum_Vertical += 1;
                }
            }

            // Going Down
            if (Player.Momentum_Vertical > 0)
            {
                (int, int) CollisionDetails = CheckCubeCollision(Player.x, Player.y + Player.Height,
                                                                    Player.x + (Player.Width - 1), Player.y + Player.Height,
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
                                                                    Player.x + (Player.Width - 1), Player.y - 1,
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
                                                                    Player.x - 1, Player.y + (Player.Height - 1),
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
                (int, int) CollisionDetails = CheckCubeCollision(Player.x + Player.Width, Player.y,
                                                                    Player.x + Player.Width, Player.y + (Player.Height - 1),
                                                                    "Right", Player.Momentum_Horizontal);

                Player.x += CollisionDetails.Item1;
                CameraOffset_X += CollisionDetails.Item1;

                //Soid
                if (GetPhysicsType(CollisionDetails.Item2) == 2)
                {
                    Player.Momentum_Horizontal = 0;
                }
            }
        }

        private void PlayerMovement_Horizontal()
        {
            int CurrentSpeedMax = 0;// (int)Math.Round(Player.Speed_Base * TileWidth, 0);
            if (Player.IsCrouching && !Player.IsShifting)
            {
                CurrentSpeedMax = (int)(Player.Speed_Crouch * TileWidth);
            }
            else if (Player.IsShifting && !Player.IsCrouching)
            {
                CurrentSpeedMax = (int)(Player.Speed_Shift * TileWidth);
            }
            else
            {
                CurrentSpeedMax = (int)(Player.Speed_Base * TileWidth);
            }

            //Left
            if (Player.IsMovingLeft && !Player.IsMovingRight && gameTick % 3 == 0)
            {
                if (Player.Momentum_Horizontal > -CurrentSpeedMax)
                {
                    Player.Momentum_Horizontal--;
                }
            }
            //Right
            if (Player.IsMovingRight && !Player.IsMovingLeft && gameTick % 3 == 0)
            {
                if (Player.Momentum_Horizontal < CurrentSpeedMax)
                {
                    Player.Momentum_Horizontal++;
                }
            }
            //Slowdown
            if ((!Player.IsMovingLeft && !Player.IsMovingRight && gameTick % 3 == 0) || (Math.Abs(Player.Momentum_Horizontal) > CurrentSpeedMax && gameTick % 5 == 0))
            {
                if (Player.Momentum_Horizontal < 0)
                {
                    Player.Momentum_Horizontal++;
                }
                else if (Player.Momentum_Horizontal > 0)
                {
                    Player.Momentum_Horizontal--;
                }
            }
        }
        private void PlayerMovement_Vertical()
        {
            if (Player.IsJumping || (Player.IsFlying && Player.IsFlyingUp))
            {
                (int, int) CollisionDetails = CheckCubeCollision(Player.x, Player.y + Player.Height,
                                                                    Player.x + (Player.Width - 1), Player.y + Player.Height,
                                                                    "Down", 1);


                // Solid
                if (GetPhysicsType(CollisionDetails.Item2) == 2 || Player.IsFlying)
                {
                    if (Player.IsCrouching && !Player.IsShifting)
                    {
                        Player.Momentum_Vertical = -Player.JumpHeight_Crouch;
                    }
                    else if (Player.IsShifting && !Player.IsCrouching)
                    {
                        Player.Momentum_Vertical = -Player.JumpHeight_Shift;
                    }
                    else
                    {
                        Player.Momentum_Vertical = -Player.JumpHeight;
                    }
                }
            }
            if (Player.IsFlyingDown)
            {
                Player.Momentum_Vertical = Player.JumpHeight;
            }
        }

        private void PlayerMovement_ToggleFlight()
        {
            Player.IsFlying = !Player.IsFlying;
        }


        private void PlayerMovementHandler()
        {
            PlayerMovement_Horizontal();
            PlayerMovement_Vertical();

            Execute_PlayerMomentum_Vertical();
            Execute_PlayerMomentum_Horizontal();

            if (Player.IsFlying && Player.Momentum_Vertical != 0)
            {
                Player.Momentum_Vertical = 0;
            }
        }

        #endregion

        /////////////////////////////////////////

        #region Keybinds

        private void KeyBind_Handler()
        {
            Keys[] Keys_NewlyPressed = Keyboard.GetState().GetPressedKeys();


            UserControl_PlayerMovement(Keys_NewlyPressed);
            UserControl_PlayerMovement_Flight(Keys_NewlyPressed);
            UserControl_PlayerShiftCrouch(Keys_NewlyPressed);



            // Toggling FullScreen
            if (Keys_NewlyPressed.Contains(Keys.F) && !Keys_BeingPressed.Contains(Keys.F))
            {
                Window_ToggleFullScreen();
            }

            if (Keys_NewlyPressed.Contains(Keys.Escape) && !Keys_BeingPressed.Contains(Keys.Escape))
            {
                UserControl_TogglePause();
            }

            Keys_BeingPressed = new List<Keys>(Keys_NewlyPressed);
        }

        private void UserControl_PlayerMovement(Keys[] NewKeys)
        {
            // Assigning value to player movement tags
            if (NewKeys.Contains(Keys.A) && !NewKeys.Contains(Keys.D))
            {
                Player.IsMovingLeft = true;
            }
            else if (NewKeys.Contains(Keys.D) && !NewKeys.Contains(Keys.A))
            {
                Player.IsMovingRight = true;
            }
            if (NewKeys.Contains(Keys.A) && NewKeys.Contains(Keys.D))
            {
                if (Keys_BeingPressed.Contains(Keys.A) && !Keys_BeingPressed.Contains(Keys.D))
                {
                    Player.IsMovingLeft = false;
                    Player.IsMovingRight = true;
                }
                else if (Keys_BeingPressed.Contains(Keys.D) && !Keys_BeingPressed.Contains(Keys.A))
                {
                    Player.IsMovingLeft = true;
                    Player.IsMovingRight = false;
                }
            }

            // Reseting Left/Right movement tags
            if (!NewKeys.Contains(Keys.A))
            {
                Player.IsMovingLeft = false;
            }
            if (!NewKeys.Contains(Keys.D))
            {
                Player.IsMovingRight = false;
            }


            // Toggling Jump tag
            if (NewKeys.Contains(Keys.Space))
            {
                Player.IsJumping = true;
            }
            else
            {
                Player.IsJumping = false;
            }


        }
        private void UserControl_PlayerMovement_Flight(Keys[] NewKeys)
        {
            // Toggling Flight tag
            if (NewKeys.Contains(Keys.LeftAlt) && !Keys_BeingPressed.Contains(Keys.LeftAlt))
            {
                PlayerMovement_ToggleFlight();
            }

            // Assigning value to player Flight Movement tags
            if (NewKeys.Contains(Keys.W) && !NewKeys.Contains(Keys.S) && Player.IsFlying)
            {
                Player.IsFlyingUp = true;
            }
            else if (NewKeys.Contains(Keys.S) && !NewKeys.Contains(Keys.W) && Player.IsFlying)
            {
                Player.IsFlyingDown = true;
            }
            if (NewKeys.Contains(Keys.S) && NewKeys.Contains(Keys.W) && Player.IsFlying)
            {
                if (Keys_BeingPressed.Contains(Keys.S) && !Keys_BeingPressed.Contains(Keys.W))
                {
                    Player.IsFlyingDown = false;
                    Player.IsFlyingUp = true;
                }
                else if (Keys_BeingPressed.Contains(Keys.W) && !Keys_BeingPressed.Contains(Keys.S))
                {
                    Player.IsFlyingDown = true;
                    Player.IsFlyingUp = false;
                }
            }
            if (!NewKeys.Contains(Keys.S) || !Player.IsFlying)
            {
                Player.IsFlyingDown = false;
            }
            if (!NewKeys.Contains(Keys.W) || !Player.IsFlying)
            {
                Player.IsFlyingUp = false;
            }
        }
        private void UserControl_PlayerShiftCrouch(Keys[] NewKeys)
        {
            // Left Shift
            if (NewKeys.Contains(Keys.LeftShift) && !NewKeys.Contains(Keys.LeftControl))
            {
                Player.IsShifting = true;
                Player.IsCrouching = false;
            }
            // Left Control
            else if (NewKeys.Contains(Keys.LeftControl) && !NewKeys.Contains(Keys.LeftShift))
            {
                Player.IsCrouching = true;
                Player.IsShifting = false;
            }
            //Both Pressed
            else if (NewKeys.Contains(Keys.LeftShift) && NewKeys.Contains(Keys.LeftControl))
            {
                Player.IsShifting = true;
                Player.IsCrouching = false;
            }
            // Neither Pressed
            else if (!NewKeys.Contains(Keys.LeftShift) && !NewKeys.Contains(Keys.LeftControl))
            {
                Player.IsShifting = false;
                Player.IsCrouching = false;
            }
        }

        private void UserControl_TogglePause()
        {
            if (GameState == "Play")
            {
                UI_ChangePage("Pause");
            }
            else if (GameState == "Pause")
            {
                UI_ChangePage("Play");
            }
        }

        #endregion

        #region Mouse

        private void MouseClick_Handler()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                if (!MouseClicking_Left)
                {
                    if (UIPage_Current != null)
                    {
                        foreach (UIItem Item in UIPage_Current.UIItems)
                        {
                            int X = _graphics.PreferredBackBufferWidth / 2 + Item.X;
                            int Y = _graphics.PreferredBackBufferHeight / 2 + Item.Y;

                            if (Item.Type == "Button")
                            {
                                if (Mouse.GetState().X > X && Mouse.GetState().X < X + Item.Width &&
                                    Mouse.GetState().Y > Y && Mouse.GetState().Y < Y + Item.Height)
                                {
                                    UserControl_ButtonPress(Item.Data);
                                }
                            }
                        }
                    }
                }

                MouseClicking_Left = true;
            }
            else
            {
                MouseClicking_Left = false;
            }
        }
        private void MouseMove_Handler()
        {
            if (UIPage_Current != null)
            {
                foreach (UIItem Item in UIPage_Current.UIItems)
                {
                    int X = _graphics.PreferredBackBufferWidth / 2 + Item.X;
                    int Y = _graphics.PreferredBackBufferHeight / 2 + Item.Y;

                    if (Item.Type == "Button")
                    {
                        if (Mouse.GetState().X > X && Mouse.GetState().X < X + Item.Width &&
                                    Mouse.GetState().Y > Y && Mouse.GetState().Y < Y + Item.Height)
                        {
                            UI_ItemToggleHighlight(Item, true);
                        }
                        else
                        {
                            UI_ItemToggleHighlight(Item, false);
                        }
                    }
                }
            }
        }

        private void UserControl_ButtonPress(List<string> Data)
        {
            if (Data.Contains("Start New"))
            {
                UI_ChangePage("Play");
            }
            else if (Data.Contains("Resume"))
            {
                UI_ChangePage("Play");
            }
            else if (Data.Contains("Quit"))
            {
                System.Environment.Exit(0);
            }
        }

        #endregion

        /////////////////////////////////////////

        #region Fundamentals

        private Rectangle getRect(int x, int y)
        {
            //return new Rectangle((int)(x * TileWidth - Player.x), (int)(y * TileHeight - Player.y), TileWidth, TileHeight);
            return new Rectangle((x * TileWidth) - CameraOffset_X, (y * TileHeight) - CameraOffset_Y, TileWidth, TileHeight);
        }

        private Color Texture_GetTileColor(byte BlockId)
        {
            switch (BlockId)
            {
                case 0:
                    return Color_Barrier;
                case 1:
                    return Color_Default;
                case 2:
                    return Color_Dirt;
                case 3:
                    return Color_Grass;
                case 4:
                    return Color_Stone;
                case 5:
                    return Color_Sand;
                case 6:
                    return Color_Water;
                case 7:
                    return Color_Bedrock;
            }

            return Color.White;
        }



        protected override void Update(GameTime gameTime)
        {
            KeyBind_Handler();
            MouseClick_Handler();
            MouseMove_Handler();

            if (GameState == "Play")
            {
                Player.RegainHandler(gameTick);
                PlayerMovementHandler();
                Execute_BlockLoadBoundary();

                try
                {
                    if (Mouse.GetState().RightButton == ButtonState.Pressed)
                    {
                        World[(CameraOffset_Y / TileHeight) + (Mouse.GetState().Y / TileHeight)][(CameraOffset_X / TileWidth) + (Mouse.GetState().X / TileWidth)] = new Tile()
                        {
                            Type = 1
                        };
                    }
                    if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                    {
                        World[(CameraOffset_Y / TileHeight) + (Mouse.GetState().Y / TileHeight)][(CameraOffset_X / TileWidth) + (Mouse.GetState().X / TileWidth)] = null;
                    }
                }
                catch { }
            }


            gameTick++;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightSkyBlue);

            // BEGIN Draw ----
            _spriteBatch.Begin();


            //Ingame
            if (GameState == "Play")
            {
                //Tiles
                for (int y = CameraLoadBound_Y_Left; y < CameraLoadBound_Y_Right; y++)
                {
                    for (int x = CameraLoadBound_X_Left; x < CameraLoadBound_X_Right; x++)
                    {
                        if (World[y][x] != null)
                        {
                            _spriteBatch.Draw(Texture_White, getRect(x, y), Texture_GetTileColor(World[y][x].Type));
                        }
                    }
                }

                //Player
                _spriteBatch.Draw(Texture_White, new Rectangle(Player.x - CameraOffset_X, Player.y - CameraOffset_Y, Player.Width, Player.Height), Color.Red);
            }

            //UI
            foreach (UIPage page in UIPages)
            {
                if (page.Type == GameState)
                {
                    UI_RenderElements(page.UIItems);
                }
                
            }


            _spriteBatch.End();
            // END Draw ------

            base.Draw(gameTime);
        }

        #endregion
    }
}