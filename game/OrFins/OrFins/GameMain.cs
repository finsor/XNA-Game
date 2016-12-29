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
using System.Net;
using SW = System.Diagnostics.Stopwatch;

namespace OrFins
{
    enum GameState
    {
        Menu,
        SinglePlayer,
        MultiPlayer
    }

    public class GameMain : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GameManager gameManager;
        MenuManager menuManager;
        GameState gameState;
        Vector2 windowScale;

        public GameMain()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // Set basic window width
            graphics.PreferredBackBufferWidth = 832;

            Window.AllowUserResizing = true;

            this.IsMouseVisible = true;

            // For one update followed by one draw
            base.IsFixedTimeStep = false;

            SoundDictionary.SoundAvailable = true;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            #region Load dictionaries
            SpritesDictionary.LoadSprites(Content, GraphicsDevice);
            ClothingDictionary.Initialize(spriteBatch,
                @"ClothingData\weaponENC.txt",
                @"ClothingData\headENC.txt",
                @"ClothingData\topENC.txt",
                @"ClothingData\bottomENC.txt",
                @"ClothingData\shoeENC.txt");
            Button soundButton = new Button(spriteBatch, Folders.SoundOn, new Vector2(823, 21), "", null, Vector2.One, Color.White, SoundDictionary.ToggleSound);
            SoundDictionary.Initialize(Content, "BGM", "SoundEffects", soundButton);
            #endregion

            #region Create service class
            Texture2D pixel;
            pixel = new Texture2D(graphics.GraphicsDevice, 1, 1);
            pixel.SetData<Color>(new Color[] { Color.White });

            Texture2D rectangleTexture = Content.Load<Texture2D>("rectangleTexture");

            int screenHeight = graphics.PreferredBackBufferHeight;
            int screenWidth = graphics.PreferredBackBufferWidth = 832;

            Service.Initialize(rectangleTexture, pixel, screenHeight, screenWidth);
            #endregion

            #region Create MenuManager
            SpriteFont font = Content.Load<SpriteFont>("Font");
            Texture2D menuTexture = Content.Load<Texture2D>("menu/main");
            menuManager = new MenuManager(menuTexture, spriteBatch, font, NewSingleGame, LoadSingleGame, MultiPlayerGame, this.Exit);

            this.gameState = GameState.Menu;
            #endregion
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            this.windowScale = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height) / new Vector2(Service.screenWidth, Service.screenHeight);

            if (gameState == GameState.Menu)
                menuManager.Update(windowScale);

            if (gameState != GameState.Menu)
                gameManager.Update(gameTime, windowScale);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (gameState)
            {
                case GameState.Menu:
                    menuManager.Draw(windowScale);

                    break;

                default:
                    gameManager.Draw(windowScale, GraphicsDevice);

                    break;
            }

            base.Draw(gameTime);
        }

        #region private functions
        private void NewSingleGame()
        {
            Player player = Player.NewPlayer(spriteBatch);

            CreateGame(GameState.SinglePlayer, player);
        }
        private void LoadSingleGame()
        {
            Player player = Player.NewPlayer(spriteBatch);
            player.LoadFromFile();

            CreateGame(GameState.SinglePlayer, player);
        }
        private void MultiPlayerGame()
        {
            Player player = Player.NewPlayer(spriteBatch);
            player.LoadFromFile();

            CreateGame(GameState.MultiPlayer, player);
        }
        private void CreateGame(GameState gameState, Player player)
        {
            SpriteFont font = Content.Load<SpriteFont>("Font");

            #region Create hud

            Equipment equipment = new Equipment(spriteBatch, Content.Load<Texture2D>("characterGUI/equipment"), new Vector2(350, 100), Vector2.One, font, player.Unwear);
            Bag bag = new Bag(spriteBatch, Content.Load<Texture2D>("characterGUI/bag"), new Vector2(350, 100), Vector2.One, font, player.Wear);
            DialogBox deathBox = new DialogBox(spriteBatch, Content.Load<Texture2D>("characterGUI/deathBox"), new Vector2(350, 100), Vector2.One, font);
            Shop shop = new Shop(spriteBatch, Content.Load<Texture2D>("characterGUI/shop"), new Vector2(175, 100), Vector2.One, font, player);
            Texture2D keyboardSettings = Content.Load<Texture2D>("keyboard");

            HUD hud = new HUD(
                font,
                spriteBatch,
                player,         // Details to display by hud and player to control with buttons
                equipment,
                bag,
                shop,
                deathBox,
                keyboardSettings);


            hud.AddButton(new Button(spriteBatch, Folders.regularButton, new Vector2(480, 473), "Equipment", font, Vector2.One, Color.CornflowerBlue, hud.OpenEquipment));
            hud.AddButton(new Button(spriteBatch, Folders.regularButton, new Vector2(580, 473), "Bag", font, Vector2.One, Color.CornflowerBlue, hud.OpenBag));
            hud.AddButton(new Button(spriteBatch, Folders.regularButton, new Vector2(680, 473), "Save", font, Vector2.One, Color.CornflowerBlue, player.SaveToFile));
            hud.AddButton(SoundDictionary.soundButton);
            #endregion

            #region Create world
            // Maps:
            Map city = Map.CreateCity(spriteBatch, graphics, Content, hud);
            Map trainingGrounds = Map.CreateTrainingGrounds(spriteBatch, graphics, Content);
            Map forest = Map.CreateForest(spriteBatch, graphics, Content);
            Map lava = Map.CreateLava(spriteBatch, graphics, Content);

            // Portals:
            city.portals.Add(new Portal(Folders.portal, spriteBatch, city.platforms[0], PortalPositions.RIGHT, Color.White, Vector2.One, 0, 10, trainingGrounds));
            trainingGrounds.portals.Add(new Portal(Folders.portal, spriteBatch, trainingGrounds.platforms[0], PortalPositions.LEFT, Color.White, Vector2.One, 0, 10, city));
            trainingGrounds.portals.Add(new Portal(Folders.portal, spriteBatch, trainingGrounds.platforms[4], PortalPositions.MIDDLE, Color.White, Vector2.One, 0, 10, forest));
            forest.portals.Add(new Portal(Folders.portal, spriteBatch, forest.platforms[0], PortalPositions.LEFT, Color.White, Vector2.One, 0, 10, trainingGrounds));
            forest.portals.Add(new Portal(Folders.portal, spriteBatch, forest.platforms[0], PortalPositions.RIGHT, Color.White, Vector2.One, 0, 10, lava));
            lava.portals.Add(new Portal(Folders.portal, spriteBatch, lava.platforms[0], PortalPositions.LEFT, Color.White, Vector2.One, 0, 10, forest));
            #endregion

            #region Create camera
            Camera camera = new Camera(player, Vector2.One, new Viewport(0, 0, Service.screenWidth, Service.screenHeight));
            #endregion

            switch (gameState)
            {
                case GameState.SinglePlayer:

                    hud.AddButton(new Button(spriteBatch, Folders.regularButton, new Vector2(780, 473), "Menu", font, Vector2.One, Color.CornflowerBlue, this.BackToMenu));
                    gameManager = new SingleplayerManager(spriteBatch, camera, player, city, hud);

                    break;

                case GameState.MultiPlayer:

                    OnlineClient onlineClient = new OnlineClient("127.7.7.7", 9999);
                    hud.AddButton(new Button(spriteBatch, Folders.regularButton, new Vector2(780, 473), "Menu", font, Vector2.One, Color.CornflowerBlue, onlineClient.CloseConnection, this.BackToMenu));
                    gameManager = new MultiplayerManager(onlineClient, spriteBatch, camera, player, city, hud, font);

                    break;
            }

            deathBox.AddButton(new Button(spriteBatch, Folders.regularButton, Vector2.One * 135, "OK", font, Vector2.One, Color.White, gameManager.PlayerRevive));
            this.gameState = gameState;
        }
        private void BackToMenu()
        {
            this.gameState = GameState.Menu;

            menuManager.LoadMainMenu();
        }
        #endregion
    }
}