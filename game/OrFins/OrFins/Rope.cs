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
    class Rope : Sprite
    {
        #region DATA
        public Folders folder;
        public int length;
        #endregion

        #region Construction
        public Rope(Folders name, SpriteBatch spriteBatch, Platform platform, int offset, int length)
            : base(spriteBatch, null, platform.CreatePositionUsingOffset(offset), null, platform.color, platform.rotation, platform.origin, platform.scale, platform.effects, platform.layerDepth)
        {
            this.folder = name;
            this.length = length;

            InitializeLimits(platform);
        }

        private void InitializeLimits(Platform platform)
        {
            base.topPixel = this.position.Y;

            Texture2D texture = SpritesDictionary.dictionary[this.folder][States.top].texture;
            this.position = new Vector2(this.position.X, this.position.Y + texture.Height * 3 / 4);

            base.bottomPixel =
                this.position.Y +
                SpritesDictionary.dictionary[folder][States.hover].rectangles[0].Height * this.length;

            base.leftmostPixel =
                this.position.X -
                SpritesDictionary.dictionary[folder][States.hover].origins[0].X;

            base.rightmostPixel =
                leftmostPixel +
                SpritesDictionary.dictionary[folder][States.hover].rectangles[0].Width;

            base.surroundingRectangle = new Rectangle(
                (int)base.leftmostPixel,
                (int)base.topPixel,
                (int)(base.rightmostPixel - base.leftmostPixel),
                (int)(base.bottomPixel - base.topPixel));
        }
        #endregion

        #region Drawing functions
        public override void DrawObject(Vector2 windowScale)
        {
            ImageProcessor page;
            Vector2 savePosition = this.position;

            // Draw top of rope
            page = SpritesDictionary.dictionary[folder][States.top];

            base.texture = page.texture;
            base.origin = page.origins[0];

            base.DrawObject(windowScale);

            // Draw middle of rope
            page = SpritesDictionary.dictionary[folder][States.hover];

            base.texture = page.texture;
            base.origin = page.origins[0];

            for (int i = 0; i < this.length; i++)
            {
                base.position += new Vector2(0, texture.Height * this.scale.Y);
                base.DrawObject(windowScale);
            }

            // Draw bottom of rope
            page = SpritesDictionary.dictionary[folder][States.bottom];

            base.texture = page.texture;
            base.origin = page.origins[0];

            base.position += new Vector2(0, texture.Height * this.scale.Y);
            base.DrawObject(windowScale);

            this.position = savePosition;
        }
        #endregion
    }
}
