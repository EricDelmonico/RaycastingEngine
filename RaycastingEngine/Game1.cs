using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaycastingEngine
{
    struct Enemy
    {
        public Vector2 spritePlane;
        public Vector2 position;
        public float hp;
        public float max_hp;
        public Texture2D sprite;
        public float distance;
        private List<Enemy> container;

        /// <summary>
        /// Creates a new enemy
        /// </summary>
        /// <param name="_position">where the enemy is on the map</param>
        /// <param name="_hp">the enemy's hp</param>
        /// <param name="_sprite">the enemy's appearance</param>
        /// <param name="_container">the list that contains this enemy</param>
        public Enemy(Vector2 _position, float _max_hp, Texture2D _sprite, List<Enemy> _container)
        {
            hp = _max_hp;
            max_hp = _max_hp;
            position = _position;
            spritePlane = Vector2.Zero;
            sprite = _sprite;
            distance = 0;
            container = _container;
        }

        /// <summary>
        /// Removes the enemy if its health is 0 or under
        /// </summary>
        public void CheckIfDead()
        {
            if (hp <= 0)
            {
                container.Remove(this);
            }
        }
    }

    struct WallBlock
    {
        // Corners of the wall (points)
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomRight;
        public Vector2 BottomLeft;
        
        // Sides of the wall (vectors)
        public Vector2 TopVector;
        public Vector2 RightVector;
        public Vector2 BottomVector;
        public Vector2 LeftVector;

        /// <summary>
        /// Creates a new wall block
        /// </summary>
        /// <param name="topL"></param>
        /// <param name="topR"></param>
        /// <param name="bottomR"></param>
        /// <param name="bottomL"></param>
        public WallBlock(Vector2 topL, Vector2 topR, Vector2 bottomR, Vector2 bottomL)
        {
            TopLeft = topL;
            TopRight = topR;
            BottomRight = bottomR;
            BottomLeft = bottomL;

            // points right
            TopVector = new Vector2(1, 0);
            // points down
            RightVector = new Vector2(0, 1);
            // points left
            BottomVector = new Vector2(-1, 0);
            // points up
            LeftVector   = new Vector2(0, -1);
        }
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private bool frame1 = true;
        //private const int screenWidth = 1024;
        //private const int screenHeight = 768;
        private const int screenWidth = 1280;
        private const int screenHeight = 720;

        private SpriteFont arial16;

        // Arrays that store data for every slice of screen
        private int[] wallHeights;
        private float[] relativeHitLocations;
        private Texture2D[] wallTextures;
        private Color[] wallColors;
        private int[] enemyHeights;
        private float[] enemyHitLocations;
        private Texture2D[] enemyTextures;
        private int[] enemyInThisSlice;
        private float[] rayDistances;

        // Textures a wall can have, decided by the
        // number in a given tile in the map below
        private Texture2D[] possibleTextures;

        private float fPlayerX;
        private float fPlayerY;
        private float fPlayerAngle;
        private float fFov;

        private bool fisheyeCorrection;

        private KeyboardState kbState;
        private KeyboardState prev_kbState;

        private Texture2D blankTexture;
        private Texture2D gunTexture;
        private Texture2D gunFiringTexture;
        private float timeSinceFiring;

        private const int gridSize = 1;
        private WallBlock[,] walls;

        // must store all enemies in scene for them to work correctly
        private List<Enemy> enemies;

        private int[,] map =
        {
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 0, 1 },
            {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 1 },
            {1, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        //private int[,] map =
        //{
        //    {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        //    {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        //    {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        //};

        // The array used to draw the map to the UI
        private char[,] mapHUD;

        // The direction the light is pointing in the scene
        private Vector2 lightDirection;

        // Whether the ui will 
        // draw to the screen
        private bool uiDraw;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic 

            fisheyeCorrection = false;

            // Store all necessary data for drawing every wall slice
            wallHeights = new int[screenWidth];
            relativeHitLocations = new float[screenWidth];
            wallTextures = new Texture2D[screenWidth];
            wallColors = new Color[screenWidth];
            enemyHeights = new int[screenWidth];
            enemyHitLocations = new float[screenWidth];
            enemyTextures = new Texture2D[screenWidth];
            enemyInThisSlice = new int[screenWidth];
            rayDistances = new float[screenWidth];

            // Possible wall textures based on number in map space
            possibleTextures = new Texture2D[2];

            // Populating the mapHUD with characters 
            // equivalent to the numbers in the map array
            int mapHeight = map.GetLength(0);
            int mapWidth = map.GetLength(1);
            mapHUD = new char[mapWidth, mapHeight];
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    mapHUD[i, j] = map[i, j].ToString()[0];
                }
            }

            // Creating the floor texture, which is basically just a white gradient
            blankTexture = new Texture2D(GraphicsDevice, screenWidth, screenHeight / 2);
            Color[] floorData = new Color[screenWidth * screenHeight / 2];
            Color rowColor = Color.Black;
            for (int i = 0; i < screenHeight / 2; i++)
            {
                if (rowColor.B < 255 && i > screenHeight / 5)
                {
                    rowColor.R += 1;
                    rowColor.G += 1;
                    rowColor.B += 1;
                }

                for (int j = 0; j < screenWidth; j++)
                {
                    //floorData[i * screenWidth + j] = rowColor;
                    floorData[i * screenWidth + j] = Color.White;
                }
            }
            blankTexture.SetData<Color>(floorData);

            // initializing the player data at position 
            // 8, 8 and an angle of 0 radians with 45 fov
            fPlayerX = 8;
            fPlayerY = 8;
            fPlayerAngle = 0.0f;
            fFov = (float)(Math.PI / 4);

            // Populating the list of wallBlocks
            walls = new WallBlock[mapWidth, mapHeight];
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (map[j, i] > 0)
                    {
                        walls[j, i] = (new WallBlock(new Vector2(gridSize * j, gridSize * i),                        // top left
                                                     new Vector2(gridSize * j + gridSize, gridSize * i),             // top right
                                                     new Vector2(gridSize * j + gridSize, gridSize * i + gridSize),  // bottom right
                                                     new Vector2(gridSize * j, gridSize * i + gridSize)));           // bottom left
                    }
                }
            }

            lightDirection = new Vector2(1, 2);
            lightDirection.Normalize();

            timeSinceFiring = 100;

            enemies = new List<Enemy>();

            // ui is on by default since it 
            // has all controls and the map
            uiDraw = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            arial16 = Content.Load<SpriteFont>("arial16");

            gunTexture = Content.Load<Texture2D>("gun");
            gunFiringTexture = Content.Load<Texture2D>("gunFiring");

            Texture2D enemyTestTexture = Content.Load<Texture2D>("enemyTest");
            enemies.Add(new Enemy(new Vector2(7, 7), 1000, enemyTestTexture, enemies));

            possibleTextures[0] = enemyTestTexture;
            possibleTextures[1] = enemyTestTexture;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override async void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            enemyHeights = new int[screenWidth];
            enemyHitLocations = new float[screenWidth];
            enemyTextures = new Texture2D[screenWidth];
            enemyInThisSlice = new int[screenWidth];

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy temp = enemies[i];
                temp.spritePlane = new Vector2(fPlayerY - temp.position.Y, -(fPlayerX - temp.position.X));
                temp.spritePlane.Normalize();
                enemies[i] = temp;
            }

            timeSinceFiring += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // USER INPUT
            prev_kbState = kbState;
            kbState = Keyboard.GetState();
            float fForwardX = (float)Math.Cos(fPlayerAngle);
            float fForwardY = (float)Math.Sin(fPlayerAngle);
            // Walk forward
            if (kbState.IsKeyDown(Keys.W))
            {
                fPlayerX += (float)(fForwardX * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                fPlayerY += (float)(fForwardY * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);

                if (map[(int)fPlayerX, (int)fPlayerY] > 0)
                {
                    fPlayerX -= (float)(fForwardX * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                    fPlayerY -= (float)(fForwardY * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                }
            }
            // Walk backward
            if (kbState.IsKeyDown(Keys.S))
            {
                fPlayerX -= (float)(fForwardX * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                fPlayerY -= (float)(fForwardY * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);

                if (map[(int)fPlayerX, (int)fPlayerY] > 0)
                {
                    fPlayerX += (float)(fForwardX * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                    fPlayerY += (float)(fForwardY * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                }
            }
            // Strafe right
            if (kbState.IsKeyDown(Keys.D))
            {
                fPlayerX -= (float)(fForwardY * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                fPlayerY -= (float)(-fForwardX * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);

                if (map[(int)fPlayerX, (int)fPlayerY] > 0)
                {
                    fPlayerX += (float)(fForwardY * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                    fPlayerY += (float)(-fForwardX * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                }
            }
            // Strafe left
            if (kbState.IsKeyDown(Keys.A))
            {
                fPlayerX += (float)(fForwardY * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                fPlayerY += (float)(-fForwardX * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);

                if (map[(int)fPlayerX, (int)fPlayerY] > 0)
                {
                    fPlayerX -= (float)(fForwardY * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                    fPlayerY -= (float)(-fForwardX * gameTime.ElapsedGameTime.TotalSeconds * 4.0f);
                }
            }
            // Turn left
            if (kbState.IsKeyDown(Keys.J))
            {
                fPlayerAngle -= (float)(1.5f * gameTime.ElapsedGameTime.TotalSeconds);
            }
            // Turn right
            if (kbState.IsKeyDown(Keys.L))
            {
                fPlayerAngle += (float)(1.5f * gameTime.ElapsedGameTime.TotalSeconds);
            }

            // Toggle whether to correct the fisheye effect caused by raycasting method
            if (kbState.IsKeyDown(Keys.F) && !prev_kbState.IsKeyDown(Keys.F))
            {
                if (fisheyeCorrection)
                {
                    fisheyeCorrection = false;
                }
                else
                {
                    fisheyeCorrection = true;
                }
            }

            // Toggle whether to draw the ui to the screen
            if (kbState.IsKeyDown(Keys.U) && !prev_kbState.IsKeyDown(Keys.U))
            {
                if (uiDraw)
                {
                    uiDraw = false;
                }
                else
                {
                    uiDraw = true;
                }
            }

            // Gives the player the ability to rotate 
            // the light source for testing purposes 
            // (or just for fun)
            if (kbState.IsKeyDown(Keys.Left))
            {
                float x = lightDirection.X, y = lightDirection.Y;
                lightDirection.X = (float)(Math.Cos(Math.PI / 4 * gameTime.ElapsedGameTime.TotalSeconds) * x - Math.Sin(Math.PI / 4 * gameTime.ElapsedGameTime.TotalSeconds) * y);
                lightDirection.Y = (float)(Math.Sin(Math.PI / 4 * gameTime.ElapsedGameTime.TotalSeconds) * x + Math.Cos(Math.PI / 4 * gameTime.ElapsedGameTime.TotalSeconds) * y);
                lightDirection.Normalize();
            }
            if (kbState.IsKeyDown(Keys.Right))
            {
                float x = lightDirection.X, y = lightDirection.Y;
                lightDirection.X = (float)(Math.Cos(-Math.PI / 4 * gameTime.ElapsedGameTime.TotalSeconds) * x - Math.Sin(-Math.PI / 4 * gameTime.ElapsedGameTime.TotalSeconds) * y);
                lightDirection.Y = (float)(Math.Sin(-Math.PI / 4 * gameTime.ElapsedGameTime.TotalSeconds) * x + Math.Cos(-Math.PI / 4 * gameTime.ElapsedGameTime.TotalSeconds) * y);
                lightDirection.Normalize();
            }

            // RAYCASTING
            int indexOfEnemyInScreenCenter = -1;
            #region Threaded
            Task[] tasks = new Task[screenWidth - 1];
            for (int column = 0; column < screenWidth - 1; column++)
            {
                int columnCopy = column;
                tasks[column] = Task.Run(() => Cast(columnCopy));
            }
            await Task.WhenAll(tasks);
            #endregion
            #region Not threaded
            // for (int column = 0; column < screenWidth - 1; column++)
            // {
            //     Cast(column);
            // }
            #endregion

            void Cast(int column)
            {
                float fRayAngle = (fPlayerAngle - fFov / 2.0f) +        // rayangle starts at the far left of the player's fov
                                  ((float)column / screenWidth) * fFov; // rayangle gets offset by column/screenangle percent of the FOV each iteration

                // Unit vector components of the ray angle
                float fRayX = (float)Math.Cos(fRayAngle);
                float fRayY = (float)Math.Sin(fRayAngle);

                // How long the ray will travel
                float rayDist = 40;

                int hit = 0;
                int hitX = 0, hitY = 0;
                float fRoughDistanceToHit = 0;
                float fExactDistanceToHit = 0;

                List<Tuple<int, int>> wallCoords = new List<Tuple<int, int>>();
                while (hit == 0 && fRoughDistanceToHit < rayDist)
                {
                    hit = map[hitX = (int)(fPlayerX + fRoughDistanceToHit * fRayX), hitY = (int)(fPlayerY + fRoughDistanceToHit * fRayY)];
                    fRoughDistanceToHit += 0.005f;
                }

                // If there was not hit, 
                // leave this column
                if (hit == 0)
                    return;
                else
                {
                    // Have a buffer of the walls around this one
                    // this one
                    wallCoords.Add(new Tuple<int, int>(hitX, hitY));

                    int mapszX = map.GetLength(0);
                    int mapszY = map.GetLength(1);
                    // up
                    if (hitY + 1 < mapszY && map[hitX, hitY + 1] > 0)
                        wallCoords.Add(new Tuple<int, int>(hitX, hitY + 1));
                    // down
                    if (hitY - 1 > 0 && map[hitX, hitY - 1] > 0)
                        wallCoords.Add(new Tuple<int, int>(hitX, hitY - 1));
                    // left
                    if (hitX - 1 > 0 && map[hitX - 1, hitY] > 0)
                        wallCoords.Add(new Tuple<int, int>(hitX - 1, hitY));
                    // right
                    if (hitX + 1 < mapszX && map[hitX + 1, hitY] > 0)
                        wallCoords.Add(new Tuple<int, int>(hitX + 1, hitY));
                    // top right corner
                    if (hitX + 1 < mapszX && hitY + 1 < mapszY && map[hitX + 1, hitY + 1] > 0)
                        wallCoords.Add(new Tuple<int, int>(hitX + 1, hitY + 1));
                    // top left corner
                    if (hitX - 1 > 0 && hitY + 1 < mapszY && map[hitX - 1, hitY + 1] > 0)
                        wallCoords.Add(new Tuple<int, int>(hitX - 1, hitY + 1));
                    // bottom right corner
                    if (hitX + 1 < mapszX && hitY - 1 > 0 && map[hitX + 1, hitY - 1] > 0)
                        wallCoords.Add(new Tuple<int, int>(hitX + 1, hitY - 1));
                    // bottom left corner
                    if (hitX - 1 > 0 && hitY - 1 > 0 && map[hitX - 1, hitY - 1] > 0)
                        wallCoords.Add(new Tuple<int, int>(hitX - 1, hitY - 1));
                }

                // Find the exact distance to the hit
                Vector2 normal = Vector2.Zero;
                fExactDistanceToHit = float.MaxValue;
                float tempHitDist = 0;
                for (int i = 0; i < wallCoords.Count; i++)
                {
                    float tempRHL;
                    Vector2 tempNormal;
                    tempHitDist = FindDistance(walls[wallCoords[i].Item1, wallCoords[i].Item2],
                                               new Vector2(fRayX, fRayY) * rayDist,
                                               new Vector2(fPlayerX, fPlayerY),
                                               out tempNormal,
                                               out tempRHL);

                    if (tempHitDist != -1 && tempHitDist < fExactDistanceToHit)
                    {
                        fExactDistanceToHit = tempHitDist;
                        normal = tempNormal;
                        relativeHitLocations[column] = tempRHL;
                    }
                }


                float fExactDistanceToEnemy = float.MaxValue;
                int indexOfEnemyHit = -1;
                float tempDistance = -1;
                Vector2 enemyNormal = new Vector2();
                for (int i = 0; i < enemies.Count; i++)
                {
                    if ((tempDistance = FindDistance(enemies[i],
                        new Vector2(fRayX, fRayY) * rayDist,
                        new Vector2(fPlayerX, fPlayerY),
                        out enemyNormal,
                        out enemyHitLocations[column])) != -1 && tempDistance < fExactDistanceToEnemy)
                    {
                        fExactDistanceToEnemy = tempDistance;
                        indexOfEnemyHit = i;
                        if (column == screenWidth / 2)
                        {
                            indexOfEnemyInScreenCenter = indexOfEnemyHit;
                        }
                    }
                }

                // FindDistanceToWall only returns 
                // -1 when something goes wrong
                if (fExactDistanceToHit == -1)
                    return;

                // Set the wall height
                wallHeights[column] = (int)(screenHeight / (fisheyeCorrection ? (double)fExactDistanceToHit * Math.Cos(fRayAngle - fPlayerAngle) : fExactDistanceToHit));
                rayDistances[column] = fExactDistanceToHit;

                // Set enemy height
                if (indexOfEnemyHit != -1)
                {
                    enemyHeights[column] = (int)(screenHeight / (fisheyeCorrection ? (double)fExactDistanceToEnemy * Math.Cos(fRayAngle - fPlayerAngle) : fExactDistanceToEnemy));
                    enemyInThisSlice[column] = indexOfEnemyHit;

                    // Change the ray distance to the 
                    // enemy's distance if it is closer
                    if (fExactDistanceToEnemy < fExactDistanceToHit)
                    {
                        rayDistances[column] = fExactDistanceToEnemy;
                    }
                }

                if (hit > 0)
                {
                    // 38-217
                    Color shade = Color.Gray;
                    if (shade.R > 0)
                        shade.R = (byte)(Vector2.Dot(normal, lightDirection) * 88 + 160);
                    if (shade.G > 0)
                        shade.G = (byte)(Vector2.Dot(normal, lightDirection) * 88 + 160);
                    if (shade.B > 0)
                        shade.B = (byte)(Vector2.Dot(normal, lightDirection) * 88 + 160);
                    wallColors[column] = shade;


                    wallTextures[column] = possibleTextures[Math.Min(hit - 1, 1)];
                }
                if (indexOfEnemyHit != -1)
                {
                    enemyTextures[column] = enemies[indexOfEnemyHit].sprite;
                }
            }

            if (kbState.IsKeyDown(Keys.Space))
            {
                if (timeSinceFiring > 0.1f)
                {
                    timeSinceFiring = 0;
                    if (indexOfEnemyInScreenCenter != -1 && enemyHeights[screenWidth / 2] > wallHeights[screenWidth / 2])
                    {
                        Enemy temp = enemies[indexOfEnemyInScreenCenter];
                        temp.hp -= 10;
                        enemies[indexOfEnemyInScreenCenter] = temp;
                    }
                }
            }

            // Update enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].CheckIfDead();
            }

            base.Update(gameTime);
        }

        public float FindDistance(Enemy e, Vector2 rayCast, Vector2 playerPosition, out Vector2 normal, out float relativeHitLocation)
        {
            // Derived from finding intersection 
            // of two lines in parametric form 
            // using a system of equations

            float distance = float.MaxValue;
            List<Vector2> solvedCoefficientSets = new List<Vector2>();
            List<Vector2> vectorsHit = new List<Vector2>();
            int indexOfClosestVectorHit = -1;

            // Player To Corner
            float pToC_X, pToC_Y;
            Vector2 solvedCoefficients;

            // Top
            pToC_X = e.position.X - playerPosition.X;
            pToC_Y = e.position.Y - playerPosition.Y;
            solvedCoefficients.X = -e.spritePlane.Y * pToC_X + e.spritePlane.X * pToC_Y;
            solvedCoefficients.Y = -rayCast.Y * pToC_X + rayCast.X * pToC_Y;
            solvedCoefficients /= (-rayCast.X * e.spritePlane.Y) + (rayCast.Y * e.spritePlane.X);
            solvedCoefficientSets.Add(new Vector2(solvedCoefficients.X, solvedCoefficients.Y));
            vectorsHit.Add(e.spritePlane);

            for (int i = 0; i < solvedCoefficientSets.Count; i++)
            {
                if (solvedCoefficientSets[i].X >= 0 && solvedCoefficientSets[i].X <= 1 &&
                    solvedCoefficientSets[i].Y >= 0 && solvedCoefficientSets[i].Y <= 1)
                {
                    Vector2 rayModified = rayCast * solvedCoefficientSets[i].X;
                    if (rayModified.Length() < distance)
                    {
                        distance = rayModified.Length();
                        indexOfClosestVectorHit = i;
                    }
                }
            }

            relativeHitLocation = 0;
            if (indexOfClosestVectorHit != -1)
                relativeHitLocation = solvedCoefficientSets[indexOfClosestVectorHit].Y;

            if (indexOfClosestVectorHit != -1)
            {
                normal = new Vector2(vectorsHit[indexOfClosestVectorHit].Y, -vectorsHit[indexOfClosestVectorHit].X);
                return distance;
            }
            normal = new Vector2();
            return -1;
        }

        public float FindDistance(WallBlock w, Vector2 rayCast, Vector2 playerPosition, out Vector2 normal, out float relativeHitLocation)
        {
            // Derived from finding intersection 
            // of two lines in parametric form 
            // using a system of equations

            float distance = float.MaxValue;
            List<Vector2> solvedCoefficientSets = new List<Vector2>();
            List<Vector2> vectorsHit = new List<Vector2>();
            int indexOfClosestVectorHit = -1;

            // Player To Corner
            float pToC_X, pToC_Y;
            Vector2 solvedCoefficients;

            // Top
            pToC_X = w.TopLeft.X - playerPosition.X;
            pToC_Y = w.TopLeft.Y - playerPosition.Y;
            solvedCoefficients.X = -w.TopVector.Y * pToC_X + w.TopVector.X * pToC_Y;
            solvedCoefficients.Y = -rayCast.Y * pToC_X + rayCast.X * pToC_Y;
            solvedCoefficients /= (-rayCast.X * w.TopVector.Y) + (rayCast.Y * w.TopVector.X);
            solvedCoefficientSets.Add(new Vector2(solvedCoefficients.X, solvedCoefficients.Y));
            vectorsHit.Add(w.TopVector);

            // Right
            pToC_X = w.TopRight.X - playerPosition.X;
            pToC_Y = w.TopRight.Y - playerPosition.Y;
            solvedCoefficients.X = -w.RightVector.Y * pToC_X + w.RightVector.X * pToC_Y;
            solvedCoefficients.Y = -rayCast.Y * pToC_X + rayCast.X * pToC_Y;
            solvedCoefficients /= (-rayCast.X * w.RightVector.Y) + (rayCast.Y * w.RightVector.X);
            solvedCoefficientSets.Add(new Vector2(solvedCoefficients.X, solvedCoefficients.Y));
            vectorsHit.Add(w.RightVector);

            // Bottom
            pToC_X = w.BottomRight.X - playerPosition.X;
            pToC_Y = w.BottomRight.Y - playerPosition.Y;
            solvedCoefficients.X = -w.BottomVector.Y * pToC_X + w.BottomVector.X * pToC_Y;
            solvedCoefficients.Y = -rayCast.Y * pToC_X + rayCast.X * pToC_Y;
            solvedCoefficients /= (-rayCast.X * w.BottomVector.Y) + (rayCast.Y * w.BottomVector.X);
            solvedCoefficientSets.Add(new Vector2(solvedCoefficients.X, solvedCoefficients.Y));
            vectorsHit.Add(w.BottomVector);

            // Left
            pToC_X = w.BottomLeft.X - playerPosition.X;
            pToC_Y = w.BottomLeft.Y - playerPosition.Y;
            solvedCoefficients.X = -w.LeftVector.Y * pToC_X + w.LeftVector.X * pToC_Y;
            solvedCoefficients.Y = -rayCast.Y * pToC_X + rayCast.X * pToC_Y;
            solvedCoefficients /= (-rayCast.X * w.LeftVector.Y) + (rayCast.Y * w.LeftVector.X);
            solvedCoefficientSets.Add(new Vector2(solvedCoefficients.X, solvedCoefficients.Y));
            vectorsHit.Add(w.LeftVector);

            for (int i = 0; i < solvedCoefficientSets.Count; i++)
            {
                if (solvedCoefficientSets[i].X >= 0 && solvedCoefficientSets[i].X <= 1 &&
                    solvedCoefficientSets[i].Y >= 0 && solvedCoefficientSets[i].Y <= 1)
                {
                    Vector2 rayModified = rayCast * solvedCoefficientSets[i].X;
                    if (rayModified.Length() < distance)
                    {
                        distance = rayModified.Length();
                        indexOfClosestVectorHit = i;
                    }
                }
            }

            if (indexOfClosestVectorHit != -1)
            {
                relativeHitLocation = solvedCoefficientSets[indexOfClosestVectorHit].Y;
                normal = new Vector2(vectorsHit[indexOfClosestVectorHit].Y, -vectorsHit[indexOfClosestVectorHit].X);
                return distance;
            }
            relativeHitLocation = -1;
            normal = new Vector2();
            return -1;
        }

        /// <summary>
        /// Draws a wall height pixels high onto pixel row row using spriteBatch sb
        /// </summary>
        /// <param name="height"></param>
        /// <param name="row"></param>
        /// <param name="sb"></param>
        private void DrawWall(int height, int row, SpriteBatch sb, Texture2D texture, float relativeHitLocation, Color color)
        {
            if (texture == null) return;
            int textureSlice = (int)(texture.Width - texture.Width * relativeHitLocation);
            sb.Draw(texture, new Rectangle(row, screenHeight / 2 - height / 2, 1, height), new Rectangle(textureSlice, 0, 1, texture.Height), color);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            // Draw the floor and ceiling
            spriteBatch.Draw(blankTexture, new Vector2(0, screenHeight / 2), Color.White);
            spriteBatch.Draw(blankTexture, Vector2.Zero, Color.Black);

            // Draw the walls
            for (int i = 0; i < wallHeights.Length; i++)
            {
                DrawWall(wallHeights[i], i, spriteBatch, wallTextures[i], relativeHitLocations[i], wallColors[i]);
            }

            // Draw Enemies and store their on screen locations
            List<List<int>> enemySlices = new List<List<int>>();
            int currentList = -1;
            for (int i = 0; i < enemyHeights.Length; i++)
            {
                if (enemyTextures[i] != null && enemyHeights[i] > wallHeights[i])
                {
                    //Means we're on a new enemy
                    if (i - 1 < 0 || enemyTextures[i-1] == null)
                    {
                        enemySlices.Add(new List<int>());
                        currentList++;
                    }
                    if (currentList > -1)
                        enemySlices[currentList].Add(i);
                    DrawWall(enemyHeights[i], i, spriteBatch, enemyTextures[i], enemyHitLocations[i], Color.Red);
                }
            }

            // Draw enemy health bars
            if (enemies.Count > 0)
            {
                for (int i = 0; i < enemySlices.Count; i++)
                {
                    // Traversal across the texture each pixel
                    float deltaSlice = 1;
                    if (enemySlices[i].Count > 2)
                    {
                         deltaSlice= enemyHitLocations[enemySlices[i][enemySlices[i].Count / 2 + 1]] -
                                     enemyHitLocations[enemySlices[i][enemySlices[i].Count / 2]];
                    }
                    
                    float healthBarWidth = 1 / deltaSlice;

                    int drawStartMod = -(int)(enemyHitLocations[enemySlices[i][0]] * healthBarWidth);

                    spriteBatch.Draw(blankTexture,
                                     new Rectangle(drawStartMod + enemySlices[i][enemySlices[i].Count / 8],                                                                    // The start of the enemy being drawn
                                                   screenHeight / 2 - (enemyHeights[enemySlices[i][enemySlices[i].Count / 2]] / 2) - 30, // Put the health bar above the enemy
                                                   (int)(healthBarWidth * 0.75f),                                                                  // Make the health bar as wide as the enemy
                                                   enemyHeights[enemySlices[i][enemySlices[i].Count / 2]] / 20),                         // hp bar height corresponds to enemy height  
                                     Color.DarkGreen);

                    spriteBatch.Draw(blankTexture,
                                     new Rectangle(drawStartMod + enemySlices[i][enemySlices[i].Count / 8],
                                                   screenHeight / 2 - (enemyHeights[enemySlices[i][enemySlices[i].Count / 2]] / 2) - 30,
                                                   (int)(healthBarWidth * .75f * enemies[enemyInThisSlice[enemySlices[i][0]]].hp / enemies[enemyInThisSlice[enemySlices[i][0]]].max_hp), // corresponds to actual hp
                                                   enemyHeights[enemySlices[i][enemySlices[i].Count / 2]] / 20),
                                     Color.Lime);
                }
            }

            // Draw the gun on top of everything
            spriteBatch.Draw(timeSinceFiring == 0 ? gunFiringTexture : gunTexture, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);

            // ------------------UI--------------------
            if (uiDraw)
            {
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        if (mapHUD[i, j] != '0')
                        {
                            spriteBatch.Draw(blankTexture, new Rectangle(i * 16, j * 16 + 16, 16, 16), null, Color.Gray);
                        }
                        else if (mapHUD[i, j] == '0')
                        {
                            spriteBatch.Draw(blankTexture, new Rectangle(i * 16, j * 16 + 16, 16, 16), null, Color.White);
                        }
                    }
                }
                // Draw the player and enemies
                spriteBatch.Draw(blankTexture, new Rectangle((int)(fPlayerX * 16), (int)(fPlayerY * 16 + 16), 4, 4), null, Color.Green);
                foreach (Enemy enemy in enemies)
                {
                    spriteBatch.Draw(blankTexture, new Rectangle((int)(enemy.position.X * 16), (int)(enemy.position.Y * 16), 16, 16), null, Color.Red);
                }
                // Drawing player's rays
                for (float i = -fFov / 2; i < fFov / 2; i += 0.001f)
                {
                    float currentAngle = fPlayerAngle - i;
                    Vector2 fovVec = new Vector2((float)Math.Cos(currentAngle), (float)Math.Sin(currentAngle));

                    // Finding current "pixel", as if we were doing more raycasting. This is the solved
                    // version of the equation to get fRayAngle. The purpose of this is to access the
                    // rayDistances array in order to more accurately draw rays on minimap
                    int currentPixel = (int)((screenWidth * (currentAngle - (fPlayerAngle - fFov / 2))) / fFov);
                    float currentDist = 0;
                    if (currentPixel > 0 && currentPixel < screenWidth)
                        currentDist = rayDistances[currentPixel];

                    // How many little boxes will be drawn 
                    // per 1 unit of distance along the ray
                    float drawsPerSquare = 4;
                    for (float draws = 0; draws < currentDist; draws += 1 / drawsPerSquare)
                    {
                        spriteBatch.Draw(blankTexture, new Rectangle((int)(fPlayerX * 16 + fovVec.X * 16f * draws), (int)(fPlayerY * 16 + fovVec.Y * 16f * draws + 16), 4, 4), null, Color.Red);
                    }
                }

                debugUIElements = 0;
                DrawDebugUI($"Fisheye Correction: {fisheyeCorrection}");
                DrawDebugUI("Press F to toggle fisheye correction");
                DrawDebugUI("Press space to shoot");
                DrawDebugUI("Use WASD to move around the world");
                DrawDebugUI("Use J and K to turn");
                DrawDebugUI("Press U to toggle this UI on or off");
                DrawDebugUI("Use the arrow keys to rotate the light in the scene");
                DrawDebugUI($"PlayerAngle: {fPlayerAngle}");
                DrawDebugUI($"FPS: {1 / gameTime.ElapsedGameTime.TotalSeconds}");
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private int debugUIElements = 1;
        private void DrawDebugUI(string message)
        {
            spriteBatch.DrawString(arial16, message, new Vector2(0, (16 * debugUIElements) + 16 * map.GetLength(1)), Color.HotPink);
            debugUIElements++;
        }
    }
}
