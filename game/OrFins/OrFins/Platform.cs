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
    class Platform : Sprite
    {
        #region DATA
        public Vector2 leftmostPart { get; private set; }
        public Vector2 rightmostPart { get; private set; }
        public Vector2 middlePart { get; private set; }
        private int[] terrainContour;
        #endregion

        #region Construction
        public Platform(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Color color, SpriteEffects effects, float layerDepth)
            : base(spriteBatch, texture, position + new Vector2(texture.Width / 2, 0), texture.Bounds, color, 0f, new Vector2(texture.Width / 2, texture.Height - 1), Vector2.One, effects, layerDepth)
        {
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

        #region Drawing functions
        public void DrawEdges(Vector2 windowScale)
        {
            Rectangle rectangle;

            rectangle = new Rectangle((int)leftmostPart.X - 2, (int)leftmostPart.Y - 2, 4, 4);
            spriteBatch.Draw(Service.pixel, rectangle.Multiply(windowScale), Color.Red);

            rectangle = new Rectangle((int)rightmostPart.X - 2, (int)rightmostPart.Y - 2, 4, 4);
            spriteBatch.Draw(Service.pixel, rectangle.Multiply(windowScale), Color.Red);

            rectangle = new Rectangle((int)middlePart.X - 2, (int)middlePart.Y - 2, 4, 4);
            spriteBatch.Draw(Service.pixel, rectangle.Multiply(windowScale), Color.Red);
        }
        #endregion

        #region Public functions
        public Vector2 GetRandomStartingPosition()
        {
            int offset = Service.Random(1, (int)Width - 1);

            float final_x = leftmostPixel + offset;
            float final_y = terrainContour[offset];

            return (new Vector2(final_x, final_y));
        }
        public Vector2 CreatePositionUsingOffset(int offset)
        {
            float final_x = leftmostPixel + offset;
            float final_y = (float)terrainContour[(int)offset];

            return (new Vector2(final_x, final_y));
        }
        public Vector2 CreatePositionUsingPortalPosition(PortalPositions portalPosition)
        {
            float offset = 0;

            switch (portalPosition)
            {
                case PortalPositions.LEFT:

                    offset = 50;

                    break;

                case PortalPositions.MIDDLE:

                    offset = Width / 2;

                    break;

                case PortalPositions.RIGHT:

                    offset = Width - 50;

                    break;
            }

            float final_x = leftmostPixel + offset;
            float final_y = (float)terrainContour[(int)offset];

            return (new Vector2(final_x, final_y));
        }
        #endregion
    }
}
