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
    enum Folders
    {
        pickup,
        teddy, worm, monkey, Drake,
        Banana, Fireball,
    }

    enum States
    {
        stand, alert, walk, prone, jump, onRope, die,
        stab1, stab2, stab3, swing1, swing2, swing3,
        money,
        mobSkill,
        launched, hit
    }

    class OnlineAnimation : OnlineSprite
    {
        #region DATA
        public Folders folder { get; set; }
        public States state { get; set; }
        public int index { get; protected set; }
        ImageProcessor spritesPage;

        public int slow;
        public int slowRate;
        public bool animate = true;

        public OnlineCircle headCircle;
        public OnlineCircle coreCircle;
        public OnlineCircle legsCircle;

        public bool IsLastAnimationFrame
        {
            get
            {
                return index == spritesPage.rectangles.Count - 1;
            }
        }

        public bool HasFinishedAnimation
        {
            get
            {
                return index == spritesPage.rectangles.Count;
            }
        }
        #endregion

        #region CTOR

        public OnlineAnimation(Folders folder, Vector2 position, int slowRate)
            : base(position)
        {
            this.folder = folder;
            this.slowRate = slowRate;
        }

        #endregion

        #region Update
        public override void Update()
        {
            this.spritesPage = SpritesDictionary.dictionary[folder][state];
            this.index %= spritesPage.rectangles.Count;

            base.texture = spritesPage.texture;
            base.sourceRectangle = spritesPage.rectangles[index];
            base.origin = spritesPage.origins[index];

            if (base.effects == SpriteEffects.FlipHorizontally)
            {
                origin = new Vector2(sourceRectangle.Value.Width - origin.X, origin.Y);
            }

            if (spritesPage.headCircles != null)
            {
                headCircle = spritesPage.headCircles[index].GetActualCircle(this);
                coreCircle = spritesPage.coreCircles[index].GetActualCircle(this);
                legsCircle = spritesPage.legsCircles[index].GetActualCircle(this);
            }

            if (animate && ++slow == slowRate)
            {
                index++;
                slow = 0;
            }

            base.Update();
        }
        #endregion
    }
}
