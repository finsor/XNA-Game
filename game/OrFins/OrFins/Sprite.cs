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
    class Sprite : IFocus
    {
        #region DATA

        // Object - relevant data
        public SpriteBatch spriteBatch { get; private set; }
        public Texture2D texture { get; protected set; }
        public Vector2 position { get; set; }
        public Color color { get; protected set; }
        public Vector2 origin { get; protected set; }
        public float rotation { get; private set; }
        public Vector2 scale { get; protected set; }
        public SpriteEffects effects { get; protected set; }
        public float layerDepth { get; private set; }

        public bool isDrawable = true;
        public float topPixel;
        public float bottomPixel;
        public float rightmostPixel;
        public float leftmostPixel;

        // Sprite - relevant data
        public Rectangle? sourceRectangle { get; protected set; }
        //private SpriteEffects spriteEffects;
        public Rectangle surroundingRectangle;

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
                return (this.texture.Height * this.scale.Y);
            }
        }
        public float Width
        {
            get
            {
                return (this.texture.Width * this.scale.X);
            }
        }
        #endregion

        #region Construction
        public Sprite(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            this.spriteBatch = spriteBatch;
            this.texture = texture;
            this.position = position;
            this.sourceRectangle = sourceRectangle;
            this.color = color;
            this.rotation = rotation;
            this.origin = origin;
            this.scale = scale;
            this.effects = effects;
            this.layerDepth = layerDepth;
        }
        public Sprite(Folders folder, States state, SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            this.spriteBatch = spriteBatch;
            this.position = position;
            this.color = color;
            this.rotation = rotation;
            this.scale = scale;
            this.effects = effects;
            this.layerDepth = layerDepth;

            ImageProcessor page = SpritesDictionary.dictionary[folder][state];

            this.texture = page.texture;
            this.origin = page.origins[0];
            this.sourceRectangle = page.rectangles[0];
        }
        public Sprite(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
        }
        #endregion

        #region Drawing functions
        public virtual void DrawObject(Vector2 windowScale)
        {
            if (isDrawable)
            {
                this.spriteBatch.Draw(texture, position * windowScale, sourceRectangle, color, rotation, origin, scale * windowScale, effects, layerDepth);
            }
        }
        public virtual void DrawObject(Rectangle destinationRectangle, Vector2 windowScale)
        {
            Rectangle scaled_destinationRectangle = destinationRectangle.Multiply(windowScale);

            this.spriteBatch.Draw(texture, scaled_destinationRectangle, sourceRectangle, Color.White);
        }
        public void DrawSurroundingRectangle(Vector2 windowScale)
        {
            this.spriteBatch.Draw(Service.rectangleTexture, surroundingRectangle.Multiply(windowScale), Color.White);
        }
        #endregion

        #region Update functions
        public virtual void Update()
        {
            UpdateLimits();
        }
        private void UpdateLimits()
        {
            this.topPixel = this.position.Y - sourceRectangle.Value.Height * this.scale.Y;
            this.bottomPixel = this.position.Y;
            this.leftmostPixel = position.X - origin.X * this.scale.X;
            this.rightmostPixel = leftmostPixel + sourceRectangle.Value.Width * this.scale.X;

            this.surroundingRectangle = new Rectangle(
                (int)this.leftmostPixel,
                (int)this.topPixel,
                (int)(this.rightmostPixel - this.leftmostPixel),
                (int)(this.bottomPixel - this.topPixel));

        }
        #endregion
    }
}