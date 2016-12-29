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
    class Mob : LivingObject
    {
        #region DATA
        public bool isAttacked { get; private set; }
        public int expGiven { get; private set; }
        private Bar hpBar;
        private int maxMesoDrop;
        public Player playerToFollow { get; private set; }
        #endregion

        #region Construction
        public Mob(Folders name, SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceRectangle,
                    Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth,
                    int expGiven, int maxMesoDrop,
                    int maxHP, int maxMP, bool canJump, float jumpHeight, Vector2 speed, int strength, Map map, Skill skill, SoundEffects deathSoundEffect)
            : base(name, spriteBatch, position, sourceRectangle,
                    color, rotation, origin, scale, effects, layerDepth, 10,
                    maxHP, maxMP, canJump, speed, Vector2.Zero, strength, States.mobSkill, skill, deathSoundEffect)
        {
            base.baseKeys = new MobKeys(this, map);

            this.isAttacked = false;
            this.expGiven = expGiven;
            this.maxMesoDrop = maxMesoDrop;

            base.canAttack = (skill != null);

            this.hpBar = new Bar(Folders.hud, States.hpBar, spriteBatch, this.color, this.rotation, new Vector2(0.5f, 0.01f), this.effects, this.layerDepth, new Rectangle());
        }
        #endregion

        #region Drawing functions
        public override void DrawObject(Vector2 windowScale)
        {
            base.DrawObject(windowScale);

            this.hpBar.DrawObject(windowScale);
        }
        #endregion

        #region Update functions
        public override void Update(GameTime gameTime, Map map)
        {
            base.Update(gameTime, map);

            // We would like mobs to be able to use skills any time.
            base.mp = base.maxMP;

            Rectangle hpBar_rec = new Rectangle(
                (int)(base.position.X - 25),
                (int)(base.topPixel),
                (int)(50),
                (int)(0));

            this.hpBar.Update(hpBar_rec, this.hp, this.maxHP);
        }
        #endregion

        #region public functions
        public void SetPlayerToFollow(Player player)
        {
            this.playerToFollow = player;
            this.isAttacked = true;
        }
        public int MoneyDropped()
        {
            return (Service.Random((int)(0.8 * this.maxMesoDrop), this.maxMesoDrop));
        }
        #endregion

        #region Specific Creators
        public static Mob CreateTeddy(SpriteBatch spriteBatch, Platform startingPlatform, Map map)
        {
            Vector2 randomPosition = startingPlatform.GetRandomStartingPosition();

            return new Mob(Folders.teddy, spriteBatch, randomPosition, null, Color.White, 0f, new Vector2(86, 106), new Vector2(1f, 1f), SpriteEffects.None, 0,
                            15, 100,
                            15, 100, true, 15, new Vector2(2f, 0), 25, map, null, SoundEffects.TeddyDies);
        }
        public static Mob CreateWorm(SpriteBatch spriteBatch, Platform startingPlatform, Map map)
        {
            Vector2 randomPosition = startingPlatform.GetRandomStartingPosition();

            return new Mob(Folders.worm, spriteBatch, randomPosition, null, Color.White, 0f, new Vector2(86, 106), Vector2.One, SpriteEffects.None, 0,
                            15, 100,
                            15, 50, false, 0, new Vector2(0.7f, 0), 25, map, null, SoundEffects.WormDies);
        }
        public static Mob CreateMonkey(SpriteBatch spriteBatch, Platform startingPlatform, Map map)
        {
            Vector2 randomPosition = startingPlatform.GetRandomStartingPosition();

            Skill skill = new Skill(Folders.Banana, spriteBatch, SoundEffects.bananaThrow, SoundEffects.bananaHit);

            return new Mob(Folders.monkey, spriteBatch, randomPosition, null, Color.White, 0f, new Vector2(86, 106), Vector2.One, SpriteEffects.None, 0,
                            100, 500,
                            150, 200, true, 15, new Vector2(3f, 0), 200, map, skill, SoundEffects.MonkeyDies);
        }
        public static Mob CreateDrake(SpriteBatch spriteBatch, Platform startingPlatform, Map map)
        {
            Vector2 randomPosition = startingPlatform.GetRandomStartingPosition();

            Skill skill = new Skill(Folders.Fireball, spriteBatch, SoundEffects.DrakeAttacks, SoundEffects.FireballHits);

            return new Mob(Folders.Drake, spriteBatch, randomPosition, null, Color.White, 0f, new Vector2(86, 106), Vector2.One, SpriteEffects.None, 0,
                            2000, 2000,
                            1000, 100, false, 15, new Vector2(1.5f, 0), 500, map, skill, SoundEffects.DrakeDies);
        }
        #endregion
    }
}