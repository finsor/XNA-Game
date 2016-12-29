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
    class PickupItem : Sprite
    {
        #region Data
        private const float MAX_SPEED = -0.8f;
        private const float ACCELERATION = 0.02f;
        private float upORdown;
        private Vector2 speed;
        private Bar existenceBar;
        private int maxTimeToLive;
        private int life_timer;

        public int money { get; private set; }
        #endregion

        #region Properties
        public bool LifeTimerEnded
        {
            get
            {
                return (life_timer == 0);
            }
        }
        #endregion

        #region Construction
        public PickupItem(SpriteBatch spriteBatch, Folders pickupFolder, States itemName, Vector2 position, int maxTimeToLive, int money)
            : base(pickupFolder, itemName, spriteBatch, position, Color.White, 0f, Vector2.One, SpriteEffects.None, 0)
        {
            this.maxTimeToLive = maxTimeToLive;
            this.life_timer = maxTimeToLive;
            this.money = money;
            this.speed = Vector2.Zero;
            this.upORdown = -1;

            this.existenceBar = new Bar(Folders.hud, States.xpBar, spriteBatch, this.color, this.rotation, new Vector2(0.5f, 0.01f), this.effects, this.layerDepth, new Rectangle());
        }
        #endregion

        #region Drawing functions
        public override void DrawObject(Vector2 windowScale)
        {
            // Draw the item
            base.DrawObject(windowScale);
            
            // Draw the time to live bar
            existenceBar.DrawObject(windowScale);
        }
        #endregion

        #region Update functions
        public override void Update()
        {
            ProcessMovement();

            life_timer--;
            existenceBar.Update(this.surroundingRectangle, this.life_timer, this.maxTimeToLive);

            base.Update();
        }

        // Function is used to process the movement of the pickup item
        private void ProcessMovement()
        {
            position -= speed * upORdown;
            speed += ACCELERATION * Vector2.UnitY * upORdown;

            if (speed.Y < MAX_SPEED || speed.Y > 0)
            {
                upORdown *= -1;
                speed = Vector2.Clamp(speed, Vector2.UnitY * MAX_SPEED, Vector2.Zero);
            }
        }
        #endregion
    }
}
