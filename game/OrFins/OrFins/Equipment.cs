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
    class Equipment : DialogBox
    {
        #region Data
        private Dictionary<ClothingType, EquipmentType> clothingButtons;
        private Action<Clothing> unwearAction;
        #endregion

        #region Construction
        public Equipment(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 scale, SpriteFont font, Action<Clothing> unwearAction)
            : base(spriteBatch, texture, position, scale, font)
        {
            this.clothingButtons = new Dictionary<ClothingType, EquipmentType>();
            this.unwearAction = unwearAction;

            this.ProcessTextureAndCreateButtons();
        }

        private void ProcessTextureAndCreateButtons()
        {
            Color[] textureColors = new Color[texture.Height * texture.Width];
            texture.GetData<Color>(textureColors);

            Color head = textureColors[0];
            Color top = textureColors[1];
            Color bottom = textureColors[2];
            Color shoe = textureColors[3];
            Color weapon = textureColors[4];

            Dictionary<Color, Button> temp_buttons = new Dictionary<Color, Button>();

            for (int col = 0; col < texture.Width; col++)
            {
                for (int row = 0; row < texture.Height; row++)
                {
                    Color currentColor = textureColors[row * texture.Width + col];

                    if (currentColor == Color.White)
                    {
                        textureColors[row * texture.Width + col] *= 0.1f;
                        continue;
                    }
                    if (currentColor == Color.Black)
                    {
                        continue;
                    }

                    temp_buttons[currentColor] = new Button(spriteBatch, null, new Vector2(col, row), Vector2.One, Color.LightPink, CallUnwear);
                }
            }

            foreach (Button button in temp_buttons.Values)
                base.AddButton(button);

            clothingButtons[ClothingType.head] = new EquipmentType(null, temp_buttons[head]);
            clothingButtons[ClothingType.top] = new EquipmentType(null, temp_buttons[top]);
            clothingButtons[ClothingType.bottom] = new EquipmentType(null, temp_buttons[bottom]);
            clothingButtons[ClothingType.shoe] = new EquipmentType(null, temp_buttons[shoe]);
            clothingButtons[ClothingType.weapon] = new EquipmentType(null, temp_buttons[weapon]);

            texture.SetData<Color>(textureColors);

        }
        #endregion

        #region Draw functions
        public override void DrawObject(Vector2 windowScale)
        {
            base.DrawObject(windowScale);

            this.Draw_Hover_Details();
        }
        private void Draw_Hover_Details()
        {
            Vector2 mouse_pos;
            Rectangle destinationRectangle;

            foreach (EquipmentType eq in clothingButtons.Values)
            {
                if (eq.button.isHovered &&
                    eq.clothing != null)
                {
                    mouse_pos = Mouse.GetState().Vector();
                    destinationRectangle = new Rectangle(
                        (int)(mouse_pos.X),
                        (int)(mouse_pos.Y),
                        100,
                        100);

                    spriteBatch.Draw(Service.pixel, destinationRectangle, Color.Gray);

                    spriteBatch.DrawString(font, eq.clothing.folder.ToString(), mouse_pos + new Vector2(10, 10), Color.Red);
                    spriteBatch.DrawString(font, "Level: " + eq.clothing.minLevel.ToString(), mouse_pos + new Vector2(10, 30), Color.White);
                    spriteBatch.DrawString(font, "Defence: " + eq.clothing.defence.ToString(), mouse_pos + new Vector2(10, 50), Color.White);
                    spriteBatch.DrawString(font, "Strength: " + eq.clothing.strength.ToString(), mouse_pos + new Vector2(10, 70), Color.White);
                }
            }
        }
        #endregion

        #region Private functions
        private void CallUnwear()
        {
            foreach (EquipmentType equipment in clothingButtons.Values)
            {
                if (equipment.button.isClicked)
                {
                    equipment.button.ChangeAppearance(null);
                    unwearAction(equipment.clothing);
                }
            }
        }
        #endregion

        #region Public functions
        public void SetClothing(Dictionary<ClothingType, Clothing> equipment)
        {
            foreach (ClothingType clothingType in equipment.Keys)
                clothingButtons[clothingType].SetClothing(equipment[clothingType]);
        }
        #endregion
    }
}
