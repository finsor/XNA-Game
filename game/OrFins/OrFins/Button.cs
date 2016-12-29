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
    class Button : Sprite
    {
        #region Data
        private States state;
        private Folders folder;
        private TextSprite textSprite;
        private bool isEquipmentButton;
        private Action onClick;
        #endregion

        #region Properties
        public bool isClicked
        {
            get
            {
                return (onClick != null && state == States.clicked);
            }
        }
        public bool isHovered
        {
            get
            {
                return (state == States.buttonHover);
            }
        }
        #endregion

        #region Construction
        public Button(SpriteBatch spriteBatch, Folders folder, Vector2 position, string text, SpriteFont font, Vector2 scale, Color color, params Action[] actions)
            : base(folder, States.notClicked, spriteBatch, position, color, 0f, scale, SpriteEffects.None, 0)
        {
            this.folder = folder;

            if (font != null)
            {
                this.textSprite = new TextSprite(spriteBatch, -new Vector2(text.Length * 4f, this.origin.Y - 7 / this.scale.Y), Color.White, text, font);
            }

            foreach (Action act in actions)
                this.onClick += act;

            this.isEquipmentButton = false;
        }
        public Button(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 scale, Color color, params Action[] actions)
            : base(spriteBatch, texture, position, null, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0)
        {
            this.isEquipmentButton = true;

            foreach (Action act in actions)
                this.onClick += act;
        }

        #endregion

        #region Drawing functions
        public override void DrawObject(Vector2 windowScale)
        {
            if (sourceRectangle != null)
                base.DrawObject(windowScale);

            if (textSprite != null)
            {
                textSprite.DrawObject(windowScale);
            }
        }
        #endregion

        #region Update functions
        public void Update(MouseState previousMouseState, MouseState currentMouseState, Vector2 windowScale)
        {
            GetState(previousMouseState, currentMouseState, windowScale);

            if (textSprite != null)
            {
                textSprite.Update(this.position);
            }

            if (base.sourceRectangle != null)
            {
                base.Update();
            }

            if (isClicked)
                onClick();
        }
        private void GetState(MouseState previousMouseState, MouseState currentMouseState, Vector2 windowScale)
        {
            Vector2 mousePosition = currentMouseState.Vector() / windowScale;

            if (this.surroundingRectangle.Contains(mousePosition))
            {
                this.state = States.buttonHover;
                if (previousMouseState.LeftPressed() && !currentMouseState.LeftPressed())
                {
                    this.state = States.clicked;
                }
            }
            else
            {
                this.state = States.notClicked;
            }

            if (!isEquipmentButton)
            {
                this.texture = SpritesDictionary.dictionary[folder][state].texture;
            }
        }
        #endregion

        #region Public functions
        public void ChangeAppearance(ImageProcessor page)
        {
            if (page == null)
            {
                this.texture = null;
                this.sourceRectangle = null;
            }
            else
            {
                this.texture = page.texture;
                this.sourceRectangle = page.rectangles[0];
                this.origin = page.origins[0];
                this.scale = Vector2.One;
            }
        }
        public void SetFolder(Folders folder)
        {
            this.folder = folder;
        }
        #endregion
    }
}
