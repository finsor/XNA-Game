using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Timers;

namespace OrFins
{
    class Player : LivingObject
    {
        #region DATA

        // Player-relevant data
        public Dictionary<ClothingType, Clothing> equipment;
        public Clothing[] bag;
        private int bagCount;
        private Timer dying_timer;

        //private Timer save_timer;
        public int exp;
        public int money;
        public int requiredEXP;
        public int level;
        public int alert_period = 0;
        public bool hasHitMob = false;

        #endregion

        #region Properties
        private bool IsBagFull
        {
            get
            {
                return (bagCount == bag.Length);
            }
        }
        public bool CanHit
        {
            get
            {
                return (
                canAttack &&
                !IsDead &&
                isAttacking &&
                IsLastAnimationFrame &&
                !hasHitDuringCurrentAttack);
            }
        }
        #endregion

        #region Construction
        public Player(Folders name, SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth,
                      int level, int money, bool canJump, Vector2 speed, Vector2 ropeSpeed, BaseKeys baseKeys, SoundEffects deathSoundEffect)
            : base(name, spriteBatch, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth,
                      10, 0, 0, canJump, speed, ropeSpeed, 0, States.swing1, new Skill(Folders.Ball, spriteBatch, SoundEffects.ballLaunch, SoundEffects.ballHit), deathSoundEffect)
        {
            base.baseKeys = baseKeys;

            this.LevelUp(level);
            this.money = money;

            this.equipment = new Dictionary<ClothingType, Clothing>();
            this.bag = new Clothing[24];
            this.bagCount = 0;

            InitializeDyingTimer();

            AddToBag(ClothingDictionary.dictionary[ClothingType.top][0].GetClothing());
            AddToBag(ClothingDictionary.dictionary[ClothingType.weapon][0].GetClothing());
        }

        private void InitializeDyingTimer()
        {
            SoundEffect effect = SoundDictionary.Get(SoundEffects.HeartBeat);
            dying_timer = new Timer(effect.Duration.Minutes * 60000 + effect.Duration.Seconds * 1000 + effect.Duration.Milliseconds);
            dying_timer.Enabled = true;
            dying_timer.Elapsed += OnTimedEvent;
            dying_timer.Start();
        }
        #endregion

        #region Draw functions
        public override void DrawObject(Vector2 windowScale)
        {
            if (isHit && hit_period % 10 == 0)
                base.isDrawable = !base.isDrawable;
            else
                base.isDrawable = true;


            base.DrawObject(windowScale);

            if (!IsDead)
            {
                foreach (ClothingType clothingType in Enum.GetValues(typeof(ClothingType)))
                {
                    if (equipment.ContainsKey(clothingType))
                        equipment[clothingType].DrawObject(windowScale);
                }
            }
        }
        #endregion

        #region Update functions
        public override void Update(GameTime gameTime, Map map)
        {
            if (IsDead && HasFinishedAnimation)
            {
                animate = false;
                return;
            }

            // Hide hair if player wears a hat
            if (equipment.ContainsKey(ClothingType.head))
            {
                base.folder = Folders.player_bald;
            }
            else
            {
                base.folder = Folders.player;
            }

            // Can only attack when has a weapon
            canAttack = equipment.ContainsKey(ClothingType.weapon);

            base.Update(gameTime, map);

            ProcessAlert();
            Rest(gameTime, 1500, 2);

            // Update clothing if not dead
            if (!IsDead)
            {
                foreach (Clothing clothing in equipment.Values)
                    clothing.Update(this);
            }
        }
        #endregion

        #region Private functions
        private void ProcessAlert()
        {
            if (isAttacking && alert_period == 0)
            {
                isAlert = true;
                alert_period = 300;
            }
            if (alert_period > 0)
            {
                alert_period--;
            }
            else
            {
                isAlert = false;
            }
        }
        private void Rest(GameTime gameTime, int miliseconds, int addition)
        {
            if (!(base.state == States.stand || base.state == States.onRope))
                return;

            if (gameTime.TotalGameTime.TotalMilliseconds % miliseconds < 16)
            {
                this.hp = (int)MathHelper.Clamp(hp + addition, 0, maxHP);
                this.mp = (int)MathHelper.Clamp(mp + addition, 0, maxMP);
            }
        }
        private void AddToBag(Clothing clothing)
        {
            for (int i = 0; i < bag.Length; i++)
            {
                if (bag[i] == null)
                {
                    bag[i] = clothing;
                    bag[i].isDrawable = true;
                    bagCount++;
                    return;
                }
            }
        }
        private void RemoveFromBag(int bagIndex)
        {
            if (bag[bagIndex] == null)
                return;

            bag[bagIndex].isDrawable = false;
            bag[bagIndex] = null;
            bagCount--;
        }
        private void Switch_Bag_Equipment(Clothing clothing, int bagIndex)
        {
            Clothing temp = equipment[clothing.clothingType];

            equipment[clothing.clothingType] = clothing;
            RemoveFromBag(bagIndex);
            AddToBag(temp);

            this.UpdateStrength(-temp.strength + clothing.strength);
            this.UpdateDefence(-temp.defence + clothing.defence);

            equipment[clothing.clothingType].Update(this);
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (!this.IsDead && this.hp <= this.maxHP / 10)
            {
                SoundDictionary.Play(SoundEffects.HeartBeat);
            }
        }
        #endregion

        #region Public functions
        public void LevelUp(int levels)
        {
            exp -= requiredEXP;
            if (exp < 0)
                exp = 0;

            level += levels;

            this.requiredEXP = (int)(100 * Math.Pow(1.5, this.level - 1));

            this.maxHP = 100 * this.level;
            this.maxMP = 100 * this.level;

            this.hp = maxHP;
            this.mp = maxMP;
        }
        public void GainEXP(int expGain)
        {
            exp += expGain;

            if (exp >= requiredEXP)
                this.LevelUp(1);
        }
        public void Wear(Clothing clothing, int bagIndex)
        {
            if (clothing == null || clothing.minLevel > this.level)
                return;

            if (equipment.ContainsKey(clothing.clothingType))
            {
                this.Switch_Bag_Equipment(clothing, bagIndex);
            }
            else
            {
                this.equipment.Add(clothing.clothingType, clothing);
                this.RemoveFromBag(bagIndex);

                base.UpdateStrength(clothing.strength);
                this.UpdateDefence(clothing.defence);
            }
        }
        public void Unwear(Clothing clothing)
        {
            if (clothing == null || IsBagFull)
                return;

            equipment.Remove(clothing.clothingType);
            AddToBag(clothing);

            UpdateStrength(-clothing.strength);
            UpdateDefence(-clothing.defence);
        }
        public void GainMoney(int money)
        {
            this.money += money;
        }
        public override void Revive()
        {
            exp = (int)(exp * 0.9f);
            base.Revive();
        }
        public void BuyFromShop(Clothing clothing, int buying_price)
        {
            // If bag isn't full and player has enough money
            if (!IsBagFull && buying_price <= money)
            {
                AddToBag(clothing);
                money -= buying_price;
            }
        }
        public void SellBack(Clothing clothing, int sell_back_price)
        {
            for (int i = 0; i < bag.Length; i++)
            {
                if (bag[i] != null && bag[i].folder == clothing.folder)
                {
                    this.RemoveFromBag(i);
                    break;
                }
            }

            this.money += sell_back_price;
        }
        public void SaveToFile()
        {
            AES_Encryption AES = new AES_Encryption();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"savefile.txt"))
            {
                AES.Write(file, level);
                AES.Write(file, hp);
                AES.Write(file, mp);
                AES.Write(file, requiredEXP);
                AES.Write(file, exp);
                AES.Write(file, money);
                AES.Write(file, bagCount);

                foreach (Clothing clothing in bag)
                {
                    if (clothing != null)
                    {
                        AES.Write(file, clothing.folder);
                        AES.Write(file, clothing.clothingType);
                    }
                }

                AES.Write(file, equipment.Count);

                foreach (Clothing clothing in equipment.Values)
                {
                    AES.Write(file, clothing.folder);
                    AES.Write(file, clothing.clothingType);
                }
            }
        }
        public void LoadFromFile()
        {
            AES_Encryption AES = new AES_Encryption();
            using (System.IO.StreamReader file = new System.IO.StreamReader(@"savefile.txt"))
            {
                Folders folder;
                ClothingType clothingType;
                Clothing clothing;

                this.level = int.Parse(AES.Read(file));
                this.maxHP = level * 100;
                this.maxMP = maxHP;
                this.hp = int.Parse(AES.Read(file));
                this.mp = int.Parse(AES.Read(file));
                this.requiredEXP = int.Parse(AES.Read(file));
                this.exp = int.Parse(AES.Read(file));
                this.money = int.Parse(AES.Read(file));

                this.bag = new Clothing[24];
                this.bagCount = int.Parse(AES.Read(file));

                for (int i = 0; i < bagCount; i++)
                {
                    folder = (Folders)Enum.Parse(typeof(Folders), AES.Read(file));
                    clothingType = (ClothingType)Enum.Parse(typeof(ClothingType), AES.Read(file));

                    bag[i] = ClothingDictionary.GetClothing(clothingType, folder);
                }

                int equipmentCount = int.Parse(AES.Read(file));
                this.equipment = new Dictionary<ClothingType, Clothing>();

                for (int i = 0; i < equipmentCount; i++)
                {
                    folder = (Folders)Enum.Parse(typeof(Folders), AES.Read(file));
                    clothingType = (ClothingType)Enum.Parse(typeof(ClothingType), AES.Read(file));

                    clothing = ClothingDictionary.GetClothing(clothingType, folder);
                    equipment.Add(clothingType, clothing);

                    this.UpdateStrength(clothing.strength);
                    this.UpdateDefence(clothing.defence);
                }
            }
        }
        public static BaseKeys DefaultUserKeys()
        {
            return new UserKeys(Keys.Up, Keys.Down, Keys.Right, Keys.Left, Keys.LeftAlt, Keys.LeftControl, Keys.S, Keys.LeftShift, Keys.Z); // base keys
        }
        public static Player NewPlayer(SpriteBatch spriteBatch)
        {
            return new Player(
                Folders.player,                         // folder
                spriteBatch,                            // spritebatch
                Vector2.Zero,                           // starting platform
                null,                                   // sourceRectangle
                Color.White,                            // color
                0f,                                     // rotation
                Vector2.Zero,                           // origin
                Vector2.One,                            // scale
                SpriteEffects.FlipHorizontally,         // effect
                0,                                      // layerDepth
                1,                                      // level
                17,                                      // money
                true,                                   // canJump
                new Vector2(3f, 0),                     // walk speed
                new Vector2(0, -3f),                    // rope speed
                Player.DefaultUserKeys(),               // base keys
                SoundEffects.Death);
        }
        #endregion

        #region Online functions
        public void SendProfile(OnlineClient onlineClient)
        {
            // Send details of body
            onlineClient.Send(
                base.folder.ToString(),
                base.state.ToString(),
                base.index.ToString(),
                base.position.X.ToString(),
                base.position.Y.ToString(),
                base.effects.ToString(),
                base.isHit.ToString(),
                base.strength.ToString());

            foreach (ClothingType type in Enum.GetValues(typeof(ClothingType)))
            {
                if (equipment.ContainsKey(type))
                {
                    onlineClient.Send("SENDING",
                        equipment[type].folder.ToString());
                }
            }
            onlineClient.Send("DONE");

            // Send details of circles
            base.headCircle.SendProfile(onlineClient);
            base.coreCircle.SendProfile(onlineClient);
            base.legsCircle.SendProfile(onlineClient);

            skill.SendDrawingData(onlineClient);
        }
        public void SendSurroundingRectangle(OnlineClient onlineClient)
        {
            onlineClient.Send(
                surroundingRectangle.X.ToString(),
                surroundingRectangle.Y.ToString(),
                surroundingRectangle.Width.ToString(),
                surroundingRectangle.Height.ToString());
        }
        public void ResetSkillOnlineTarget(Vector2 onlineTarget)
        {
            skill.SetTarget(onlineTarget);
        }
        #endregion
    }
}