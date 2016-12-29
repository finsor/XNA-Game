using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrFins
{
    class EquipmentType
    {
        #region Data
        public Clothing clothing { get; private set; }
        public Button button { get; private set; }
        #endregion

        #region Construction
        public EquipmentType(Clothing clothing, Button button)
        {
            this.clothing = clothing;
            this.button = button;
        }
        #endregion

        #region Public functions
        public void SetClothing(Clothing clothing)
        {
            this.clothing = clothing;

            if (clothing == null)
            {
                button.ChangeAppearance(null);
            }
            else
            {
                this.button.ChangeAppearance(clothing.EquipmentPage);
            }
        }
        #endregion
    }
}
