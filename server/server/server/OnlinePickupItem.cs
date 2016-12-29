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
    class OnlinePickupItem : OnlineSprite
    {
        #region Data
        public Folders folder;
        public States state;

        private const float MAX_HEIGHT = -0.8f;
        private const float ACCELERATION = 0.02f;

        private float upORdown;
        private Vector2 speed;

        public int maxTimeToLive;
        public int life_timer;

        public int money { get; private set; }

        #endregion

        #region properties
        public bool LifeTimerEnded
        {
            get
            {
                return (life_timer == 0);
            }
        }
        #endregion

        #region ctor
        public OnlinePickupItem(Folders folder, States state, Vector2 position, int maxTimeToLive, int money)
            : base(position)
        {
            this.folder = folder;
            this.state = state;
            this.money = money;

            this.maxTimeToLive = maxTimeToLive;
            this.life_timer = maxTimeToLive;
            this.speed = Vector2.Zero;
            this.upORdown = -1;

            base.sourceRectangle = SpritesDictionary.dictionary[folder][state].rectangles[0];
            base.origin = SpritesDictionary.dictionary[folder][state].origins[0];
        }

        #endregion

        public override void Update()
        {
            ProcessMovement();

            life_timer--;

            base.Update();
        }

        private void ProcessMovement()
        {
            position -= speed * upORdown;
            speed += ACCELERATION * Vector2.UnitY * upORdown;

            if (speed.Y < MAX_HEIGHT || speed.Y > 0)
            {
                upORdown *= -1;
                speed = Vector2.Clamp(speed, Vector2.UnitY * MAX_HEIGHT, Vector2.Zero);
            }
        }


    }
}
