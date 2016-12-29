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
    abstract class GameManager
    {
        #region Data
        protected Player player;
        protected Map current_map;
        protected SpriteBatch spriteBatch;
        protected Camera camera;
        private Map base_map;
        private HUD hud;
        private int load_period { get; set; }
        #endregion

        #region Properties
        public bool IsLoading
        {
            get
            {
                return (load_period != 0);
            }
            private set
            {
                if (value == true)
                {
                    load_period = 30;
                }
                else
                {
                    load_period = 0;
                }
            }
        }
        #endregion

        #region Construction
        public GameManager(SpriteBatch spriteBatch, Camera cam, Player player, Map base_map, HUD hud)
        {
            this.camera = cam;
            this.player = player;
            this.base_map = base_map;
            this.current_map = base_map;
            this.spriteBatch = spriteBatch;
            this.hud = hud;

            this.player.position = current_map.platforms[0].CreatePositionUsingOffset(15);

            ChangeMap(base_map);
            IsLoading = false;
        }
        #endregion

        #region Draw functions
        protected void Draw_Map_And_Player(Vector2 windowScale, GameState gameState)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.mat);

            // Draw current map
            current_map.Draw(windowScale, gameState);

            // Draw this
            player.DrawObject(windowScale);

            spriteBatch.End();
        }
        protected void DrawHud(Vector2 windowScale, params Vector2[] players_positions)
        {
            spriteBatch.Begin();

            hud.DrawObject(windowScale);

            hud.DrawMinimap(current_map, windowScale, players_positions);

            spriteBatch.End();
        }
        #endregion

        #region Update functions
        protected void Update_Map_And_Player(GameTime gameTime, Vector2 windowScale, GameState gameState)
        {
            camera.UpdateMat(current_map, windowScale);
            current_map.Update(gameTime, camera, spriteBatch, gameState, windowScale);

            if (IsLoading)
            {
                return;
            }

            player.Update(gameTime, current_map);
        }
        protected void Update_Hud(Vector2 windowScale)
        {
            this.hud.Update(windowScale);
        }

        protected void PlayerTeleports()
        {
            if (!player.AttemptsToTeleport)
                return;

            Map temp = current_map;

            foreach (Portal portal in current_map.portals)
            {
                if (Vector2.Distance(player.position, portal.position) <= 20)
                {
                    temp = current_map;
                    SoundDictionary.Play(SoundEffects.Teleport);
                    ChangeMap(portal.destination);

                    foreach (Portal nextPortal in current_map.portals)
                    {
                        if (nextPortal.destination == temp)
                        {
                            player.position = nextPortal.position;
                            return;
                        }
                    }
                }
            }
        }
        protected void Update_Load_Time()
        {
            this.load_period = (int)MathHelper.Clamp(--load_period, 0, 150);
        }
        protected int Knockback_Direction(LivingObject attacker, LivingObject victim)
        {
            if (attacker.position.X > victim.position.X)
                return -1;
            return 1;
        }
        private void ChangeMap(Map map)
        {
            this.current_map = map;
            SoundDictionary.SetBackGroundMusic(map.bgm);
            this.IsLoading = true;
        }
        public void PlayerRevive()
        {
            ChangeMap(base_map);

            // Update player's position and revive
            player.position = current_map.platforms[0].CreatePositionUsingOffset(50);
            player.Revive();

            hud.SetDialogBox(null);
        }
        #endregion

        #region Abstract functions
        public abstract void Draw(Vector2 windowScale, GraphicsDevice GraphicsDevice);
        public abstract void Update(GameTime gameTime, Vector2 windowScale);
        protected abstract void PlayerAttacked();
        protected abstract void PlayerAttacks();
        protected abstract void PlayerPicksUp();
        protected abstract void ProcessSkills();
        #endregion
    }
}
