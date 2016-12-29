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
    class ClothingData
    {
        #region Data
        private SpriteBatch spriteBatch;
        private Folders folder;
        private ClothingType clothingType;
        private int minLevel;
        private int defence;
        private int strength;
        private int selling_price;
        private int sell_back_price;
        #endregion

        #region Construction
        public ClothingData(SpriteBatch spriteBatch, Folders folder, ClothingType clothingType, int minLevel, int defence, int strength, int selling_price, int sell_back_price)
        {
            this.spriteBatch = spriteBatch;
            this.folder = folder;
            this.clothingType = clothingType;
            this.minLevel = minLevel;
            this.defence = defence;
            this.strength = strength;
            this.selling_price = selling_price;
            this.sell_back_price = sell_back_price;
        }
        #endregion

        #region Public functions
        public Clothing GetClothing()
        {
            return (new Clothing(folder, spriteBatch, clothingType, minLevel, defence, strength));
        }
        public ImageProcessor GetPage()
        {
            return (SpritesDictionary.dictionary[folder][States.equipment]);
        }
        public int GetSellingPrice()
        {
            return (this.selling_price);
        }
        public int GetSellBack()
        {
            return (this.sell_back_price);
        }
        public int GetDefence()
        {
            return (this.defence);
        }
        public int GetStrength()
        {
            return (this.strength);
        }
        public Folders GetFolder()
        {
            return (this.folder);
        }
        public int GetMinLevel()
        {
            return (minLevel);
        }
        #endregion
    }
}
