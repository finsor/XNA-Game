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
    class ShopType
    {
        #region Data
        private ClothingData data;
        public Button button { get; private set; }
        #endregion

        #region Construction
        public ShopType(ClothingData data, Button button)
        {
            this.data = data;
            this.button = button;
        }
        #endregion

        #region Public functions
        public bool ContainsClothing()
        {
            return (this.data != null);
        }

        public void ChangeItem(ClothingData data)
        {
            this.data = data;

            if (data == null)
            {
                this.button.ChangeAppearance(null);
            }
            else
            {
                this.button.ChangeAppearance(data.GetPage());
            }
        }
        public int GetSellingPrice()
        {
            if (data != null)
            {
                return (data.GetSellingPrice());
            }
            else
            {
                return 0;
            }
        }
        public int GetSellBackPrice()
        {
            if (data != null)
            {
                return (data.GetSellBack());
            }
            else
            {
                return 0;
            }
        }
        public Clothing GetClothing()
        {
            if (data != null)
            {
                return (data.GetClothing());
            }
            else
            {
                return null;
            }
        }
        public int GetStrength()
        {
            if (data != null)
            {
                return (data.GetStrength());
            }
            else
            {
                return 0;
            }
        }
        public string GetName()
        {
            if (data != null)
            {
                return (data.GetFolder().ToString());
            }
            else
            {
                return "";
            }
        }
        public int GetDefence()
        {
            if (data != null)
            {
                return (data.GetDefence());
            }
            else
            {
                return 0;
            }
        }
        public int GetMinLevel()
        {
            if (data != null)
            {
                return (data.GetMinLevel());
            }
            else
            {
                return 0;
            }
        }
        #endregion
    }
}
