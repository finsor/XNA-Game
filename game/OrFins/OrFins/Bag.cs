using System;
using System.Collections.Generic;
using System.Linq;
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
    class Bag : DialogBox
    {
        #region Data
        private EquipmentType[] bag;
        private Action<Clothing, int> wearAction;
        private TextSprite money;
        #endregion

        #region Construction
        public Bag(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 scale, SpriteFont font, Action<Clothing, int> wearAction)
            : base(spriteBatch, texture, position, scale, font)
        {
            this.wearAction = wearAction;

            this.ProcessTextureAndCreateButtons();
        }
        private void ProcessTextureAndCreateButtons()
        {
            Color[] textureColors = new Color[texture.Height * texture.Width];
            texture.GetData<Color>(textureColors);

            Color buttonColor = textureColors[0];
            Color textPosColor = textureColors[1];

            List<EquipmentType> temp_bag = new List<EquipmentType>();

            for (int row = 0; row < texture.Height; row++)
            {
                for (int col = 2; col < texture.Width; col++)
                {
                    Color currentColor = textureColors[row * texture.Width + col];

                    if (currentColor == buttonColor)
                    {
                        temp_bag.Add(new EquipmentType(null,
                            new Button(spriteBatch, null, new Vector2(col, row), Vector2.One, Color.LightPink, this.CallWear)));
                    }

                    if (currentColor == textPosColor)
                    {
                        money = new TextSprite(spriteBatch, new Vector2(col, row), Color.Black, "", font);
                    }
                }
            }

            foreach (EquipmentType equipmentType in temp_bag)
                base.AddButton(equipmentType.button);

            this.bag = temp_bag.ToArray();

            texture.SetData<Color>(textureColors);

        }
        #endregion

        #region Drawing functions
        public override void DrawObject(Vector2 windowScale)
        {
            base.DrawObject(windowScale);

            this.money.DrawObject(windowScale);

            this.Draw_Hover_Details();
        }

        // Function draws details for items which hovers above using mouse
        private void Draw_Hover_Details()
        {
            Vector2 mouse_pos;
            Rectangle destinationRectangle;

            foreach (EquipmentType eq in bag)
            {
                // If mouse hovering above item
                if (eq.button.isHovered && eq.clothing != null)
                {
                    mouse_pos = Mouse.GetState().Vector();

                    destinationRectangle = new Rectangle(
                        (int)(mouse_pos.X),
                        (int)(mouse_pos.Y),
                        100,
                        100);

                    // Draw background
                    spriteBatch.Draw(Service.pixel, destinationRectangle, Color.Gray);

                    // Draw item details
                    spriteBatch.DrawString(font, eq.clothing.folder.ToString(), mouse_pos + new Vector2(10, 10), Color.Red);
                    spriteBatch.DrawString(font, "Level: " + eq.clothing.minLevel.ToString(), mouse_pos + new Vector2(10, 30), Color.White);
                    spriteBatch.DrawString(font, "Defence: " + eq.clothing.defence.ToString(), mouse_pos + new Vector2(10, 50), Color.White);
                    spriteBatch.DrawString(font, "Strength: " + eq.clothing.strength.ToString(), mouse_pos + new Vector2(10, 70), Color.White);
                }
            }
        }
        #endregion

        #region Update functions
        public override void Update(MouseState previousMouseState, MouseState currentMouseState, Vector2 windowScale)
        {
            base.Update(previousMouseState, currentMouseState, windowScale);

            this.money.Update(this.position);
        }
        #endregion

        #region Private functions
        private void CallWear()
        {
            for (int i = 0; i < bag.Length; i++)
            {
                if (bag[i].button.isClicked)
                {
                    wearAction(bag[i].clothing, i);
                    bag[i].button.ChangeAppearance(null);
                    return;
                }
            }
        }
        #endregion

        #region Public functions
        public void SetClothing(Clothing[] clothing)
        {
            int i;
            for (i = 0; i < clothing.Length; i++)
            {
                bag[i].SetClothing(clothing[i]);
            }
        }
        public void SetMoney(object money)
        {
            this.money.Update(money.ToString());
        }
        #endregion
    }
}
