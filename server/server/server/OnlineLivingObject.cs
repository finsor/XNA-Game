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
    struct Sensor
    {
        public Vector2 position { get; set; }
        public bool IsOn { get; set; }
        public void Draw(Texture2D pixel, SpriteBatch spriteBatch)
        {
            Rectangle rectangle = new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4);

            spriteBatch.Draw(pixel, rectangle, Color.Red);
        }
    }

    class OnlineLivingObject : OnlineAnimation
    {
        #region DATA

        // Living-relevant data
        public int maxHP { get; set; }
        public int hp { get; set; }

        public int maxMP { get; set; }
        public int mp { get; set; }

        public int strength { get; set; }

        public Sensor leftSensor;
        public Sensor rightSensor;
        public Sensor upSensor;
        public Sensor downSensor;
        public Sensor downLeftSensor;
        public Sensor downRightSensor;

        // Movement-relevant data
        public OnlineMobKeys keys;

        public OnlinePlatform basePlat;
        public Vector2 speed { get; set; }

        public bool canJump { get; set; }
        public bool isJumping;
        public float fallSpeed;

        public bool isAttacking;
        public bool isHit;
        public Vector2 knockback;

        public const float GRAVITY = 0.4f;
        public const float MAX_FALL_SPEED = 8f;
        public const float KNOCKBACK_MAX_DISTANCE = 8f;

        protected int hit_period;
        private States skillState;
        public Skill skill { get; private set; }
        private bool canAttack;

        private bool hasHitDuringCurrentAttack;
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
                return (canAttack && keys.SkillKey() && mp >= Skill.REQUIRED_MP && !IsDead && !skill.IsLaunched);
            }
        }
        #endregion

        #region CTOR
        public OnlineLivingObject(Folders folder, Vector2 position, int slowRate, int maxHP, int maxMP, bool canJump, Vector2 speed, int strength, Skill skill)
            : base(folder, position, slowRate)
        {
            this.hp = this.maxHP = maxHP;
            this.mp = this.maxMP = maxMP;
            this.canJump = canJump;
            this.speed = speed;
            this.strength = strength;
            this.fallSpeed = 0;
            this.skill = skill;
            if (skill != null)
            {
                this.skillState = States.mobSkill;
                this.canAttack = true;
            }
        }
        #endregion

        #region Update functions
        public virtual void Update(OnlineMap map)
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

            this.keys.Update();
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

            this.position = new Vector2(MathHelper.Clamp(this.position.X, map.leftLimit + (position.X - leftSensor.position.X) + 3, map.rightLimit - (rightSensor.position.X - position.X) - 3), this.position.Y);
            //this.position = new Vector2(MathHelper.Clamp(this.position.X, map.leftLimit, map.rightLimit), this.position.Y);
            this.Update_State();

            if (skill != null && (skill.IsLaunched || skill.IsHitAnimation))
            {
                skill.Update(base.IsLookingLeft);
            }

            base.Update();
        }
        #endregion

        #region private functions
        private void UpdateBasePlat(OnlineMap map)
        {
            if (!downSensor.IsOn)
                return;

            this.basePlat = map.mapMask.At(downSensor.position).platform;
        }
        private void UpdateSensors(OnlineMap map)
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
        private void ProcessActions(OnlineMap map)
        {
            if ((canAttack && keys.SkillKey()) || isAttacking)
            {
                if (!isAttacking)
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

                if (base.HasFinishedAnimation)
                {
                    isAttacking = false;
                }
                return;
            }


            // Jump
            if (keys.JumpKey() && !isJumping && downSensor.IsOn)
            {
                fallSpeed = -MAX_FALL_SPEED;
                isJumping = true;
            }

            // Move left and right
            if (keys.LeftKey() && (!leftSensor.IsOn || (leftSensor.IsOn && rightSensor.IsOn)))
            {
                position -= speed;
            }

            if (keys.RightKey() && (!rightSensor.IsOn || (leftSensor.IsOn && rightSensor.IsOn)))
            {
                position += speed;
            }

        }
        private void Update_State()
        {
            if (IsDead)
            {
                return;
            }

            if (isAttacking)
            {
                return;
            }
            else
            {
                base.slowRate = 10;
            }

            if (keys.LeftKey())
            {
                base.effects = SpriteEffects.None;
            }
            if (keys.RightKey())
            {
                base.effects = SpriteEffects.FlipHorizontally;
            }

            if (isJumping)
            {
                base.state = States.jump;
                return;
            }

            if (keys.LeftKey() != keys.RightKey())
            {
                base.state = States.walk;
                return;
            }

            base.state = States.stand;

            if (isHit)
            {
                base.state = States.alert;
            }
        }
        private void StickToTheGround(OnlineMap map)
        {
            float height = 0;

            while (!map.IsLand(this.position + Vector2.UnitY * height))
            {
                height++;
            }
        }
        #endregion

        #region protected functions
        protected void ProcessGravity(OnlineMap map)
        {
            // When do I fall?
            if (fallSpeed < 0 ||                                              // when rising up
                (fallSpeed > 0 && !(downSensor.IsOn && !upSensor.IsOn))       // or falling down while not hitting ground)
                )
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
        #endregion

        #region public functions
        public virtual void Hit(OnlinePlayer attacker)
        {
            this.knockback = Vector2.UnitX * KNOCKBACK_MAX_DISTANCE;

            if (attacker.position.X > this.position.X)
                this.knockback *= -1;

            int damage = attacker.AttackDamage();
            ReduceHP(damage);
        }
        public void ReduceHP(int damage)
        {
            this.hp = (int)MathHelper.Clamp(this.hp - damage, 0, this.maxHP);
            this.isHit = true;
            this.hit_period = 50;

            if (this.hp < 1)
            {
                IsDead = true;
            }
        }
        public int AttackDamage()
        {
            return new Random().Next((int)(0.8 * this.strength), this.strength);
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
        public void SetBasePlat(OnlineMap map)
        {
            foreach (OnlinePlatform platform in map.platforms)
            {
                if (this.position.Y <= platform.topPixel &&
                    this.position.X >= platform.leftmostPixel &&
                    this.position.X <= platform.rightmostPixel)
                {
                    this.basePlat = platform;
                }
            }
        }
        public void SetPosition(Vector2 position)
        {
            this.position = position;
        }
        public bool HasSkillReachedTarget()
        {
            return (skill != null && !this.IsDead && skill.HasReachedTarget);
        }
        public bool HasSkillHit()
        {
            return (skill != null && skill.IsHitAnimation);
        }
        public OnlinePlayer GetSkillTarget()
        {
            return skill.GetTarget();
        }
        public void SkillHasReachedTarget()
        {
            skill.SetTarget(null);
        }
        public void LaunchSkill(OnlinePlayer player)
        {
            this.mp -= Skill.REQUIRED_MP;

            Vector2 startingPosition = this.upSensor.position;

            skill.SetTarget(player);

            skill.Launch(startingPosition);
        }
        public bool IsSkillLaunched()
        {
            return (skill != null && skill.IsLaunched);
        }
        #endregion
    }
}
