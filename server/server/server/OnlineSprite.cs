using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace server
{
    class OnlineSprite : IOnlineFocus
    {
        #region DATA
        // Object - relevant data
        public float topPixel;
        public float bottomPixel;
        public float rightmostPixel;
        public float leftmostPixel;
        public Rectangle surroundingRectangle;

        // Sprite - relevant data
        public Texture2D texture { get; set; }
        public Rectangle? sourceRectangle { get; set; }
        public Vector2 position { get; set; }
        public Vector2 origin { get; set; }
        public SpriteEffects effects { get; set; }
        #endregion

        #region Properties

        public bool IsLookingLeft
        {
            get
            {
                return (effects == SpriteEffects.None);
            }
            protected set
            {
                effects = value ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            }
        }
        public float Height
        {
            get
            {
                return (this.texture.Height);
            }
        }
        public float Width
        {
            get
            {
                return (this.texture.Width);
            }
        }
        #endregion

        #region Construction
        public OnlineSprite(Vector2 position, Texture2D texture = null, Rectangle? sourceRectangle = null)
        {
            this.position = position;
            this.texture = texture;
            this.sourceRectangle = sourceRectangle;
        }
        #endregion

        #region Update
        public virtual void Update()
        {
            this.topPixel = this.position.Y - sourceRectangle.Value.Height;
            this.bottomPixel = this.position.Y;
            this.leftmostPixel = position.X - origin.X;
            this.rightmostPixel = leftmostPixel + sourceRectangle.Value.Width;

            this.surroundingRectangle = new Rectangle(
                (int)this.leftmostPixel,
                (int)this.topPixel,
                (int)(this.rightmostPixel - this.leftmostPixel),
                (int)(this.bottomPixel - this.topPixel));
        }
        #endregion
    }
}
