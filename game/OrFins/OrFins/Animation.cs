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
    class Animation : Sprite
    {

        #region DATA
        private int slow;
        protected int slowRate;
        private ImageProcessor spritesPage;
        protected Folders folder { get; set; }
        public States state { get; set; }
        public int index { get; protected set; }
        public bool animate { get; protected set; }

        public Circle headCircle { get; private set; }
        public Circle coreCircle { get; private set; }
        public Circle legsCircle { get; private set; }
        #endregion

        #region Properties
        public bool IsLastAnimationFrame
        {
            get
            {
                return (index == spritesPage.rectangles.Count - 1);
            }
        }
        public bool HasFinishedAnimation
        {
            get
            {
                return (index == spritesPage.rectangles.Count);
            }
        }
        #endregion

        #region Construction

        public Animation(Folders name, SpriteBatch spriteBatch, Vector2 position, Color color, Vector2 scale, SpriteEffects effects, int slowRate)
            : base(spriteBatch, null, position, null, color, 0f, Vector2.Zero, scale, effects, 0f)
        {
            this.folder = name;
            this.slowRate = slowRate;

            this.index = 0;
            this.slow = 0;

            this.animate = true;
        }

        #endregion

        #region Drawing functions
        public override void DrawObject(Vector2 windowScale)
        {
            base.DrawObject(windowScale);

            if (animate && ++slow == slowRate)
            {
                index++;
                slow = 0;
            }
        }
        public void DrawCircles(Vector2 windowScale)
        {
            if (headCircle != null)
            {
                headCircle.DrawObject(base.spriteBatch, windowScale);
            }

            if (coreCircle != null)
            {
                coreCircle.DrawObject(base.spriteBatch, windowScale);
            }

            if (legsCircle != null)
            {
                legsCircle.DrawObject(base.spriteBatch, windowScale);
            }
        }
        #endregion

        #region Update functions
        public override void Update()
        {
            this.spritesPage = SpritesDictionary.dictionary[folder][state];
            this.index %= spritesPage.rectangles.Count;

            base.texture = spritesPage.texture;
            base.sourceRectangle = spritesPage.rectangles[index];
            base.origin = spritesPage.origins[index];

            if (!IsLookingLeft)
            {
                origin = new Vector2(sourceRectangle.Value.Width - origin.X, origin.Y);
            }

            this.UpdateCircles();

            base.Update();
        }
        private void UpdateCircles()
        {
            if (spritesPage.headCircles != null)
            {
                headCircle = spritesPage.headCircles[index].GetActualCircle(this);
            }

            if (spritesPage.coreCircles != null)
            {
                coreCircle = spritesPage.coreCircles[index].GetActualCircle(this);
            }

            if (spritesPage.legsCircles != null)
            {
                legsCircle = spritesPage.legsCircles[index].GetActualCircle(this);
            }
        }
        #endregion
    }
}
