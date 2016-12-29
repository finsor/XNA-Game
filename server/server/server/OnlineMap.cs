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
    struct MapRecord
    {
        public bool isLand { get; set; }
        public OnlinePlatform platform { get; set; }
    }

    class OnlineMap
    {
        #region Data
        public List<OnlinePlatform> platforms { get; private set; }
        public List<OnlineMob> monsters { get; private set; }
        public List<OnlinePlayer> players { get; private set; }
        public List<OnlinePickupItem> pickup { get; private set; }

        public MapRecord[,] mapMask { get; private set; }
        public float leftLimit { get; private set; }
        public float rightLimit { get; private set; }
        public float bottomLimit { get; private set; }
        public float topLimit { get; private set; }

        public string name { get; private set; }

        private MonsterCreator[] monsterCreators;
        private int maxAmountOfMonsters;

        private Random random;
        #endregion

        #region Construction
        public OnlineMap(string name, List<OnlinePlatform> platforms, int maxAmountOfMonsters, MonsterCreator[] monsterCreators)
        {
            this.name = name;
            this.platforms = platforms;
            this.maxAmountOfMonsters = maxAmountOfMonsters;
            this.monsterCreators = monsterCreators;
            this.monsters = new List<OnlineMob>();
            this.pickup = new List<OnlinePickupItem>();
            this.players = new List<OnlinePlayer>();
            this.random = new Random();

            this.topLimit = 0;
            this.leftLimit = 0;
            this.rightLimit = platforms[0].surroundingRectangle.Right;
            this.bottomLimit = platforms[0].surroundingRectangle.Bottom;

            CreateMapMask();
        }

        private void CreateMapMask()
        {
            mapMask = new MapRecord[(int)rightLimit, (int)bottomLimit];

            // For each platform, store its colors in mapMask as well as its platform
            foreach (OnlinePlatform platform in platforms)
            {
                Color[] platformColors = new Color[platform.surroundingRectangle.Width * platform.surroundingRectangle.Height];
                platform.texture.GetData<Color>(platformColors);

                for (int y = 0; y < platform.texture.Height; y++)
                {
                    for (int x = 0; x < platform.texture.Width; x++)
                    {
                        // Store platform
                        mapMask[platform.surroundingRectangle.X + x, platform.surroundingRectangle.Y + y].platform = platform;

                        mapMask[platform.surroundingRectangle.X + x, platform.surroundingRectangle.Y + y].isLand =
                            platformColors[y * platform.texture.Width + x] != Color.Transparent;
                    }
                }
            }
        }
        #endregion

        #region Update functions
        public void Update()
        {
            if (players.Count == 0)
                return;

            SpawnUpdateRemoveMonsters();

            MonstersAttackPlayers();

            PlayersAttackMonsters();

            UpdatePickup();

            ProcessSkills();
        }
        private void SpawnUpdateRemoveMonsters()
        {
            SpawnMonsters();

            foreach (OnlineMob monster in monsters)
                monster.Update(this);

            RemoveDeadMobs();
        }
        private void SpawnMonsters()
        {
            if (random.Next(0, 100) == 0 && monsters.Count < maxAmountOfMonsters)
            {
                MonsterCreator monster_creator = monsterCreators[random.Next(0, monsterCreators.Length)];
                OnlineMob monster = monster_creator(platforms[random.Next(0, platforms.Count - 1)], this);
                monsters.Add(monster);
            }
        }
        private void RemoveDeadMobs()
        {
            OnlineMob monster;
            for (int i = 0; i < monsters.Count; i++)
            {
                monster = monsters[i];
                if (monster.IsDead && monster.IsLastAnimationFrame)
                {
                    RemoveMob(monster);
                }
            }
        }
        private void RemoveMob(OnlineMob mob)
        {
            pickup.Add(new OnlinePickupItem(Folders.pickup, States.money, mob.position, 1800, mob.MoneyDropped()));
            monsters.Remove(mob);
        }

        private void MonstersAttackPlayers()
        {
            int knockback_mul = 0;

            List<OnlinePlayer> connectedPlayers = players.FindAll(item => item.isConnected);

            foreach (OnlinePlayer player in connectedPlayers)
            {
                if (!player.isDead && !player.isHit)
                {
                    foreach (OnlineMob mob in monsters)
                    {
                        if (player.DoesCollide(mob))
                        {

                            if (mob.position.X > player.position.X)
                                knockback_mul = -1;
                            else
                                knockback_mul = 1;

                            player.Hit(mob.AttackDamage(), knockback_mul);

                            break;
                        }
                    }
                }
            }
        }
        private void PlayersAttackMonsters()
        {
            List<OnlinePlayer> connectedPlayers = players.FindAll(item => item.isConnected);

            float total_exp = 0;

            foreach (OnlinePlayer player in connectedPlayers)
            {
                if (player.isAttacking && !player.hasHit)
                {
                    foreach (OnlineMob mob in monsters)
                    {
                        if (!mob.IsDead && !mob.isHit &&
                            player.attackRectangle.Intersects(mob.surroundingRectangle))
                        {
                            mob.Hit(player);

                            player.AttackSuccessful();

                            connectedPlayers.ForEach(item => item.sound.Add("hit" + random.Next(1, 4)));

                            if (mob.hp < 1)
                            {
                                total_exp += mob.expGiven * 1.2f;
                                connectedPlayers.ForEach(item => item.sound.Add(mob.deathSoundEffect.ToString()));
                            }

                            break;
                        }
                    }
                }
            }

            total_exp /= players.Count;

            foreach (OnlinePlayer player in connectedPlayers)
            {
                player.GainExp((int)total_exp);
            }
        }
        private void UpdatePickup()
        {
            foreach (OnlinePickupItem item in pickup)
                item.Update();

            pickup.RemoveAll(item => item.LifeTimerEnded);

            int total_pickup_for_player;
            List<OnlinePlayer> connectedPlayers = players.FindAll(item => item.isConnected);

            foreach (OnlinePlayer player in connectedPlayers)
            {
                total_pickup_for_player = 0;

                if (player.isAttemptingToPickUp)
                {

                    for (int k = 0; k < pickup.Count; k++)
                    {
                        if (player.surroundingRectangle.Intersects(pickup[k].surroundingRectangle))
                        {
                            total_pickup_for_player += pickup[k].money;
                            connectedPlayers.ForEach(item => item.sound.Add(SoundEffects.PickUp.ToString()));
                            pickup.RemoveAt(k);
                        }
                    }
                }

                player.GainMoney(total_pickup_for_player);
            }
        }
        private void ProcessSkills()
        {
            float total_exp;
            Vector2 target;
            OnlineMob mob_target;

            total_exp = 0;

            // set targets
            List<OnlinePlayer> connectedPlayers = players.FindAll(item => item.isConnected);
            foreach (OnlinePlayer player in connectedPlayers)
            {
                // If just launching skill
                if (player.isLaunchingSkill)
                {
                    player.skill_target = null;
                    target = Vector2.Zero;
                    foreach (OnlineMob mob in monsters)
                    {
                        if (!mob.IsDead && Vector2.Distance(mob.position, player.position) <= Skill.MAX_DISTANCE)
                        {
                            connectedPlayers.ForEach(item => item.sound.Add(SoundEffects.ballLaunch.ToString()));
                            player.skill_target = mob;
                            target = mob.position;
                            break;
                        }
                    }
                }


                // If skill is already launched

                if (player.isSkillAlreadyLaunched)
                {
                    if (player.skill_target != null)
                    {
                        target = player.skill_target.position;
                    }
                    else
                    {
                        target = Vector2.Zero;
                    }

                    // If skill has reached its target
                    if (player.hasSkillReachedTarget)
                    {
                        mob_target = player.skill_target;

                        if (!mob_target.IsDead)
                        {
                            mob_target.Hit(player);
                            connectedPlayers.ForEach(item => item.sound.Add(SoundEffects.ballHit.ToString()));
                            //has_hit = true;

                            if (mob_target.hp < 1)
                            {
                                total_exp += mob_target.expGiven * 1.2f;
                                connectedPlayers.ForEach(item => item.sound.Add(mob_target.deathSoundEffect.ToString()));
                            }
                        }
                    }

                }
            }

            total_exp /= connectedPlayers.Count;

            foreach (OnlinePlayer player in connectedPlayers)
            {
                player.GainExp((int)total_exp);
            }

            // Mobs launch skills
            foreach (OnlineMob mob in monsters)
            {
                if (mob.IsLaunchingSkill)
                {
                    mob.LaunchSkill(mob.playerToFollow);
                    connectedPlayers.ForEach(item => item.sound.Add(mob.skill.launch_SoundEffect.ToString()));

                }
            }

            // Mobs' skills hit players
            int knck, dmg;
            foreach (OnlinePlayer player in connectedPlayers)
            {
                knck = 0;
                dmg = 0;
                foreach (OnlineMob mob in monsters)
                {
                    if (!mob.IsSkillLaunched() || player != mob.playerToFollow)
                        continue;

                    if (!player.isDead && !player.isHit && player.DoesCollide(mob.skill))
                    {
                        mob.SkillHasReachedTarget();
                        connectedPlayers.ForEach(item => item.sound.Add(mob.skill.hit_SoundEffect.ToString()));

                        if (mob.position.X > player.position.X)
                            knck = -1;
                        else
                            knck = 1;

                        dmg = mob.AttackDamage();

                        player.Hit(dmg, knck);
                        break;
                    }
                }
            }
        }
        #endregion

        #region Public functions
        // Function returns whether position is inside a platform or not
        public bool IsLand(Vector2 position)
        {
            if (leftLimit < position.X && position.X < rightLimit &&
                topLimit < position.Y && position.Y < bottomLimit)
            {
                return (mapMask.At(position).isLand);
            }
            else
            {
                return true;
            }
        }

        // Function clears players list.
        public void ClearPlayers()
        {
            this.players.Clear();
        }

        // Function adds player to this
        public void AddPlayer(OnlinePlayer player)
        {
            this.players.Add(player);
        }
        #endregion

        #region Specific Creators
        public static OnlineMap CreateCity(ContentManager contentManager)
        {
            List<OnlinePlatform> platforms = new List<OnlinePlatform>();
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/city/city_0"), new Vector2(0, 500)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/city/city_1"), new Vector2(300, 350)));

            MonsterCreator[] creators = new MonsterCreator[0];

            return (new OnlineMap("City Center", platforms, 0, creators));
        }
        public static OnlineMap CreateTrainingGrounds(ContentManager contentManager)
        {
            List<OnlinePlatform> platforms = new List<OnlinePlatform>();
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/map1/platform1"), new Vector2(0, 500)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/map1/platform2"), new Vector2(200, 260)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/map1/platform2"), new Vector2(300, 170)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/map1/platform3"), new Vector2(1100, 350)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/map1/platform4"), new Vector2(1730, 163)));

            MonsterCreator[] creators = new MonsterCreator[2];
            creators[0] = OnlineMob.CreateTeddy;
            creators[1] = OnlineMob.CreateWorm;

            return new OnlineMap("Training Grounds", platforms, 15, creators);
        }
        public static OnlineMap CreateForest(ContentManager contentManager)
        {
            List<OnlinePlatform> platforms = new List<OnlinePlatform>();
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/forest/forest_0"), new Vector2(0, 600)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(0, 200)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(270, 260)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(540, 320)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(810, 260)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(1080, 200)));

            MonsterCreator[] creators = new MonsterCreator[1];
            creators[0] = OnlineMob.CreateMonkey;

            return new OnlineMap("Forest", platforms, 15, creators);
        }
        public static OnlineMap CreateLava(ContentManager contentManager)
        {
            List<OnlinePlatform> platforms = new List<OnlinePlatform>();
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/lava/lava_0"), new Vector2(0, 700)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/lava/lava_1"), new Vector2(350, 200)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/lava/lava_1"), new Vector2(250, 450)));
            platforms.Add(new OnlinePlatform(contentManager.Load<Texture2D>("maps/lava/lava_2"), new Vector2(1150, 350)));

            MonsterCreator[] creators = new MonsterCreator[1];
            creators[0] = OnlineMob.CreateDrake;

            return new OnlineMap("Lava", platforms, 7, creators);
        }
        #endregion
    }
}
