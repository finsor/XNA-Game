using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace server
{
    class OnlinePlatform : OnlineSprite
    {
        #region Data
        public Vector2 leftmostPart { get; private set; }
        public Vector2 rightmostPart { get; private set; }
        public Vector2 middlePart { get; private set; }
        private Random random;
        private int[] terrainContour;
        #endregion

        #region Construction
        public OnlinePlatform(Texture2D texture, Vector2 position)
            :base(position, texture, texture.Bounds)
        {
            this.random = new Random();

            base.Update();

            RemoveBackground();
            CreateTerrainContour();
            CreateSpecialPositions();
        }

        private void RemoveBackground()
        {
            Color[] textureColors = new Color[texture.Height * texture.Width];
            texture.GetData<Color>(textureColors);

            Color colorToRemove = textureColors[0];

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    if (textureColors[y * texture.Width + x] == colorToRemove)
                    {
                        textureColors[y * texture.Width + x] = Color.Transparent;
                    }
                }
            }

            texture.SetData<Color>(textureColors);
        }
        private void CreateTerrainContour()
        {
            Color[] textureColors = new Color[texture.Height * texture.Width];
            texture.GetData<Color>(textureColors);

            terrainContour = new int[(int)Width];

            int y;

            for (int x = 0; x < Width; x++)
            {
                y = 0;

                while (textureColors[(int)(x + y * Width)] == Color.Transparent)
                {
                    y++;
                }

                terrainContour[x] = (int)(topPixel + y);
            }
        }
        private void CreateSpecialPositions()
        {
            leftmostPart = new Vector2(leftmostPixel, terrainContour[0]);
            middlePart = new Vector2(leftmostPixel + Width / 2, terrainContour[(int)(Width / 2)]);
            rightmostPart = new Vector2(rightmostPixel - 1, terrainContour[(int)Width - 1]);
        }
        #endregion

        #region Public functions
        public Vector2 GetRandomStartingPosition()
        {
            int offset = random.Next(10, (int)Width - 10);

            float final_x = leftmostPixel + offset;
            float final_y = terrainContour[offset];

            return (new Vector2(final_x, final_y));
        }
        #endregion
    }
}
