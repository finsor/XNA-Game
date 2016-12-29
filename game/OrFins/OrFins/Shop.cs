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

namespace OrFins
{
    class Shop : DialogBox
    {
        #region Data
        private Player player;

        private Vector2[] textRelativePositions;

        private ShopType[] itemsToSell;
        private int indexOfFirstShopItemInList;
        private ClothingType currentClothingType;

        private ShopType[] itemsToSellBack;
        private int itemsToSellBackIndex;

        private TextSprite money;
        #endregion

        #region Construction
        public Shop(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Vector2 scale, SpriteFont font, Player player)
            : base(spriteBatch, texture, position, scale, font)
        {
            this.player = player;

            this.itemsToSell = new ShopType[5];
            this.itemsToSellBack= new ShopType[5];
            itemsToSellBackIndex = 0;

            this.textRelativePositions = new Vector2[45];

            ProcessTextureAndCreateButtons();
        }

        private void ProcessTextureAndCreateButtons()
        {
            Color[] textureColors = new Color[texture.Height * texture.Width];
            texture.GetData<Color>(textureColors);

            int item_to_sell_index = 0;
            Color item_to_sell_color = textureColors[0];

            int item_to_sell_back_index = 0;
            Color item_to_sell_back_color = textureColors[7];

            Dictionary<Color, Action> ColorToLoad = new Dictionary<Color, Action>();
            ColorToLoad.Add(textureColors[1], Load_Hats);
            ColorToLoad.Add(textureColors[2], Load_Tops);
            ColorToLoad.Add(textureColors[3], Load_Bots);
            ColorToLoad.Add(textureColors[4], Load_Shos);
            ColorToLoad.Add(textureColors[5], Load_Weps);

            Dictionary<Color, Action> ColorToPrevNext = new Dictionary<Color, Action>();
            ColorToPrevNext.Add(textureColors[8], Shop_Prev_Page);
            ColorToPrevNext.Add(textureColors[9], Shop_Next_Page);
            ColorToPrevNext.Add(textureColors[10], Player_Prev_Page);
            ColorToPrevNext.Add(textureColors[11], Player_Next_Page);

            Dictionary<Color, string> ColorToText = new Dictionary<Color, string>();
            ColorToText.Add(textureColors[1], "Hats");
            ColorToText.Add(textureColors[2], "Tops");
            ColorToText.Add(textureColors[3], "Bots");
            ColorToText.Add(textureColors[4], "Shoe");
            ColorToText.Add(textureColors[5], "Weps");
            ColorToText.Add(textureColors[8], "<");
            ColorToText.Add(textureColors[9], ">");
            ColorToText.Add(textureColors[10], "<");
            ColorToText.Add(textureColors[11], ">");

            Color text_pos_color = textureColors[6];
            int text_pos_index = 0;

            Color player_money_color = textureColors[12];

            for (int col = 13; col < texture.Width; col++)
            {
                for (int row = 0; row < texture.Height; row++)
                {
                    Color currentColor = textureColors[row * texture.Width + col];

                    if (currentColor == Color.White)
                    {
                        continue;
                    }

                    if (currentColor == item_to_sell_color)
                    {
                        itemsToSell[item_to_sell_index] = new ShopType(null, new Button(spriteBatch, null, new Vector2(col, row), this.scale, Color.White, SellItem));

                        item_to_sell_index++;
                    }

                    if (currentColor == item_to_sell_back_color)
                    {
                        itemsToSellBack[item_to_sell_back_index] = new ShopType(null, new Button(spriteBatch, null, new Vector2(col, row), this.scale, Color.White, SellBack));

                        item_to_sell_back_index++;
                    }

                    if (ColorToLoad.ContainsKey(currentColor))
                    {
                        base.AddButton(new Button(spriteBatch, Folders.regularButton, new Vector2(col, row), ColorToText[currentColor], font, new Vector2(0.5f, 0.7f), Color.White, ColorToLoad[currentColor]));
                    }

                    if (ColorToPrevNext.ContainsKey(currentColor))
                    {
                        base.AddButton(new Button(spriteBatch, Folders.regularButton, new Vector2(col, row), ColorToText[currentColor], font, Vector2.One * 0.5f, Color.White, ColorToPrevNext[currentColor]));
                    }

                    if (currentColor == text_pos_color)
                    {
                        this.textRelativePositions[text_pos_index] = new Vector2(col, row);
                        text_pos_index++;
                    }

                    if (currentColor == player_money_color)
                    {
                        money = new TextSprite(spriteBatch, new Vector2(col, row), Color.Black, "", font);
                    }
                }
            }

            texture.SetData<Color>(textureColors);

            foreach (ShopType shopType in itemsToSell)
            {
                base.AddButton(shopType.button);
            }

            foreach (ShopType shopType in itemsToSellBack)
            {
                base.AddButton(shopType.button);
            }
        }
        #endregion

        #region Draw functions
        public override void DrawObject(Vector2 windowScale)
        {
            base.DrawObject(windowScale);

            this.DrawItems(windowScale);

            this.money.DrawObject(windowScale);
        }
        private void DrawItems(Vector2 windowScale)
        {
            ShopType item;
            for (int i = 0; i < itemsToSell.Length; i++)
            {
                item = itemsToSell[i];

                if (item.ContainsClothing())
                {
                    spriteBatch.DrawString(font, item.GetName(), (this.position + textRelativePositions[i]) * windowScale, Color.Red, 0F, Vector2.Zero, windowScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Min.Level: " + item.GetMinLevel(), (this.position + textRelativePositions[i + 10]) * windowScale, Color.Red, 0F, Vector2.Zero, windowScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Cost: " + item.GetSellingPrice(), (this.position + textRelativePositions[i + 20]) * windowScale, Color.Red, 0F, Vector2.Zero, windowScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Strength: " + item.GetStrength(), (this.position + textRelativePositions[i + 5]) * windowScale, Color.Red, 0F, Vector2.Zero, windowScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Defence : " + item.GetDefence(), (this.position + textRelativePositions[i + 15]) * windowScale, Color.Red, 0F, Vector2.Zero, windowScale, SpriteEffects.None, 0);
                }
            }

            for (int i = 0; i < itemsToSellBack.Length; i++)
            {
                item = itemsToSellBack[i];

                if (item.ContainsClothing())
                {
                    spriteBatch.DrawString(font, item.GetName(), (this.position + textRelativePositions[i + 25]) * windowScale, Color.Red, 0F, Vector2.Zero, windowScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Strength: " + item.GetStrength(), (this.position + textRelativePositions[i + 30]) * windowScale, Color.Red, 0F, Vector2.Zero, windowScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Defence: " + item.GetDefence(), (this.position + textRelativePositions[i + 35]) * windowScale, Color.Red, 0F, Vector2.Zero, windowScale, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Sell back: " + item.GetSellBackPrice(), (this.position + textRelativePositions[i + 40]) * windowScale, Color.Red, 0F, Vector2.Zero, windowScale, SpriteEffects.None, 0);
                }
            }
        }
        #endregion

        #region Update functions
        public override void Update(MouseState previousMouseState, MouseState currentMouseState, Vector2 windowScale)
        {
            base.Update(previousMouseState, currentMouseState, windowScale);

            money.Update(this.position);
            money.Update(player.money.ToString());
        }
        #endregion

        #region Shop tabs
        private void Load_Hats()
        {
            indexOfFirstShopItemInList = 0;
            currentClothingType = ClothingType.head;
            Reload_Items_To_Sell(ClothingType.head);
        }
        private void Load_Tops()
        {
            indexOfFirstShopItemInList = 0;
            currentClothingType = ClothingType.top;
            Reload_Items_To_Sell(ClothingType.top);
        }
        private void Load_Bots()
        {
            indexOfFirstShopItemInList = 0;
            currentClothingType = ClothingType.bottom;
            Reload_Items_To_Sell(ClothingType.bottom);
        }
        private void Load_Shos()
        {
            indexOfFirstShopItemInList = 0;
            currentClothingType = ClothingType.shoe;
            Reload_Items_To_Sell(ClothingType.shoe);
        }
        private void Load_Weps()
        {
            indexOfFirstShopItemInList = 0;
            currentClothingType = ClothingType.weapon;
            Reload_Items_To_Sell(ClothingType.weapon);
        }
        private void Reload_Items_To_Sell(ClothingType clothingType)
        {
            List<ClothingData> list = ClothingDictionary.dictionary[clothingType];

            indexOfFirstShopItemInList = (int)MathHelper.Clamp(indexOfFirstShopItemInList, 0, list.Count - 5);

            int index_in_list, index_in_shop;

            for (index_in_list = indexOfFirstShopItemInList, index_in_shop = 0;
                index_in_list < list.Count && index_in_shop < 5;
                ++index_in_list, ++index_in_shop)
            {
                itemsToSell[index_in_shop].ChangeItem(list[index_in_list]);
            }

            while (index_in_shop < 5)
            {
                itemsToSell[index_in_shop].ChangeItem(null);

                index_in_shop++;
            }
        }
        private void Shop_Next_Page()
        {
            indexOfFirstShopItemInList++;
            Reload_Items_To_Sell(currentClothingType);
        }
        private void Shop_Prev_Page()
        {
            indexOfFirstShopItemInList--;
            Reload_Items_To_Sell(currentClothingType);
        }
        #endregion

        #region Sellback tabs
        public void Reload_Player_Bag()
        {

            ClothingData clothingData;
            Clothing clothing;
            List<Clothing> bag_without_nulls = new List<Clothing>();

            foreach (Clothing ccloting in player.bag)
            {
                if (ccloting != null)
                    bag_without_nulls.Add(ccloting);
            }

            int shop_index;
            int bag_index;

            itemsToSellBackIndex = (int)MathHelper.Clamp(itemsToSellBackIndex, 0, bag_without_nulls.Count - 5);

            for (shop_index = 0, bag_index = itemsToSellBackIndex;
                shop_index < 5 && bag_index < bag_without_nulls.Count;
                shop_index++, bag_index++)
            {
                clothingData = null;
                clothing = bag_without_nulls[bag_index];

                if (clothing != null)
                {
                    clothingData = ClothingDictionary.dictionary[clothing.clothingType].Find(item => item.GetFolder() == clothing.folder);
                }
                itemsToSellBack[shop_index].ChangeItem(clothingData);
            }

            while (shop_index < 5)
            {
                itemsToSellBack[shop_index].ChangeItem(null);
                shop_index++;
            }
        }
        private void Player_Next_Page()
        {
            itemsToSellBackIndex++;
            Reload_Player_Bag();
        }
        private void Player_Prev_Page()
        {
            itemsToSellBackIndex--;
            Reload_Player_Bag();
        }
        #endregion

        public void SellItem()
        {
            foreach (ShopType shop_item in itemsToSell)
            {
                if (shop_item.button.isClicked)
                {
                    player.BuyFromShop(shop_item.GetClothing(), shop_item.GetSellingPrice());
                    Reload_Player_Bag();
                }
            }
        }
        public void SellBack()
        {
            foreach (ShopType shop_item in itemsToSellBack)
            {
                if (shop_item.button.isClicked)
                {
                    player.SellBack(shop_item.GetClothing(), shop_item.GetSellBackPrice());
                    Reload_Player_Bag();
                }
            }
        }
    }
}
