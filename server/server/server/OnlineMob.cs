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
    enum SoundEffects
    {
        ballLaunch, ballHit,
        bananaThrow, bananaHit, DrakeAttacks, FireballHits,
        MonkeyDies, WormDies, TeddyDies, DrakeDies,
        PickUp
    }

    delegate OnlineMob MonsterCreator(OnlinePlatform platform, OnlineMap map);

    class OnlineMob : OnlineLivingObject
    {

        #region DATA

        // Mob relevant data
        public bool isAttacked;
        public int expGiven;

        private int maxMesoDrop;

        public SoundEffects deathSoundEffect { get; private set; }

        // Hit relevant data
        public OnlinePlayer playerToFollow;

        #endregion

        #region Construction
        // CTOR using spawn platform
        public OnlineMob(Folders folder, Vector2 position,
                    int expGiven, int maxMesoDrop,
                    int maxHP, int maxMP, bool canJump, float jumpHeight, Vector2 speed, int strength, OnlineMap map, Skill skill, SoundEffects deathSoundEffect)
            : base(folder, position, 10, maxHP, maxMP, canJump, speed, strength, skill)
        {
            base.keys = new OnlineMobKeys(this, map);

            this.isAttacked = false;
            this.expGiven = expGiven;
            this.maxMesoDrop = maxMesoDrop;
            this.deathSoundEffect = deathSoundEffect;
        }
        #endregion

        public override void Update(OnlineMap map)
        {
            base.Update(map);
            this.mp = maxMP;
        }

        #region Specific Creators
        public static OnlineMob CreateTeddy(OnlinePlatform startingPlatform, OnlineMap map)
        {
            Vector2 randomPosition = startingPlatform.GetRandomStartingPosition();

            return new OnlineMob(Folders.teddy, randomPosition, 15, 100, 15, 100, true, 15, new Vector2(2f,0), 25, map, null, SoundEffects.TeddyDies);
        }
        public static OnlineMob CreateWorm(OnlinePlatform startingPlatform, OnlineMap map)
        {
            Vector2 randomPosition = startingPlatform.GetRandomStartingPosition();

            return new OnlineMob(Folders.worm, randomPosition, 15, 100, 15, 100, false, 0, new Vector2(0.7f, 0), 25, map, null, SoundEffects.WormDies);
        }
        public static OnlineMob CreateMonkey(OnlinePlatform startingPlatform, OnlineMap map)
        {
            Vector2 randomPosition = startingPlatform.GetRandomStartingPosition();

            Skill skill = new Skill(Folders.Banana, SoundEffects.bananaThrow, SoundEffects.bananaHit);

            return new OnlineMob(Folders.monkey, randomPosition, 100, 500, 150, 100, true, 15, new Vector2(3f, 0), 200, map, skill, SoundEffects.MonkeyDies);
        }
        public static OnlineMob CreateDrake(OnlinePlatform startingPlatform, OnlineMap map)
        {
            Vector2 randomPosition = startingPlatform.GetRandomStartingPosition();

            Skill skill = new Skill(Folders.Fireball, SoundEffects.DrakeAttacks, SoundEffects.FireballHits);

            return new OnlineMob(Folders.Drake, randomPosition, 2000, 2000, 1000, 100, false, 0, new Vector2(1.5f, 0), 500, map, skill, SoundEffects.DrakeDies);
        }

        #endregion

        #region public functions
        public override void Hit(OnlinePlayer playerToFollow)
        {
            this.playerToFollow = playerToFollow;
            this.isAttacked = true;

            base.Hit(playerToFollow);
        }
        public int MoneyDropped()
        {
            return new Random().Next((int)(0.8 * this.maxMesoDrop), this.maxMesoDrop);
        }
        #endregion
    }
}
