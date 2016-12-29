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
    class Level : Sprite
    {
        #region Data
        Rectangle destinationRectangle;
        ImageProcessor page;
        #endregion

        #region Construction
        public Level(SpriteBatch spriteBatch, Color color, float rotation, Vector2 hudScale, SpriteEffects effects, float layerDepth,
                    Folders folder, States state, Rectangle destinationRectangle)
            : base(folder, state, spriteBatch, Vector2.Zero, Color.White, 0f, Vector2.One, SpriteEffects.None, 0f)
        {
            this.destinationRectangle = destinationRectangle;
            this.page = SpritesDictionary.dictionary[folder][state];
        }
        #endregion

        #region Draw functions
        public void DrawObject(int level, Vector2 windowScale)
        {
            string levelS = level.ToString();

            Rectangle drawRectangle = this.destinationRectangle;

            foreach(char digit in levelS)
            {
                sourceRectangle = page.rectangles[digit - '0'];
                base.DrawObject(drawRectangle, windowScale);
                drawRectangle.X += 13 * (int)scale.X;
            }
        }
        #endregion
    }
}
