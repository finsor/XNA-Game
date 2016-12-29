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
using MB = System.Windows.Forms.MessageBox;
using SW = System.Diagnostics.Stopwatch;

namespace OrFins
{
    class SingleplayerManager : GameManager
    {
        public SingleplayerManager(SpriteBatch spriteBatch, Camera camera, Player player, Map base_map, HUD hud)
            : base(spriteBatch, camera, player, base_map, hud)
        {
        }

        public override void Draw(Vector2 windowScale, GraphicsDevice GraphicsDevice)
        {
            if (!base.IsLoading)
            {
                base.Draw_Map_And_Player(windowScale, GameState.SinglePlayer);

                base.DrawHud(windowScale, player.position);
            }
            else
            {
                GraphicsDevice.Clear(Color.Black);
            }
        }

        #region Update functions
        public override void Update(GameTime gameTime, Vector2 windowScale)
        {
            base.Update_Map_And_Player(gameTime, windowScale, GameState.SinglePlayer);

            if (IsLoading)
            {
                base.Update_Load_Time();
            }
            else
            {
                base.PlayerTeleports();
                this.PlayerAttacks();
                this.PlayerAttacked();
                this.ProcessSkills();
                this.PlayerPicksUp();

                base.Update_Hud(windowScale);
            }
        }

        protected override void PlayerAttacks()
        {
            if (player.CanHit)
            {

                // Assuming player faces right
                Rectangle playerAttackRectangle = new Rectangle(
                    (int)(player.position.X),
                    (int)(player.surroundingRectangle.Top),
                    (int)(player.surroundingRectangle.Width * 1.5f),
                    (int)(player.surroundingRectangle.Height));

                // if player faces left
                if (player.IsLookingLeft)
                {
                    playerAttackRectangle.X -= playerAttackRectangle.Width;
                }

                foreach (Mob monster in current_map.monsters)
                {
                    // If player's attack range collides with mob
                    if (!monster.IsDead &&
                        playerAttackRectangle.Intersects(monster.surroundingRectangle))
                    {
                        int knockback_direction = base.Knockback_Direction(player, monster);
                        int damage = player.AttackDamage();

                        monster.Hit(damage, knockback_direction);

                        monster.SetPlayerToFollow(player);

                        player.hasHitDuringCurrentAttack = true;

                        if (monster.hp < 1)
                        {
                            player.GainEXP(monster.expGiven);
                        }

                        break;
                    }
                }
            }
        }
        protected override void PlayerAttacked()
        {
            if (player.CanBeHit)
            {
                foreach (Mob monster in current_map.monsters)
                {
                    if (!monster.IsDead && IsCollision(player, monster))
                    {
                        int knockback_direction = base.Knockback_Direction(monster, player);
                        int damage = monster.AttackDamage();

                        player.Hit(damage, knockback_direction);

                        break;
                    }
                }
            }
        }
        protected override void PlayerPicksUp()
        {
            if (player.AttemptsToPickUp && !base.IsLoading)
            {
                PickupItem[] array = new PickupItem[current_map.pickup.Count];
                current_map.pickup.CopyTo(array);

                foreach (PickupItem item in array)
                {

                    if (player.surroundingRectangle.Intersects(item.surroundingRectangle))
                    {
                        if (item.money > 0)
                        {
                            player.GainMoney(item.money);
                            current_map.RemovePickupItem(item);
                            SoundDictionary.Play(SoundEffects.PickUp);
                        }
                    }
                }
            }
        }
        protected override void ProcessSkills()
        {
            if (player.IsLaunchingSkill)
            {
                player.LaunchSkill(null);
                foreach (Mob mob in current_map.monsters)
                {
                    if (!mob.IsDead && Vector2.Distance(mob.position, player.position) <= Skill.MAX_DISTANCE)
                    {
                        player.LaunchSkill(mob);
                        break;
                    }
                }
            }

            foreach (Mob mob in current_map.monsters)
            {
                if (mob.IsLaunchingSkill)
                {
                    mob.LaunchSkill(mob.playerToFollow);
                }
            }

            if (player.HasSkillReachedTarget())
            {
                LivingObject target = player.GetSkillTarget();

                if (!target.IsDead)
                {
                    player.SkillHasReachedTarget();

                    int knockback_direction = base.Knockback_Direction(player, target);
                    int damage = player.AttackDamage();

                    target.Hit(damage, knockback_direction);

                    Mob monster = (Mob)target;
                    monster.SetPlayerToFollow(player);

                    if (target.hp < 1)
                    {
                        foreach (Mob mob in current_map.monsters)
                        {
                            if (mob == target)
                            {
                                player.GainEXP(mob.expGiven);
                            }
                        }
                    }
                }
            }

            foreach (Mob mob in current_map.monsters)
            {
                if (mob.HasSkillReachedTarget())
                {
                    if (!mob.IsDead && player.CanBeHit)
                    {
                        mob.SkillHasReachedTarget();

                        int knockback_direction = base.Knockback_Direction(mob, player);
                        int damage = mob.AttackDamage();

                        player.Hit(damage, knockback_direction);
                        break;
                    }
                }
            }
        }
        private bool IsCollision(LivingObject obj1, LivingObject obj2)
        {
            return (
                obj1.headCircle.DoesCollide(obj2.headCircle) ||
                obj1.headCircle.DoesCollide(obj2.coreCircle) ||
                obj1.headCircle.DoesCollide(obj2.legsCircle) ||

                obj1.coreCircle.DoesCollide(obj2.headCircle) ||
                obj1.coreCircle.DoesCollide(obj2.coreCircle) ||
                obj1.coreCircle.DoesCollide(obj2.legsCircle) ||

                obj1.legsCircle.DoesCollide(obj2.headCircle) ||
                obj1.legsCircle.DoesCollide(obj2.coreCircle) ||
                obj1.legsCircle.DoesCollide(obj2.legsCircle));

        }
        #endregion
    }
}
