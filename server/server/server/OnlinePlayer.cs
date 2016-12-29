using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace server
{
    class OnlinePlayer : OnlineClient
    {
        public const string skill_folder = "Ball";

        public string name { get; private set; }

        public Vector2 position { get; private set; }
        public OnlinePlatform basePlat { get; private set; }
        public string folder { get; private set; }
        public States state { get; private set; }
        public int index { get; private set; }
        public SpriteEffects spriteEffects { get; private set; }
        public int strength { get; private set; }

        private OnlineCircle headCircle;
        private OnlineCircle coreCircle;
        private OnlineCircle legsCircle;

        public string mapName { get; private set; }
        public OnlineMap map;
        public OnlineMob skill_target;

        public bool isAttacking { get; set; }
        public Rectangle attackRectangle { get; set; }
        public Rectangle surroundingRectangle { get; set; }
        public bool hasHit { get; set; }
        public bool isAttemptingToPickUp { get; set; }
        public bool isLaunchingSkill { get; private set; }
        public bool isSkillAlreadyLaunched { get; private set; }
        public bool hasSkillReachedTarget { get; private set; }

        private int damage;
        private int knockback_mul;
        private int expGain;
        private int moneyGain;

        public string skill_state { get; private set; }
        public string skill_index { get; private set; }
        public string skill_position_x { get; private set; }
        public string skill_position_y { get; private set; }

        public bool isHit;
        public bool isDead
        {
            get
            {
                return (state == States.die);
            }
        }

        private Dictionary<string, OnlineMap> world;

        public List<string> clothing { get; private set; }
        public List<string> sound { get; private set; }

        #region Construction
        public OnlinePlayer(TcpClient tcp_client)
            : base(tcp_client)
        {
            this.name = base.GetString();

            this.clothing = new List<string>();
            this.sound = new List<string>();

            this.headCircle = new OnlineCircle();
            this.coreCircle = new OnlineCircle();
            this.legsCircle = new OnlineCircle();

            skill_state = "launched";
        }
        #endregion

        #region Run functions
        public void Sync(Dictionary<string, OnlineMap> world, OnlineMap firstMap)
        {
            base.Send("SYNC");

            this.map = firstMap;
            this.mapName = firstMap.name;

            this.world = world;

            new Thread(THIS_THREAD).Start();
        }

        private void THIS_THREAD()
        {
            this.mapName = "City Center";
            damage = 0;
            knockback_mul = 0;
            expGain = 0;

            while (true)
            {
                try
                {
                    Update();
                    Draw();

                    Thread.Sleep(1000 / 120);
                }
                catch (Exception ex)
                {
                    base.isConnected = false;
                    Console.WriteLine(ex);
                    break;
                }
            }
        }
        #endregion

        #region Update functions
        private void Update()
        {
            this.mapName = base.GetString();
            this.map = world[mapName];

            this.RebuildProfile();
            this.UpdateBasePlat();

            this.PlayerAttacked();
            this.PlayerAttacks();
            this.PlayerPicksUp();
            this.ProcessSkills();
            this.SendSound();
        }
        private void RebuildProfile()
        {
            this.folder = base.GetString();
            this.state = (States)Enum.Parse(typeof(States), base.GetString());
            this.index = base.GetInt();
            this.position = base.GetVector2();
            this.spriteEffects = (SpriteEffects)Enum.Parse(typeof(SpriteEffects), base.GetString());
            this.isHit = base.GetBool();
            this.strength = base.GetInt();

            clothing.Clear();

            while (base.GetString().Equals("SENDING"))
            {
                clothing.Add(base.GetString());
            }

            this.headCircle.Update(base.GetVector2(), base.GetFloat());
            this.coreCircle.Update(base.GetVector2(), base.GetFloat());
            this.legsCircle.Update(base.GetVector2(), base.GetFloat());

            skill_state = base.GetString();
            skill_index = base.GetString();
            skill_position_x = base.GetString();
            skill_position_y = base.GetString();
        }
        private void UpdateBasePlat()
        {
            if (map.mapMask.At(this.position).platform != null)
                this.basePlat = map.mapMask.At(this.position).platform;
        }
        private void PlayerAttacked()
        {
            base.Send(damage.ToString(), knockback_mul.ToString());
            damage = knockback_mul = 0;
        }
        private void PlayerAttacks()
        {
            isAttacking = base.GetBool();

            if (isAttacking)
            {
                this.attackRectangle = base.GetRectangle();
            }
            if (!isAttacking)
            {
                hasHit = false;
            }

            if (expGain != 0)
            {
                base.Send(expGain.ToString());
                expGain = 0;
            }
            else
            {
                base.Send("0");
            }
        }
        private void PlayerPicksUp()
        {
            isAttemptingToPickUp = base.GetBool();

            if (isAttemptingToPickUp)
            {
                this.surroundingRectangle = base.GetRectangle();
            }

            if (moneyGain != 0)
            {
                base.Send(moneyGain.ToString());
                moneyGain = 0;
            }
            else
            {
                Send("0");
            }
        }
        private void ProcessSkills()
        {
            isLaunchingSkill = base.GetBool();
            if (isLaunchingSkill)
            {
                if (skill_target != null)
                {
                    base.Send(skill_target.position.X.ToString(),
                                skill_target.position.Y.ToString());
                }
                else
                {
                    base.Send(Vector2.Zero.X.ToString(), Vector2.Zero.Y.ToString());
                }
            }

            isSkillAlreadyLaunched = base.GetBool();
            if (isSkillAlreadyLaunched)
            {
                if (skill_target != null)
                {
                    base.Send(skill_target.position.X.ToString(),
                                skill_target.position.Y.ToString());
                }
                else
                {
                    base.Send(Vector2.Zero.X.ToString(), Vector2.Zero.Y.ToString());
                }

                hasSkillReachedTarget = base.GetBool();
            }
        }
        private void SendSound()
        {
            foreach (string str in sound)
            {
                base.Send("SENDING", str);
            }

            sound.Clear();
            base.Send("DONE");
        }
        #endregion

        #region Draw functions
        private void Draw()
        {
            DrawPlayers();
            DrawMonsters();
            DrawPickupItems();
            base.Send("DONE");

            DrawBars();
            base.Send("DONE");

            DrawNames();
            base.Send("DONE");
        }
        private void DrawPlayers()
        {
            List<OnlinePlayer> players = map.players.FindAll(item => item != null && item.isConnected && item != this);

            foreach (OnlinePlayer player in players)
            {
                base.Send(
                    "SENDING",
                    player.folder,
                    player.state.ToString(),
                    player.index.ToString(),
                    player.position.X.ToString(),
                    player.position.Y.ToString(),
                    player.spriteEffects.ToString());

                if (!player.isDead)
                {
                    try
                    {
                        foreach (string clothingFolder in player.clothing)
                        {
                            base.Send(
                                "SENDING",
                                clothingFolder,
                                player.state.ToString(),
                                player.index.ToString(),
                                player.position.X.ToString(),
                                player.position.Y.ToString(),
                                player.spriteEffects.ToString());
                        }
                    }
                    catch { }

                    // The monster's skill
                    if (player.isSkillAlreadyLaunched || player.hasSkillReachedTarget)
                    {
                        base.Send(
                            "SENDING",
                            skill_folder,
                            player.skill_state,
                            player.skill_index,
                            player.skill_position_x,
                            player.skill_position_y,
                            SpriteEffects.None.ToString());
                    }
                }
            }
        }
        private void DrawMonsters()
        {
            List<OnlineMob> mobs = map.monsters.GetRange(0, map.monsters.Count);

            foreach (OnlineMob mob in mobs)
            {
                // The monster itself
                base.Send(
                    "SENDING",
                    mob.folder.ToString(),
                    mob.state.ToString(),
                    mob.index.ToString(),
                    mob.position.X.ToString(),
                    mob.position.Y.ToString(),
                    mob.effects.ToString());

                // The monster's skill
                if (mob.IsSkillLaunched() || mob.HasSkillHit())
                {
                    base.Send(
                        "SENDING",
                        mob.skill.folder.ToString(),
                        mob.skill.state.ToString(),
                        mob.skill.index.ToString(),
                        mob.skill.position.X.ToString(),
                        mob.skill.position.Y.ToString(),
                        mob.skill.effects.ToString());
                }
            }
        }
        private void DrawPickupItems()
        {
            List<OnlinePickupItem> items = map.pickup.GetRange(0, map.pickup.Count);

            foreach (OnlinePickupItem item in items)
            {
                base.Send(
                    "SENDING",
                    item.folder.ToString(),
                    item.state.ToString(),
                    "0",
                    item.position.X.ToString(),
                    item.position.Y.ToString(),
                    item.effects.ToString());
            }
        }
        private void DrawBars()
        {
            // Bars representing mobs' hp
            List<OnlineMob> mobs = map.monsters.GetRange(0, map.monsters.Count);

            foreach (OnlineMob mob in mobs)
            {
                base.Send(
                    "SENDING",
                    "hpBar");

                base.Send(
                    ((int)(mob.position.X - 25)).ToString(),
                    ((int)(mob.topPixel - 10)).ToString(),
                    ((int)(mob.hp * (50) / mob.maxHP)).ToString(),
                    "5");
            }

            // Bars representing pickup items' lifetime
            List<OnlinePickupItem> items = map.pickup.GetRange(0, map.pickup.Count);

            foreach (OnlinePickupItem item in items)
            {
                base.Send(
                    "SENDING",
                    "xpBar");

                base.Send(
                    ((int)(item.position.X - 25)).ToString(),
                    ((int)(item.topPixel - 10)).ToString(),
                    ((int)(item.life_timer * 50 / item.maxTimeToLive)).ToString(),
                    "5");
            }
        }
        private void DrawNames()
        {
            List<OnlinePlayer> players = map.players.FindAll(item => item != null && item.isConnected);

            foreach (OnlinePlayer player in players)
            {
                base.Send(
                    "SENDING",
                    player.name,
                    player.position.X.ToString(),
                    (player.position.Y + 3).ToString());
            }
        }
        #endregion

        #region Public functions
        public bool DoesCollide(OnlineAnimation obj)
        {
            return (
                this.headCircle.DoesCollide(obj.headCircle) ||
                this.headCircle.DoesCollide(obj.coreCircle) ||
                this.headCircle.DoesCollide(obj.legsCircle) ||

                this.coreCircle.DoesCollide(obj.headCircle) ||
                this.coreCircle.DoesCollide(obj.coreCircle) ||
                this.coreCircle.DoesCollide(obj.legsCircle) ||

                this.legsCircle.DoesCollide(obj.headCircle) ||
                this.legsCircle.DoesCollide(obj.coreCircle) ||
                this.legsCircle.DoesCollide(obj.legsCircle));
        }
        public int AttackDamage()
        {
            return (new Random().Next((int)(0.8 * this.strength), this.strength));
        }
        public void Hit(int damage, int knockback_mul)
        {
            this.damage += damage;
            this.knockback_mul = (int)MathHelper.Clamp(this.knockback_mul + knockback_mul, -1, 1);
        }
        public void AttackSuccessful()
        {
            this.hasHit = true;
        }
        public void GainExp(int expGain)
        {
            this.expGain += expGain;
        }
        public void GainMoney(int moneyGain)
        {
            this.moneyGain += moneyGain;
        }
        #endregion
    }
}
