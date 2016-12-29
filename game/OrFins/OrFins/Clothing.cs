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
    enum ClothingType
    {
        head,
        bottom,
        top,
        shoe,
        weapon
    }

    class Clothing : Sprite
    {
        #region Data
        public Folders folder { get; private set; }
        public States state { get; private set; }
        public ClothingType clothingType { get; private set; }
        public int minLevel { get; private set; }
        public int defence { get; private set; }
        public int strength { get; private set; }

        private ImageProcessor page;
        private int index;
        #endregion

        #region Properties
        public ImageProcessor EquipmentPage
        {
            get
            {
                return (SpritesDictionary.dictionary[folder][States.equipment]);
            }
        }
        #endregion

        #region Construction
        public Clothing(Folders folder, SpriteBatch spriteBatch, ClothingType clothingType, int minLevel, int defence, int strength)
            : base(spriteBatch)
        {
            this.folder = folder;
            this.clothingType = clothingType;
            this.minLevel = minLevel;
            this.defence = defence;
            this.strength = strength;
        }
        #endregion

        #region Update functions
        public void Update(Player player)
        {
            base.isDrawable = player.isDrawable;
            base.position = player.position;
            base.scale = player.scale;
            base.IsLookingLeft = player.IsLookingLeft;
            base.color = player.color;

            this.state = player.state;

            this.index = player.index;
            this.page = SpritesDictionary.dictionary[folder][state];
            base.texture = page.texture;
            base.sourceRectangle = page.rectangles[index];
            base.origin = page.origins[index];

            if (!IsLookingLeft)
            {
                origin = new Vector2(sourceRectangle.Value.Width - origin.X, origin.Y);
            }

            base.Update();

        }
        #endregion

        #region Online functions
        public void SendToServer(OnlineClient onlineClient)
        {
            // Send drawing details of clothing
            onlineClient.Send(
                this.folder.ToString(),
                this.state.ToString(),
                this.index.ToString(),
                base.position.X.ToString(),
                base.position.Y.ToString(),
                base.effects.ToString());
        }
        #endregion
    }
}
