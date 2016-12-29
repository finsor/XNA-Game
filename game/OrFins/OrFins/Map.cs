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
    struct MapRecord
    {
        public bool isLand { get; set; }
        public Platform platform { get; set; }
    }

    class Map
    {
        public delegate Mob MonsterCreator(SpriteBatch spriteBatch, Platform platform, Map map);

        public float leftLimit { get; private set; }
        public float rightLimit { get; private set; }
        public float bottomLimit { get; private set; }
        public float topLimit { get; private set; }

        public MapRecord[,] mapMask { get; private set; }
        public List<Platform> platforms { get; private set; }
        public List<Rope> ropes { get; private set; }
        public List<Mob> monsters { get; private set; }
        public List<NPC> npcs { get; private set; }
        public List<Portal> portals { get; private set; }
        public List<BackGroundObject> bgObjects { get; private set; }
        public List<PickupItem> pickup { get; private set; }

        private MonsterCreator[] monsterCreators;
        private int maxAmountOfMonsters;

        public string name { get; private set; }

        public BGM bgm { get; private set; }

        #region Properties
        public Vector2 BottomLeftCorner
        {
            get
            {
                return (new Vector2(this.leftLimit, this.bottomLimit));
            }
        }
        public Vector2 TopLeftCorner
        {
            get
            {
                return (new Vector2(this.leftLimit, this.topLimit));
            }
        }
        public Vector2 TopRightCorner
        {
            get
            {
                return (new Vector2(this.rightLimit, this.topLimit));
            }
        }
        public Vector2 BottomRightCorner
        {
            get
            {
                return (new Vector2(this.rightLimit, this.bottomLimit));
            }
        }
        public float Width
        {
            get
            {
                return (this.rightLimit - this.leftLimit);
            }
        }
        public float Height
        {
            get
            {
                return (this.bottomLimit - this.topLimit);
            }
        }
        #endregion

        #region Construction
        public Map(string name, List<Platform> platforms, List<Rope> ropes, List<NPC> npcs, List<BackGroundObject> bgObjects, int maxAmountOfMonsters, MonsterCreator[] monsterCreators, BGM bgm)
        {
            this.name = name;
            this.platforms = platforms;
            this.ropes = ropes;
            this.npcs = npcs;
            this.bgObjects = bgObjects;
            this.pickup = new List<PickupItem>();
            this.portals = new List<Portal>();
            this.monsters = new List<Mob>();
            this.maxAmountOfMonsters = maxAmountOfMonsters;
            this.monsterCreators = monsterCreators;
            this.bgm = bgm;

            this.topLimit = 0;
            this.leftLimit = 0;
            this.rightLimit = platforms[0].surroundingRectangle.Right;
            this.bottomLimit = platforms[0].surroundingRectangle.Bottom;

            foreach (BackGroundObject bgo in bgObjects)
                bgo.InitializeScale(this.Width, this.Height);

            CreateMapMask();
        }

        private void CreateMapMask()
        {
            mapMask = new MapRecord[(int)rightLimit, (int)bottomLimit];

            // For each platform, store its colors in mapMask as well as its platform
            foreach (Platform platform in platforms)
            {
                Color[] platformColors = new Color[platform.surroundingRectangle.Width * platform.surroundingRectangle.Height];
                platform.texture.GetData<Color>(platformColors);

                for (int y = 0; y < platform.texture.Height; y++)
                {
                    for (int x = 0; x < platform.texture.Width; x++)
                    {
                        // Store platform
                        mapMask[platform.surroundingRectangle.X + x, platform.surroundingRectangle.Y + y].platform = platform;

                        // Store boolean: true for 'not land' else false
                        mapMask[platform.surroundingRectangle.X + x, platform.surroundingRectangle.Y + y].isLand =
                            platformColors[y * platform.texture.Width + x] != Color.Transparent;
                    }
                }
            }
        }
        #endregion

        #region Draw functions
        public void Draw(Vector2 windowScale, GameState gameState)
        {
            foreach (BackGroundObject bgo in bgObjects)
                bgo.DrawObject(windowScale);

            foreach (Platform platform in platforms)
                platform.DrawObject(windowScale);

            foreach (Rope rope in ropes)
                rope.DrawObject(windowScale);

            foreach (NPC npc in npcs)
                npc.DrawObject(windowScale);

            foreach (Portal portal in portals)
                portal.DrawObject(windowScale);

            if (gameState == GameState.SinglePlayer)
            {
                foreach (Mob monster in monsters)
                    monster.DrawObject(windowScale);

                foreach (PickupItem item in pickup)
                    item.DrawObject(windowScale);
            }
        }
        #endregion

        #region Update functions
        public void Update(GameTime gameTime, Camera camera, SpriteBatch spriteBatch, GameState gameState, Vector2 windowScale)
        {
            // Update background
            foreach (BackGroundObject bgObject in bgObjects)
                bgObject.Update(camera, this);

            foreach (Portal portal in portals)
                portal.Update();

            foreach (NPC npc in npcs)
                npc.Update(windowScale, camera);

            if (gameState == GameState.SinglePlayer)
            {
                SpawnMonsters(spriteBatch);

                // Update monsters
                foreach (Mob monster in monsters)
                    monster.Update(gameTime, this);

                RemoveDeadMobs(spriteBatch);

                foreach (PickupItem item in pickup)
                    item.Update();

                // Remove timed-out items
                pickup.RemoveAll(item => item.LifeTimerEnded);
            }
        }
        #endregion

        #region private functions
        private void SpawnMonsters(SpriteBatch spriteBatch)
        {
            if (Service.Random(0, 100) == 0 && monsters.Count < maxAmountOfMonsters)
            {
                // Pull a random MonsterCreate
                MonsterCreator monster_creator = monsterCreators[Service.Random(0, monsterCreators.Length - 1)];

                // Create a monster on a random position
                Mob monster = monster_creator(spriteBatch, platforms[Service.Random(0, platforms.Count - 1)], this);

                // Add monster to map
                monsters.Add(monster);
            }
        }
        private void RemoveDeadMobs(SpriteBatch spriteBatch)
        {
            // Find all dead mobs, and for each of them, call RemoveMob on.
            this.monsters.FindAll(item => item.IsDead && item.IsLastAnimationFrame).ForEach(item => RemoveMob(item, spriteBatch));
        }
        private void RemoveMob(Mob monster, SpriteBatch spriteBatch)
        {
            // Mob drops money
            pickup.Add(new PickupItem(spriteBatch, Folders.pickup, States.money, monster.position, 1800, monster.MoneyDropped()));

            monsters.Remove(monster);
        }
        #endregion

        #region public functions
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
        public void RemovePickupItem(PickupItem item)
        {
            this.pickup.Remove(item);
        }
        #endregion

        #region Specific Creators
        public static Map CreateCity(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, ContentManager contentManager, HUD hud)
        {
            List<Platform> platforms = new List<Platform>();
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/city/city_0"), new Vector2(0, 500), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/city/city_1"), new Vector2(300, 350), Color.White, SpriteEffects.None, 0));

            List<Rope> ropes = new List<Rope>();

            List<NPC> npcs = new List<NPC>();
            npcs.Add(new NPC(Folders.npc, States.chiang, spriteBatch, platforms[0].CreatePositionUsingOffset(50), Color.White, Vector2.One, 10, hud.OpenShop));
            npcs.Add(new NPC(Folders.npc, States.marco, spriteBatch, platforms[1].CreatePositionUsingOffset(15), Color.White, Vector2.One, 10));

            List<BackGroundObject> bgObjects = new List<BackGroundObject>();
            bgObjects.Add(new BackGroundObject(Folders.cityBackGround, States.back, spriteBatch, new Vector2(platforms[0].leftmostPixel, platforms[0].position.Y), new Vector2(1.05f), MovementCode.Static));

            MonsterCreator[] creators = new MonsterCreator[0];

            return new Map("City Center", platforms, ropes, npcs, bgObjects, 0, creators, BGM.City_Center);
        }
        public static Map CreateTrainingGrounds(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, ContentManager contentManager)
        {
            List<Platform> platforms = new List<Platform>();
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/map1/platform1"), new Vector2(0, 500), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/map1/platform2"), new Vector2(200, 260), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/map1/platform2"), new Vector2(300, 170), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/map1/platform3"), new Vector2(1100, 350), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/map1/platform4"), new Vector2(1730, 163), Color.White, SpriteEffects.None, 0));

            List<Rope> ropes = new List<Rope>();
            ropes.Add(new Rope(Folders.rope, spriteBatch, platforms[1], 150, 0));
            ropes.Add(new Rope(Folders.rope, spriteBatch, platforms[2], 250, 0));
            ropes.Add(new Rope(Folders.rope, spriteBatch, platforms[3], 140, 0));
            ropes.Add(new Rope(Folders.rope, spriteBatch, platforms[4], 230, 2));

            List<NPC> npcs = new List<NPC>();

            List<BackGroundObject> bgObjects = new List<BackGroundObject>();
            bgObjects.Add(new BackGroundObject(Folders.greenBackGround, States.back, spriteBatch, new Vector2(platforms[0].surroundingRectangle.Left, platforms[0].surroundingRectangle.Bottom), new Vector2(1.05f), MovementCode.Linear));
            bgObjects.Add(new BackGroundObject(Folders.greenBackGround, States.middle, spriteBatch, new Vector2(platforms[0].rightmostPixel + 420, platforms[0].bottomPixel + 90), new Vector2(0.6f), MovementCode.Dependent, 2));
            bgObjects.Add(new BackGroundObject(Folders.greenBackGround, States.front, spriteBatch, new Vector2(platforms[0].leftmostPixel - 200, platforms[0].bottomPixel), new Vector2(0.6f, 0.65f), MovementCode.Dependent, 5));

            MonsterCreator[] creators = new MonsterCreator[2];
            creators[0] = Mob.CreateTeddy;
            creators[1] = Mob.CreateWorm;

            return new Map("Training Grounds", platforms, ropes, npcs, bgObjects, 15, creators, BGM.TrainingGrounds);
        }
        public static Map CreateForest(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, ContentManager contentManager)
        {
            List<Platform> platforms = new List<Platform>();
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/forest/forest_0"), new Vector2(0, 600), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(0, 200), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(270, 260), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(540, 320), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(810, 260), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/forest/forest_1"), new Vector2(1080, 200), Color.White, SpriteEffects.None, 0));

            List<Rope> ropes = new List<Rope>();
            ropes.Add(new Rope(Folders.rope, spriteBatch, platforms[3], 130, 1));

            List<NPC> npcs = new List<NPC>();

            List<BackGroundObject> bgObjects = new List<BackGroundObject>();
            bgObjects.Add(new BackGroundObject(Folders.forestBackGround, States.back, spriteBatch, new Vector2(platforms[0].surroundingRectangle.Left, platforms[0].surroundingRectangle.Bottom), new Vector2(1.02f, 1), MovementCode.Linear));
            bgObjects.Add(new BackGroundObject(Folders.forestBackGround, States.middle, spriteBatch, new Vector2(platforms[0].leftmostPixel - 200, platforms[0].bottomPixel), new Vector2(1f, 0.5f), MovementCode.Dependent, 5));
            bgObjects.Add(new BackGroundObject(Folders.forestBackGround, States.front, spriteBatch, new Vector2(platforms[0].leftmostPixel, platforms[0].position.Y - platforms[0].Height + 10), new Vector2(1.01f, 0.375f), MovementCode.Static));

            MonsterCreator[] creators = new MonsterCreator[1];
            creators[0] = Mob.CreateMonkey;

            return new Map("Forest", platforms, ropes, npcs, bgObjects, 10, creators, BGM.Forest);
        }
        public static Map CreateLava(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, ContentManager contentManager)
        {
            List<Platform> platforms = new List<Platform>();
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/lava/lava_0"), new Vector2(0, 700), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/lava/lava_1"), new Vector2(350, 200), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/lava/lava_1"), new Vector2(250, 450), Color.White, SpriteEffects.None, 0));
            platforms.Add(new Platform(spriteBatch, contentManager.Load<Texture2D>("maps/lava/lava_2"), new Vector2(1150, 350), Color.White, SpriteEffects.None, 0));

            List<Rope> ropes = new List<Rope>();
            ropes.Add(new Rope(Folders.rope, spriteBatch, platforms[1], 370, 1));
            ropes.Add(new Rope(Folders.rope, spriteBatch, platforms[2], 70, 1));
            ropes.Add(new Rope(Folders.rope, spriteBatch, platforms[3], 130, 2));

            List<NPC> npcs = new List<NPC>();

            List<BackGroundObject> bgObjects = new List<BackGroundObject>();
            bgObjects.Add(new BackGroundObject(Folders.lavaBackGround, States.back, spriteBatch, new Vector2(platforms[0].surroundingRectangle.Left, platforms[0].surroundingRectangle.Bottom), new Vector2(1.02f, 1), MovementCode.Static));
            bgObjects.Add(new BackGroundObject(Folders.lavaBackGround, States.middle, spriteBatch, new Vector2(platforms[0].leftmostPixel, platforms[0].bottomPixel), new Vector2(0.5f, 1f), MovementCode.Dependent, 20));
            bgObjects.Add(new BackGroundObject(Folders.lavaBackGround, States.front, spriteBatch, new Vector2(platforms[0].leftmostPixel + 1000, platforms[0].bottomPixel), new Vector2(0.5f, 1f), MovementCode.Dependent, 3));

            MonsterCreator[] creators = new MonsterCreator[1];
            creators[0] = Mob.CreateDrake;

            return new Map("Lava", platforms, ropes, npcs, bgObjects, 7, creators, BGM.Dungeon);
        }
        #endregion
    }
}
