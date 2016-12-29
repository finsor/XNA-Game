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
    public delegate void ButtonUpdater(MouseState previousMouseState, MouseState currentMouseState, Vector2 boxPosition);
    public delegate void ButtonDrawer(Vector2 windowScale);

    class DialogBox : Sprite
    {
        private ButtonUpdater UPDATE_BUTTONS;
        private ButtonDrawer DRAW_BUTTONS;
        private List<Button> buttons;
        protected SpriteFont font;

        public DialogBox(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 scale, SpriteFont font)
            : base(spriteBatch, texture, position, new Rectangle(0, 0, texture.Width, texture.Height), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0)
        {
            this.InitializeBoundingRectangle();
            this.font = font;
            this.buttons = new List<Button>();

            base.isDrawable = false;

            #region create exit button
            Vector2 exitButtonPosition = new Vector2(base.texture.Width - 15, 29) * this.scale;
            this.AddButton(new Button(spriteBatch, Folders.regularButton, exitButtonPosition, "X", font, new Vector2(0.3f, 1), new Color(255, 0, 0, 200), this.Exit));
            #endregion
        }

        #region Drawing functions
        public override void DrawObject(Vector2 windowScale)
        {
            base.DrawObject(windowScale);

            DRAW_BUTTONS(windowScale);
        }
        #endregion

        #region Update functions
        public virtual void Update(MouseState previousMouseState, MouseState currentMouseState, Vector2 windowScale)
        {
            DragAttempt(previousMouseState, currentMouseState, windowScale);

            UPDATE_BUTTONS(previousMouseState, currentMouseState, windowScale);
        }

        private void DragAttempt(MouseState previousMouseState, MouseState currentMouseState, Vector2 windowScale)
        {
            Vector2 current_mouse_position = currentMouseState.Vector() / windowScale;
            Vector2 previous_mouse_position = previousMouseState.Vector() / windowScale;

            // If clicking continiously and mouse's position inside bounding rectangle
            // Then update locations of window and buttons
            if (previousMouseState.LeftPressed() && currentMouseState.LeftPressed() && surroundingRectangle.Contains(current_mouse_position))
            {
                Vector2 positionChange = current_mouse_position - previous_mouse_position;

                this.position += positionChange;

                foreach (Button button in buttons)
                    button.position += positionChange;

                this.InitializeBoundingRectangle();
            }
        }
        private void InitializeBoundingRectangle()
        {
            base.surroundingRectangle = new Rectangle(
                (int)base.position.X,
                (int)base.position.Y,
                (int)base.texture.Width,
                (int)base.texture.Height);
        }
        #endregion

        private void Exit()
        {
            this.isDrawable = false;
        }

        public void AddButton(Button button)
        {
            button.position += this.position;
            this.buttons.Add(button);
            DRAW_BUTTONS += button.DrawObject;
            UPDATE_BUTTONS += button.Update;
        }
    }
}
