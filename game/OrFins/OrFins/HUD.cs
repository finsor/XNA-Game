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
    class HUD : Sprite
    {
        #region DATA
        private SpriteFont font;

        private bool showHud;

        private Player player;

        private Rectangle destinationRectangle;

        private TextSprite hp;
        private TextSprite mp;
        private TextSprite xp;

        private Bar hpBar;
        private Bar mpBar;
        private Bar xpBar;

        private Level level;


        private DialogBox deathBox;
        private DialogBox dialogBox;

        private MouseState previousMouseState;
        private MouseState currentMouseState;

        private Equipment equipmentWindow;
        private Bag bag;
        private Shop shop;
        private Texture2D keyboardSettings;

        private ButtonDrawer DRAW_BUTTONS;
        private ButtonUpdater UPDATE_BUTTONS;
        #endregion

        #region Construction
        public HUD(SpriteFont font, SpriteBatch spriteBatch, Player player, Equipment equipmentWindow, Bag bag, Shop shop, DialogBox deathBox, Texture2D keyboardSettings)
            : base(Folders.hud, States.hud, spriteBatch, Vector2.Zero, Color.White, 0f, Vector2.One, SpriteEffects.None, 0f)
        {
            this.player = player;
            this.font = font;
            this.equipmentWindow = equipmentWindow;
            this.bag = bag;
            this.shop = shop;
            this.keyboardSettings = keyboardSettings;

            this.destinationRectangle = new Rectangle(0, Service.screenHeight - (int)base.Height, (int)(base.Width), (int)(base.Height));

            #region Create TextSprites
            Vector2 hpTextDST = new Vector2(this.destinationRectangle.X + 109 * this.scale.X,
                            this.destinationRectangle.Y + 7 * this.scale.Y / 2);

            hp = new TextSprite(spriteBatch, hpTextDST, Color.White, "", font);

            Vector2 mpTextDST = new Vector2(this.destinationRectangle.X + 213 * this.scale.X,
                            this.destinationRectangle.Y + 7 * this.scale.Y / 2);

            mp = new TextSprite(spriteBatch, mpTextDST, Color.White, "", font);

            Vector2 xpTextDST = new Vector2(this.destinationRectangle.X + 316 * this.scale.X,
                this.destinationRectangle.Y + 7 * this.scale.Y / 2);

            xp = new TextSprite(spriteBatch, xpTextDST, Color.White, "", font);
            #endregion

            #region Create Bars

            this.hpBar = new Bar(Folders.hud, States.hpBar, spriteBatch, this.color, this.rotation, this.scale, this.effects, this.layerDepth,
                new Rectangle((int)(this.destinationRectangle.X + 091 * this.scale.X),
                                (int)(this.destinationRectangle.Y + 19 * this.scale.Y),
                                0,
                                (int)(17 * this.scale.X)));

            this.mpBar = new Bar(Folders.hud, States.mpBar, spriteBatch, this.color, this.rotation, this.scale, this.effects, this.layerDepth,
                new Rectangle((int)(this.destinationRectangle.X + 191 * this.scale.X),
                                (int)(this.destinationRectangle.Y + 19 * this.scale.Y),
                                0,
                                (int)(17 * this.scale.X)));

            this.xpBar = new Bar(Folders.hud, States.xpBar, spriteBatch, this.color, this.rotation, this.scale, this.effects, this.layerDepth,
                    new Rectangle((int)(this.destinationRectangle.X + 291 * this.scale.X),
                                (int)(this.destinationRectangle.Y + 19 * this.scale.Y),
                                0,
                                (int)(17 * this.scale.X)));

            #endregion

            #region Create levels
            this.level = new Level(spriteBatch, this.color, this.rotation, this.scale, this.effects, this.layerDepth, Folders.hud, States.levels,
    new Rectangle((int)(this.destinationRectangle.X + 36 * this.scale.X),
                    (int)(this.destinationRectangle.Y + 14 * this.scale.Y),
                    (int)(10 * this.scale.X),
                    (int)(17 * this.scale.X)));
            #endregion

            this.deathBox = deathBox;

            showHud = true;
        }
        #endregion

        #region Draw functions
        public override void DrawObject(Vector2 windowScale)
        {
            if (showHud)
            {
                base.DrawObject(destinationRectangle, windowScale);

                DrawBars(windowScale);

                DRAW_BUTTONS(windowScale);

                if (dialogBox != null && dialogBox.isDrawable)
                    dialogBox.DrawObject(windowScale);

                if (Keyboard.GetState().IsKeyDown(Keys.Tab))
                {
                    spriteBatch.DrawString(font, "Strength: " + player.strength, Vector2.Zero, Color.White);
                    spriteBatch.DrawString(font, "Defence : " + player.defence, Vector2.UnitY * 20, Color.White);
                    spriteBatch.Draw(keyboardSettings, Vector2.One * 50, Color.White);
                }
            }
        }
        private void DrawBars(Vector2 windowScale)
        {
            hpBar.DrawObject(windowScale);
            mpBar.DrawObject(windowScale);
            xpBar.DrawObject(windowScale);
            level.DrawObject(player.level, windowScale);

            hp.DrawObject(windowScale);
            mp.DrawObject(windowScale);
            xp.DrawObject(windowScale);
        }

        public void DrawMinimap(Map map, Vector2 windowScale, params Vector2[] players)
        {
            if (showHud)
            {
                float minimap_rate = 1 / 7.5f;

                Rectangle minimapBackground = new Rectangle(0, 0, (int)(map.Width), (int)(map.Height));

                spriteBatch.Draw(Service.pixel, minimapBackground.Multiply(windowScale * minimap_rate), Color.Gray * 0.5f);


                foreach (Platform plat in map.platforms)
                {
                    spriteBatch.Draw(plat.texture, plat.surroundingRectangle.Multiply(minimap_rate * windowScale), Color.White * 0.5f);
                }

                foreach (Portal portal in map.portals)
                {
                    spriteBatch.Draw(portal.texture, portal.surroundingRectangle.Multiply(windowScale * minimap_rate), portal.sourceRectangle, Color.White * 0.5f);
                }

                Rectangle player_pos_rectangle;
                Color color;

                for (int i = 0; i < players.Length; i++)
                {
                    if (i == 0)
                        color = Color.Yellow;
                    else
                        color = Color.Red;

                    player_pos_rectangle = new Rectangle(
                        (int)(players[i].X - 30),
                        (int)(players[i].Y - 80),
                        (int)(60),
                        (int)(80));

                    spriteBatch.Draw(Service.pixel, player_pos_rectangle.Multiply(minimap_rate * windowScale), color * 0.5f);
                }
            }
        }
        #endregion

        #region Update functions
        public void Update(Vector2 windowScale)
        {
            if (showHud)
            {
                previousMouseState = currentMouseState;
                currentMouseState = Mouse.GetState();

                UpdateBars();

                UPDATE_BUTTONS(previousMouseState, currentMouseState, windowScale);

                UpdateDialogBox(windowScale);

                UpdateTextSprites();
            }
        }
        private void UpdateBars()
        {
            hpBar.Update(player.hp, player.maxHP);
            mpBar.Update(player.mp, player.maxMP);
            xpBar.Update(player.exp, player.requiredEXP);
        }
        private void UpdateDialogBox(Vector2 windowScale)
        {
            // If player is dead, then show the deathBox
            if (player.IsDead)
            {
                SetDialogBox(deathBox);
            }
            else // Control tabs using the keyboard
            {
                Open_Tabs_Using_Keyboard();
            }

            // If a dialog box is on
            if (dialogBox != null)
            {
                // If dialogBox is active, then update it
                if (dialogBox.isDrawable)
                {

                    dialogBox.Update(previousMouseState, currentMouseState, windowScale);

                    if (dialogBox is Bag)
                    {
                        bag.SetClothing(player.bag);
                        bag.SetMoney(player.money);
                    }
                }
                else // Remove the dialog box
                {
                    SetDialogBox(null);
                }
            }
        }
        private void Open_Tabs_Using_Keyboard()
        {
            KeyboardState kbs = Keyboard.GetState();

            if (kbs.IsKeyDown(Keys.B))
            {
                this.OpenBag();
            }
            if (kbs.IsKeyDown(Keys.E))
            {
                this.OpenEquipment();
            }
            if (kbs.IsKeyDown(Keys.Escape))
            {
                this.SetDialogBox(null);
            }
        }
        private void UpdateTextSprites()
        {
            hp.Update(player.hp + "/" + player.maxHP);
            mp.Update(player.mp + "/" + player.maxMP);
            xp.Update(player.exp + "/" + player.requiredEXP + "( " + ((float)player.exp * 100 / player.requiredEXP).ToString("n2") + "% )");
        }
        #endregion

        #region Public functions
        public void SetDialogBox(DialogBox box)
        {
            this.dialogBox = box;
            if (box != null)
                this.dialogBox.isDrawable = true;
        }
        public void AddButton(Button button)
        {
            DRAW_BUTTONS += button.DrawObject;
            UPDATE_BUTTONS += button.Update;
        }
        public void OpenEquipment()
        {
            if (dialogBox != equipmentWindow)
            {
                equipmentWindow.SetClothing(player.equipment);
                SetDialogBox(equipmentWindow);

                SoundDictionary.Play(SoundEffects.pageFlip);
            }
        }
        public void OpenBag()
        {
            if (dialogBox != bag)
            {
                bag.SetClothing(player.bag);
                SetDialogBox(bag);

                SoundDictionary.Play(SoundEffects.pageFlip);
            }
        }
        public void OpenShop()
        {
            if (dialogBox != shop)
            {
                SetDialogBox(shop);
                shop.Reload_Player_Bag();

                SoundDictionary.Play(SoundEffects.pageFlip);
            }
        }
        #endregion
    }
}
