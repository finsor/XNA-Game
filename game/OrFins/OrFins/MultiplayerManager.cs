using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace OrFins
{
    class MultiplayerManager : GameManager
    {
        #region Data
        private OnlineClient onlineClient;
        private SpriteFont font;
        private List<Vector2> players_positions;
        #endregion

        #region Construction
        public MultiplayerManager(OnlineClient onlineClient, SpriteBatch spriteBatch, Camera camera, Player player, Map base_map, HUD hud, SpriteFont font)
            : base(spriteBatch, camera, player, base_map, hud)
        {
            this.onlineClient = onlineClient;
            this.font = font;
            this.players_positions = new List<Vector2>();

            // Waiting for synchronization with server
            onlineClient.Connect();
            while (!onlineClient.GetString().Equals("SYNC")) ;
        }
        #endregion

        #region Drawing functions
        public override void Draw(Vector2 windowScale, GraphicsDevice GraphicsDevice)
        {
            if (!base.IsLoading)
            {
                base.Draw_Map_And_Player(windowScale, GameState.MultiPlayer);

                this.Draw_Online(windowScale);

                base.DrawHud(windowScale, players_positions.ToArray());
            }
            else
            {
                GraphicsDevice.Clear(Color.Black);
            }
        }

        private void Draw_Online(Vector2 windowScale)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.mat);

            // Draw other players, monsters and pickup items of this.map
            Draw_Objects(windowScale);

            // Draw monsters' health bars and pickup items' existence bars
            Draw_Bars(windowScale);

            // Draw players' names
            Draw_Names(windowScale);

            spriteBatch.End();
        }
        private void Draw_Objects(Vector2 windowScale)
        {
            Folders folder;
            States state;
            int index;
            Vector2 position;
            SpriteEffects effects;
            Texture2D texture;
            Vector2 origin;
            Rectangle sourceRectangle;
            ImageProcessor page;

            players_positions.Clear();
            players_positions.Add(player.position);
            string line;

            line = onlineClient.GetString();
            while (line != null && !line.Equals("DONE"))
            {
                folder = (Folders)Enum.Parse(typeof(Folders), onlineClient.GetString());
                state = (States)Enum.Parse(typeof(States), onlineClient.GetString());
                index = int.Parse(onlineClient.GetString());
                position = onlineClient.GetVector2();
                effects = (SpriteEffects)Enum.Parse(typeof(SpriteEffects), onlineClient.GetString());

                page = SpritesDictionary.dictionary[folder][state];

                index %= page.rectangles.Count;

                texture = page.texture;
                origin = page.origins[index];
                sourceRectangle = page.rectangles[index];

                if (effects == SpriteEffects.FlipHorizontally)
                    origin = new Vector2(sourceRectangle.Width - origin.X, origin.Y);

                spriteBatch.Draw(texture, position * windowScale, sourceRectangle, Color.White, 0f, origin, windowScale, effects, 0);

                if (line.Equals("PLAYER"))
                {
                    players_positions.Add(position);
                }

                line = onlineClient.GetString();
            }
        }
        private void Draw_Bars(Vector2 windowScale)
        {
            States state;
            Texture2D texture;
            Rectangle destinationRectangle;

            string line;
            line = onlineClient.GetString();
            while (line != null && line.Equals("SENDING"))
            {
                state = (States)Enum.Parse(typeof(States), onlineClient.GetString());

                destinationRectangle = new Rectangle(
                    onlineClient.GetInt(),
                    onlineClient.GetInt(),
                    onlineClient.GetInt(),
                    onlineClient.GetInt());

                texture = SpritesDictionary.dictionary[Folders.hud][state].texture;

                spriteBatch.Draw(texture, destinationRectangle.Multiply(windowScale), Color.White);

                line = onlineClient.GetString();
            }
        }
        private void Draw_Names(Vector2 windowScale)
        {
            Vector2 position;
            string name;
            Rectangle bg_rec;
            Vector2 origin;
            Vector2 string_length_in_pixels;

            string line;
            line = onlineClient.GetString();
            while (line != null && line.Equals("SENDING"))
            {
                name = onlineClient.GetString();
                string_length_in_pixels = font.MeasureString(name);

                position = onlineClient.GetVector2();
                origin = Vector2.UnitX * string_length_in_pixels.X / 2;

                bg_rec = new Rectangle(
                    (int)((position.X - origin.X)),
                    (int)((position.Y - 1)),
                    (int)(string_length_in_pixels.X + 5),
                    (int)(string_length_in_pixels.Y + 1));

                spriteBatch.Draw(Service.pixel, bg_rec.Multiply(windowScale), Color.Black * 0.7f);

                spriteBatch.DrawString(font, name, position * windowScale, Color.White, 0f, origin, windowScale, SpriteEffects.None, 0);

                line = onlineClient.GetString();
            }
        }
        #endregion

        #region Update functions
        public override void Update(GameTime gameTime, Vector2 windowScale)
        {
            base.Update_Map_And_Player(gameTime, windowScale, GameState.MultiPlayer);

            if (IsLoading)
            {
                base.Update_Load_Time();
            }
            else
            {
                base.PlayerTeleports();

                this.onlineClient.Send(current_map.name);
                this.player.SendProfile(onlineClient);

                this.PlayerAttacked();
                this.PlayerAttacks();
                this.PlayerPicksUp();
                this.ProcessSkills();
                this.GetSound();

                base.Update_Hud(windowScale);
            }
        }

        protected override void PlayerAttacked()
        {
            int damage = onlineClient.GetInt();
            int knockback_mul = onlineClient.GetInt();

            if (!player.isHit)
            {
                player.Hit(damage, knockback_mul);
            }
        }
        protected override void PlayerAttacks()
        {
            onlineClient.Send(player.CanHit.ToString());

            if (player.CanHit)
            {
                #region Create player's attacking rectangle and send to server
                // Assuming player faces right
                Rectangle playerAttackRectangle = new Rectangle(
                    (int)(player.position.X),
                    (int)(player.surroundingRectangle.Top),
                    (int)(player.surroundingRectangle.Width * 1.5f),
                    (int)(player.surroundingRectangle.Height));

                // if player faces right
                if (player.effects == SpriteEffects.None)
                {
                    playerAttackRectangle.X -= playerAttackRectangle.Width;
                }

                onlineClient.Send(
                    playerAttackRectangle.X.ToString(),
                    playerAttackRectangle.Y.ToString(),
                    playerAttackRectangle.Width.ToString(),
                    playerAttackRectangle.Height.ToString());

                #endregion
            }

            player.GainEXP(onlineClient.GetInt());
        }
        protected override void PlayerPicksUp()
        {
            bool is_attempting_to_pick_up = (player.AttemptsToPickUp && !base.IsLoading);

            onlineClient.Send(is_attempting_to_pick_up.ToString());

            if (is_attempting_to_pick_up)
            {
                player.SendSurroundingRectangle(onlineClient);
            }

            int money = onlineClient.GetInt();
            if (money > 0)
            {
                player.GainMoney(money);
                SoundDictionary.Play(SoundEffects.PickUp);
            }
        }
        protected override void ProcessSkills()
        {
            Vector2 target;

            // If player is just launching skill
            onlineClient.Send(player.IsLaunchingSkill.ToString());

            if (player.IsLaunchingSkill)
            {
                target = onlineClient.GetVector2();
                if (target != Vector2.Zero)
                    player.LaunchSkill(target);
                else
                    player.LaunchSkill(null);
            }

            // Update player skill's target
            onlineClient.Send(player.IsSkillLaunched.ToString());

            if (player.IsSkillLaunched)
            {
                target = onlineClient.GetVector2();

                if (target != Vector2.Zero)
                {
                    player.ResetSkillOnlineTarget(target);
                }

                // If skill has reached its target
                onlineClient.Send(player.HasSkillReachedTarget().ToString());

                if (player.HasSkillReachedTarget())
                {
                    player.SkillHasReachedTarget();
                }
            }
        }
        private void GetSound()
        {
            SoundEffects se;
            while (!onlineClient.GetString().Equals("DONE"))
            {
                se = (SoundEffects)Enum.Parse(typeof(SoundEffects), onlineClient.GetString());
                SoundDictionary.Play(se);
            }
        }
        #endregion
    }
}