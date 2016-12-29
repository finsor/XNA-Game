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
using MessageBox = System.Windows.Forms.MessageBox;

namespace OrFins
{
    struct Sensor
    {
        public Vector2 position { get; set; }
        public bool IsOn { get; set; }
        public void Draw(Texture2D pixel, SpriteBatch spriteBatch, Vector2 windowScale)
        {
            Rectangle rectangle = new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4);

            spriteBatch.Draw(pixel, rectangle.Multiply(windowScale), Color.Red);
        }
    }

    class LivingObject : Animation
    {
        #region DATA

        private const float GRAVITY = 0.4f;
        private const float MAX_FALL_SPEED = 8f;
        private const float KNOCKBACK_MAX_DISTANCE = 8f;

        // Living-relevant data
        public int maxHP { get; set; }
        public int hp { get; set; }

        public int maxMP { get; set; }
        public int mp { get; set; }

        public int strength { get; set; }
        public int defence { get; set; }

        public Sensor leftSensor;
        public Sensor rightSensor;
        public Sensor upSensor;
        public Sensor downSensor;
        public Sensor downLeftSensor;
        public Sensor downRightSensor;

        // Movement-relevant data
        protected BaseKeys baseKeys;

        public Platform basePlat;
        public Vector2 speed { get; set; }

        public bool canJump { get; set; }
        protected bool isJumping { get; private set; }
        public float fallSpeed;

        public bool isOnRope;
        public Vector2 ropeSpeed { get; set; }

        protected bool canAttack;
        protected bool isAttacking;
        protected bool isAlert;
        public bool isHit { get; private set; }
        private Vector2 knockback;
        public bool hasHitDuringCurrentAttack;


        protected int hit_period;
        private States skillState;
        protected Skill skill;

        private SoundEffects deathSoundEffect;
        #endregion

        #region Properties
        public bool IsDead
        {
            get
            {
                return (this.state == States.die);
            }
            set
            {
                this.state = States.die;
            }
        }
        public bool CanBeHit
        {
            get
            {
                return (!this.IsDead && !this.isHit);
            }
        }
        public bool CanLaunchSkill
        {
            get
            {
                return (skill != null && base.state != States.mobSkill);
            }
        }
        public bool IsLaunchingSkill
        {
            get
            {
                return (canAttack && baseKeys.SkillKey() && mp >= Skill.REQUIRED_MP && !IsDead && !skill.IsLaunched);
            }
        }
        public bool AttemptsToPickUp
        {
            get
            {
                return (!IsDead && baseKeys.PickupKey());
            }
        }
        public bool AttemptsToTeleport
        {
            get
            {
                return (!IsDead && baseKeys.UpKey());
            }
        }
        public bool IsSkillLaunched
        {
            get
            {
                return (skill != null && skill.IsLaunched);
            }
        }
        #endregion

        #region Construction

        // CTOR with ropeSpeed
        public LivingObject(Folders name, SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceRectangle,
                            Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, int slowRate,
                            int maxHP, int maxMP, bool canJump, Vector2 speed, Vector2 ropeSpeed, int strength, States skillState, Skill skill, SoundEffects deathSoundEffect)
            : base(name, spriteBatch, position, color, scale, effects, slowRate)
        {
            this.hp = this.maxHP = maxHP;
            this.mp = this.maxMP = maxMP;
            this.canJump = canJump;
            this.speed = speed;
            this.ropeSpeed = ropeSpeed;
            this.strength = strength;
            this.defence = 0;
            this.fallSpeed = 0;
            this.skillState = skillState;
            this.skill = skill;
            this.deathSoundEffect = deathSoundEffect;
        }

        #endregion

        #region Draw functions
        public override void DrawObject(Vector2 windowScale)
        {
            if (skill != null && (skill.IsLaunched || skill.IsHitAnimation))
            {
                skill.DrawObject(windowScale);
            }

            base.DrawObject(windowScale);
        }
        public void DrawSensors(Vector2 windowScale)
        {
            leftSensor.Draw(Service.pixel, spriteBatch, windowScale);
            rightSensor.Draw(Service.pixel, spriteBatch, windowScale);
            upSensor.Draw(Service.pixel, spriteBatch, windowScale);
            downSensor.Draw(Service.pixel, spriteBatch, windowScale);
            downLeftSensor.Draw(Service.pixel, spriteBatch, windowScale);
            downRightSensor.Draw(Service.pixel, spriteBatch, windowScale);
        }
        #endregion

        #region Update functions
        public virtual void Update(GameTime gameTime, Map map)
        {
            if (IsDead)
            {
                StickToTheGround(map);

                isAttacking = false;
                isHit = false;
                //base.state = States.die;
                base.Update(); // Update animation only
                return;
            }

            // Object is not dead

            this.baseKeys.Update();
            ProcessActions(map);

            if (this.isHit)
            {
                ProcessKnockback();
                if (--hit_period == 0)
                {
                    isHit = false;
                }
            }

            // Update the sensors and set current platform
            UpdateSensors(map);
            UpdateBasePlat(map);

            ProcessGravity(map);

            this.position = new Vector2(MathHelper.Clamp(this.position.X, map.leftLimit + (position.X - leftSensor.position.X), map.rightLimit - (rightSensor.position.X - position.X) / 2), this.position.Y);
            //this.position = new Vector2(MathHelper.Clamp(this.position.X, map.leftLimit, map.rightLimit), this.position.Y);
            this.UpdateState();

            if (skill != null && (skill.IsLaunched || skill.IsHitAnimation))
            {
                skill.Update(base.IsLookingLeft);
            }

            base.Update();
        }
        #endregion

        #region private functions
        private void UpdateBasePlat(Map map)
        {
            if (!downSensor.IsOn)
                return;

            this.basePlat = map.mapMask.At(downSensor.position).platform;
        }
        private void UpdateSensors(Map map)
        {
            this.leftSensor.position = new Vector2((this.position.X + this.leftmostPixel) / 2, this.position.Y - 15);
            this.rightSensor.position = new Vector2((this.position.X + this.rightmostPixel) / 2, this.position.Y - 15);
            this.upSensor.position = new Vector2(this.position.X, this.position.Y - 15);
            this.downSensor.position = new Vector2(this.position.X, this.position.Y + 8);
            this.downLeftSensor.position = new Vector2(leftSensor.position.X + 3, downSensor.position.Y);
            this.downRightSensor.position = new Vector2(rightSensor.position.X - 3, downSensor.position.Y);

            this.leftSensor.IsOn = map.IsLand(this.leftSensor.position);
            this.rightSensor.IsOn = map.IsLand(this.rightSensor.position);
            this.upSensor.IsOn = map.IsLand(this.upSensor.position);
            this.downSensor.IsOn = map.IsLand(this.downSensor.position);
            this.downLeftSensor.IsOn = map.IsLand(this.downLeftSensor.position);
            this.downRightSensor.IsOn = map.IsLand(this.downRightSensor.position);
        }
        private void ProcessActions(Map map)
        {
            if (isOnRope)
            {
                fallSpeed = 0;
                if (!baseKeys.UpKey() && !baseKeys.DownKey())
                    base.index = 0;
                if (baseKeys.UpKey())
                {
                    this.position += ropeSpeed;
                }
                if (baseKeys.DownKey())
                {
                    this.position -= ropeSpeed;
                }
            }
            else
            {
                if ((canAttack && (baseKeys.AttackKey() || baseKeys.SkillKey())) || isAttacking)
                {
                    if (!isAttacking)
                    {
                        if (baseKeys.AttackKey())
                        {
                            SoundDictionary.PlayAttack();
                            base.state = States.stab1 + Service.Random(0, 5);
                            hasHitDuringCurrentAttack = false;
                            isAttacking = true;
                            slowRate = 15;
                            base.index = 0;
                        }
                        if (baseKeys.SkillKey())
                        {
                            if (this.IsLaunchingSkill)
                            {
                                base.state = skillState;
                                hasHitDuringCurrentAttack = true;
                                isAttacking = true;
                                //slowRate = 15;

                                base.index = 0;
                            }
                        }
                    }

                    if (base.HasFinishedAnimation)
                    {
                        isAttacking = false;
                    }
                    return;
                }
            }

            if (!isHit)
            {
                foreach (Rope rope in map.ropes)
                {
                    // If player touches rope
                    if (base.surroundingRectangle.Intersects(rope.surroundingRectangle))
                    {
                        {
                            // If player is not on rope and trying to catch it
                            // or if player is on rope and not jumped to side
                            if ((
                                (!isOnRope && baseKeys.RopeKey())
                                ||
                                (isOnRope &&
                                !(baseKeys.JumpKey() && !baseKeys.UpKey() && !baseKeys.DownKey() && (baseKeys.LeftKey() || baseKeys.RightKey())))
                                ))
                            {

                                if (!isOnRope)
                                {
                                    position = new Vector2(rope.position.X, position.Y);
                                }

                                isOnRope = true;
                                isJumping = false;

                                return;
                            }
                        }
                    }
                }
            }

            isOnRope = false;

            // Jump
            if (baseKeys.JumpKey() && !isJumping && downSensor.IsOn)
            {
                fallSpeed = -MAX_FALL_SPEED;
                isJumping = true;


                // Play jump sound only for player
                if (this is Player)
                {
                    SoundDictionary.Play(SoundEffects.Jump);
                }
            }

            // Move left and right
            if (baseKeys.LeftKey() && (!leftSensor.IsOn || (leftSensor.IsOn && rightSensor.IsOn)))
            {
                position -= speed;
            }

            if (baseKeys.RightKey() && (!rightSensor.IsOn || (leftSensor.IsOn && rightSensor.IsOn)))
            {
                position += speed;
            }

        }
        private void UpdateState()
        {
            if (IsDead || isAttacking)
            {
                return;
            }

            //if (isAttacking)
            //{
            //    return;
            //}
            //else
            //{
                base.slowRate = 10;
            //}

            if (baseKeys.LeftKey())
            {
                base.effects = SpriteEffects.None;
            }
            if (baseKeys.RightKey())
            {
                base.effects = SpriteEffects.FlipHorizontally;
            }

            if (isOnRope)
            {
                base.effects = SpriteEffects.None;
                base.state = States.onRope;
                return;
            }

            if (isJumping)
            {
                base.state = States.jump;
                return;
            }

            if (baseKeys.LeftKey() != baseKeys.RightKey())
            {
                base.state = States.walk;
                return;
            }

            base.state = States.stand;

            if (isHit || isAlert)
            {
                base.state = States.alert;
            }

            if (baseKeys.DownKey())
            {
                base.state = States.prone;
            }
        }
        private void StickToTheGround(Map map)
        {
            float height = 0;

            while (!map.IsLand(this.position + Vector2.UnitY * height))
            {
                height++;
            }

            this.position += Vector2.UnitY * height;
        }
        #endregion

        #region protected functions
        protected void ProcessGravity(Map map)
        {
            if (isOnRope)
            {
                return;
            }

            // When do I fall?
            if (fallSpeed < 0 ||                                              // when rising up
                (fallSpeed > 0 && !(downSensor.IsOn && !upSensor.IsOn)))       // or falling down while not hitting ground)
            {
                position += new Vector2(0, fallSpeed);
                fallSpeed = MathHelper.Clamp(fallSpeed + GRAVITY, -MAX_FALL_SPEED, MAX_FALL_SPEED);
            }
            else
            {
                // Look for closest ground's highest pixel
                for (int y = (int)downSensor.position.Y; y > 0; y--)
                {
                    if (!map.IsLand(new Vector2(downSensor.position.X, y - 1)))
                    {
                        this.position = new Vector2(downSensor.position.X, y);
                        break;
                    }
                }

                fallSpeed = 0;
                isJumping = false;

            }
        }
        protected void ProcessKnockback()
        {
            // Apply knockback
            this.position += knockback;

            // Update knockback
            if (knockback.X != 0)
                knockback -= Vector2.Normalize(knockback);
        }
        protected void UpdateStrength(int strength)
        {
            this.strength += strength;

            if (this.strength < 0)
            {
                MessageBox.Show("SYSTEM ERROR: STRENGTH VALUE INVALID");
            }
        }
        protected void UpdateDefence(int difference)
        {
            this.defence += difference;
            if (this.defence < 0 || this.defence > 100)
            {
                MessageBox.Show("SYSTEM ERROR: DEFENCE VALUE INVALID");
            }
        }
        #endregion

        #region public functions
        public void Hit(int damage, int knockback_direction)
        {
            if (IsDead || damage == 0)
                return;

            SoundDictionary.PlayHit();

            this.knockback = Vector2.UnitX * KNOCKBACK_MAX_DISTANCE * knockback_direction;

            damage *= (100 - this.defence) / 100;
            ReduceHP(damage);
        }
        public void ReduceHP(int damage)
        {
            this.hp = (int)MathHelper.Clamp(this.hp - damage, 0, this.maxHP);
            this.isHit = true;
            this.hit_period = 50;

            if (this.hp < 1)
            {
                this.Kill();
            }
        }
        public void Kill()
        {
            IsDead = true;
            this.PlayDeathSoundEffect();
        }
        public int AttackDamage()
        {
            return Service.Random((int)(0.8 * this.strength), this.strength);
        }
        public virtual void Revive()
        {
            this.IsDead = false;
            this.state = States.stand;
            this.hp = this.maxHP / 2;
            this.mp = this.maxMP / 2;
            this.effects = SpriteEffects.FlipHorizontally;
            base.animate = true;
        }
        public bool HasSkillReachedTarget()
        {
            return (skill != null && !this.IsDead && skill.HasReachedTarget);
        }
        public LivingObject GetSkillTarget()
        {
            return skill.GetTarget();
        }
        public void SkillHasReachedTarget()
        {
            skill.SetTarget(null);
            skill.PlayHit();
        }
        public void LaunchSkill(LivingObject livingObject)
        {
            this.mp -= Skill.REQUIRED_MP;

            Vector2 startingPosition = this.upSensor.position;

            skill.SetTarget(livingObject);
            skill.SetTarget(Vector2.Zero);

            skill.Launch(startingPosition);
        }
        public void LaunchSkill(Vector2 onlineTarget)
        {
            this.mp -= Skill.REQUIRED_MP;

            Vector2 startingPosition = this.upSensor.position;

            skill.SetTarget(onlineTarget);

            skill.Launch(startingPosition);
        }
        public void PlayDeathSoundEffect()
        {
            SoundDictionary.Play(this.deathSoundEffect);
        }
        #endregion
    }
}
