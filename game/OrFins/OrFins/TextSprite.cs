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
    class TextSprite : Sprite
    {
        #region Data
        private SpriteFont font;
        private string text;
        private Vector2 relativePosition;
        #endregion

        #region Construction
        public TextSprite(SpriteBatch spriteBatch, Vector2 relativePosition, Color color, string text, SpriteFont font)
            : base(spriteBatch, null, relativePosition, null, color, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f)
        {
            this.font = font;
            this.text = text;
            this.relativePosition = relativePosition;
        }
        #endregion

        #region Drawing functions
        public override void DrawObject(Vector2 windowScale)
        {
            spriteBatch.DrawString(font, text, position * windowScale, color, rotation, origin, scale * windowScale, effects, layerDepth);
        }
        #endregion

        #region Update functions
        public virtual void Update(Vector2 buttonPosition)
        {
            base.position = buttonPosition + relativePosition;
        }
        public virtual void Update(string text)
        {
            this.text = text;
        }
        #endregion
    }
}
