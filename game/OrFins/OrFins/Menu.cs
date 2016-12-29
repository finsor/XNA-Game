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

namespace OrFins
{
    class Menu : Sprite
    {
        #region Data
        private Rectangle destinationRectangle;
        private MouseState previousMS;
        private MouseState currentMS;

        private ButtonDrawer DRAW_BUTTONS;
        private ButtonUpdater UPDATE_BUTTONS;
        #endregion

        #region Construction
        public Menu(SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle)
            : base(spriteBatch, texture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0)
        {
            this.destinationRectangle = destinationRectangle;
        }
        #endregion

        #region Draw functions
        public override void DrawObject(Vector2 windowScale)
        {
            base.DrawObject(destinationRectangle, windowScale);

            DRAW_BUTTONS(windowScale);
        }
        #endregion

        #region Update functions
        public void Update(Vector2 windowScale)
        {
            previousMS = currentMS;
            currentMS = Mouse.GetState();

            UPDATE_BUTTONS(previousMS, currentMS, windowScale);
        }
        #endregion

        #region Public functions
        public void AddButtons(params Button[] buttons)
        {
            foreach (Button button in buttons)
            {
                DRAW_BUTTONS += button.DrawObject;
                UPDATE_BUTTONS += button.Update;
            }
        }
        #endregion
    }
}
