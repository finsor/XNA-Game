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
using System.Windows.Forms;

namespace OrFins
{
    class NPC : Animation
    {
        #region DATA
        private MouseState previousMouseState;
        private MouseState currentMouseState;
        private States buttonState;
        private Action onClick;
        #endregion

        #region Properties
        private bool IsClicked
        {
            get
            {
                return (this.buttonState == States.clicked && onClick != null);
            }
        }
        #endregion

        #region Construction
        public NPC(Folders folder, States npcName, SpriteBatch spriteBatch, Vector2 position, Color color, Vector2 scale, int slowRate, params Action[] onClick)
            : base(folder, spriteBatch, position, color, scale, SpriteEffects.None, slowRate)
        {
            this.state = npcName;

            foreach (Action act in onClick)
            {
                this.onClick += act;
            }
        }
        #endregion

        #region Update functions
        public virtual void Update(Vector2 windowScale, Camera camera)
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            GetState(windowScale, camera);

            if (IsClicked)
            {
                onClick();
            }

            base.Update();
        }

        private void GetState(Vector2 windowScale, Camera camera)
        {
            Vector2 mousePosition = currentMouseState.Vector() / windowScale;
            mousePosition += (camera.position - camera.originalPosition);

            if (this.surroundingRectangle.Contains(mousePosition))
            {
                this.buttonState = States.buttonHover;
                if (previousMouseState.LeftPressed() && !currentMouseState.LeftPressed())
                {
                    this.buttonState = States.clicked;
                }
            }
            else
            {
                this.buttonState = States.notClicked;
            }
        }
        #endregion
    }
}
