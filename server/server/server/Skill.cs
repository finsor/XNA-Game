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
    class Skill : OnlineAnimation
    {
        public const float MAX_DISTANCE = 300f;
        public const int REQUIRED_MP = 50;

        private OnlinePlayer target;
        private int life_timer;
        public SoundEffects launch_SoundEffect { get; private set; }
        public SoundEffects hit_SoundEffect { get; private set; }

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
                return (this.target != null && Vector2.Distance(this.position, target.position) <= 10f);
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
        public Skill(Folders folder, SoundEffects launch, SoundEffects hit)
            : base(folder, Vector2.Zero, 10)
        {
            this.launch_SoundEffect = launch;
            this.hit_SoundEffect = hit;
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
                    MoveOnX_Axis(isObjectLookingLeft);
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

        public void SetTarget(OnlinePlayer player)
        {
            this.target = player;

            if (target == null)
            {
                base.state = States.hit;
                base.index = 0;
            }
        }

        public void Launch(Vector2 startingPosition)
        {
            this.position = startingPosition;
            this.life_timer = 30;
            base.state = States.launched;

            this.Update();
        }

        public OnlinePlayer GetTarget()
        {
            return (this.target);
        }
    }
}
