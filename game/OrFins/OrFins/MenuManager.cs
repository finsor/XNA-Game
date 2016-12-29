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
using System.Timers;
using MB = System.Windows.Forms.MessageBox;

namespace OrFins
{
    class MenuManager
    {
        #region Data
        private SpriteBatch spriteBatch;
        private Menu mainMenu;
        private Menu singlePlayerMenu;
        private Menu processedMenu;
        #endregion

        #region Properties
        public bool IsProcessing { get; private set; }
        #endregion

        #region Construction
        public MenuManager(Texture2D menuTexture, SpriteBatch spriteBatch, SpriteFont font, Action newSingleGame, Action loadSingleGame, Action enterMultiPlayer, Action exit)
        {
            this.spriteBatch = spriteBatch;

            this.CreateMainMenu(menuTexture, spriteBatch, font, enterMultiPlayer, exit);
            this.CreateSinglePlayerMenu(menuTexture, spriteBatch, font, newSingleGame, loadSingleGame);

            this.LoadMainMenu();
        }
        #endregion

        #region Draw functions
        public void Draw(Vector2 windowScale)
        {
            spriteBatch.Begin();

            this.processedMenu.DrawObject(windowScale);

            spriteBatch.End();
        }
        #endregion

        #region Update functions
        public void Update(Vector2 windowScale)
        {
            this.processedMenu.Update(windowScale);
        }
        #endregion

        #region Public functions
        public void LoadMainMenu()
        {
            SoundDictionary.SetBackGroundMusic(BGM.Menu);
            ChangeToMainMenu();
        }
        #endregion

        #region Private functions
        private void ChangeToMainMenu()
        {
            this.processedMenu = mainMenu;
        }
        private void ChangeToSinglePlayerMenu()
        {
            this.processedMenu = singlePlayerMenu;
        }
        private void CreateMainMenu(Texture2D menuTexture, SpriteBatch spriteBatch, SpriteFont font, Action enterMultiPlayer, Action exit)
        {
            Button singlePlayer = new Button(spriteBatch, Folders.regularButton, new Vector2(Service.screenWidth / 2, Service.screenHeight / 2 - 60), "SinglePlayer", font, Vector2.One, Color.White, this.ChangeToSinglePlayerMenu);
            Button multiPlayer = new Button(spriteBatch, Folders.regularButton, new Vector2(Service.screenWidth / 2, Service.screenHeight / 2), "MultiPlayer", font, Vector2.One, Color.White, enterMultiPlayer);
            Button exitButton = new Button(spriteBatch, Folders.regularButton, new Vector2(Service.screenWidth / 2, Service.screenHeight / 2 + 60), "Exit", font, Vector2.One, Color.White, exit);

            Rectangle menu_Rec = new Rectangle(0, 0, Service.screenWidth, Service.screenHeight);
            this.mainMenu = new Menu(spriteBatch, menuTexture, menu_Rec);
            this.mainMenu.AddButtons(singlePlayer, multiPlayer, exitButton);
        }
        private void CreateSinglePlayerMenu(Texture2D menuTexture, SpriteBatch spriteBatch, SpriteFont font, Action newSingleGame, Action loadSingleGame)
        {
            Button newGame = new Button(spriteBatch, Folders.regularButton, new Vector2(Service.screenWidth / 2, Service.screenHeight / 2 - 60), "New Game", font, Vector2.One, Color.White, newSingleGame);
            Button loadGame = new Button(spriteBatch, Folders.regularButton, new Vector2(Service.screenWidth / 2, Service.screenHeight / 2), "Load Game", font, Vector2.One, Color.White, loadSingleGame);
            Button backToMenu = new Button(spriteBatch, Folders.regularButton, new Vector2(Service.screenWidth / 2, Service.screenHeight / 2 + 60), "Back", font, Vector2.One, Color.White, this.ChangeToMainMenu);

            Rectangle menu_Rec = new Rectangle(0, 0, Service.screenWidth, Service.screenHeight);
            this.singlePlayerMenu = new Menu(spriteBatch, menuTexture, menu_Rec);
            this.singlePlayerMenu.AddButtons(newGame, loadGame, backToMenu);
        }
        #endregion
    }
}
