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
    class Skill : Animation
    {
        #region Data
        public const float MAX_DISTANCE = 300f;
        public const int REQUIRED_MP = 50;

        private LivingObject target;
        public Vector2 onlineTarget { get; private set; }
        private int life_timer;
        private SoundEffects launchSoundEffect;
        private SoundEffects hitSoundEffect;
        #endregion

        #region Properties
        public bool IsLaunched
        {
            get
            {
                return (life_timer > 0 && state == States.launched);
            }
        }
        public bool HasReachedTarget
        {
            get
            {
                return
                    (this.target != null && Vector2.Distance(this.position, target.position) <= 10f) // For offline targets
                    ||
                    (this.onlineTarget != Vector2.Zero && Vector2.Distance(this.position, onlineTarget) <= 10f); // For online targets
            }
        }
        public bool IsHitAnimation
        {
            get
            {
                return (base.state == States.hit && !base.HasFinishedAnimation);
            }
        }
        #endregion

        #region Construction
        public Skill(Folders folder, SpriteBatch spriteBatch, SoundEffects launch, SoundEffects hit)
            : base(folder, spriteBatch, Vector2.Zero, Color.White, Vector2.One, SpriteEffects.None, 10)
        {
            this.launchSoundEffect = launch;
            this.hitSoundEffect = hit;
            this.onlineTarget = Vector2.Zero;

            base.state = States.launched;
        }
        #endregion

        #region Update functions
        public void Update(bool isObjectLookingLeft)
        {
            if (base.state == States.launched)
            {
                // If skill has a LivingObject target, then move toward it
                if (target != null)
                {
                    this.position += Vector2.Normalize(Vector2.Subtract(target.position, this.position)) * 10;
                }
                else
                {
                    if (onlineTarget != Vector2.Zero)
                    {
                        this.position += Vector2.Normalize(Vector2.Subtract(onlineTarget, this.position)) * 10;
                    }
                    else
                    {
                        MoveOnX_Axis(isObjectLookingLeft);
                    }
                }
                life_timer--;
            }

            base.Update();
        }
        private void MoveOnX_Axis(bool isObjectLookingLeft)
        {
            if (isObjectLookingLeft)
            {
                this.position -= Vector2.UnitX * 10;
            }
            else
            {
                this.position += Vector2.UnitX * 10;
            }
        }
        #endregion

        #region Public functions
        public void SetTarget(LivingObject livingObject)
        {
            this.target = livingObject;

            if (target == null)
            {
                base.state = States.hit;
                base.index = 0;
            }
        }

        public void SetTarget(Vector2 target)
        {
            this.onlineTarget = target;
        }

        public void Launch(Vector2 startingPosition)
        {
            this.position = startingPosition;
            this.life_timer = 30;
            base.state = States.launched;

            SoundDictionary.Play(this.launchSoundEffect);

            this.Update();
        }

        public LivingObject GetTarget()
        {
            return (this.target);
        }

        public void PlayHit()
        {
            SoundDictionary.Play(this.hitSoundEffect);
        }

        public void SendDrawingData(OnlineClient onlineClient)
        {
            onlineClient.Send(
                base.state.ToString(),
                base.index.ToString(),
                base.position.X.ToString(),
                base.position.Y.ToString());
        }
        #endregion
    }
}
