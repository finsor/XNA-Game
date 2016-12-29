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
    class Bar : Sprite
    {
        #region DATA
        private Rectangle destinationRectangle;
        #endregion

        #region Construction
        public Bar(Folders folder, States barKind, SpriteBatch spriteBatch, Color color, float rotation, Vector2 hudScale, SpriteEffects effects, float layerDepth,
                    Rectangle destinationRectangle)
            : base(folder, barKind, spriteBatch, Vector2.Zero, color, 0f, hudScale, effects, layerDepth)
        {
            this.destinationRectangle = destinationRectangle;
        }
        #endregion

        #region Draw functions
        public override void DrawObject(Vector2 windowScale)
        {
            base.DrawObject(destinationRectangle, windowScale);
        }
        #endregion

        #region Update functions
        public void Update(Rectangle item_rectangle, int val, int max)
        {
            this.destinationRectangle.X = item_rectangle.X;
            this.destinationRectangle.Y = item_rectangle.Y - 10;
            this.destinationRectangle.Width = val * item_rectangle.Width / max;
            this.destinationRectangle.Height = 5;
        }

        public void Update(int data, int max)
        {
            destinationRectangle.Width = (int)(data * 100 / max * this.scale.X);
        }
        #endregion
    }
}
