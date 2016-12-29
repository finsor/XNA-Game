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
    class ImageProcessor
    {
        #region DATA
        public List<Circle> headCircles { get; private set; }
        public List<Circle> coreCircles { get; private set; }
        public List<Circle> legsCircles { get; private set; }

        public List<Rectangle> rectangles { get; private set; }
        public List<Vector2> origins { get; private set; }
        public Texture2D texture { get; private set; }
        #endregion

        #region Construction
        public ImageProcessor(ContentManager contentManager, string path, GraphicsDevice graphicsDevice)
        {
            rectangles = new List<Rectangle>();
            origins = new List<Vector2>();

            // Create the sourceRectangles list and origins lists
            // Also remove texture's background
            CreatePage(contentManager, path);
            RemoveBackground();

            // If creating circles is possible (texture exists), then create circles.
            if (File.Exists("Content/" + path + "mask.xnb"))
                CreateCircles(contentManager, path + "mask", graphicsDevice);
        }

        // Function creates and initializes origins list and sourceRectangles list
        private void CreatePage(ContentManager contentManager, string path)
        {
            texture = contentManager.Load<Texture2D>(path);
            Color[] colorArray = new Color[texture.Width];

            texture.GetData<Color>(0, new Rectangle(0, texture.Height - 1, texture.Width, 1), colorArray, 0, texture.Width);

            List<int> pnt = new List<int>();
            for (int i = 0; i < colorArray.Length; i++)
            {
                if (colorArray[i] != colorArray[1])
                    pnt.Add(i);
            }

            for (int i = 1; i < pnt.Count; i += 2)
            {
                origins.Add(new Vector2(pnt[i] - pnt[i - 1], texture.Height - 1));
                rectangles.Add(new Rectangle(pnt[i - 1] + 1, 0, pnt[i + 1] - pnt[i - 1] - 2, texture.Height - 1));
            }
        }

        // Function removes texture's background using its first pixel.
        private void RemoveBackground()
        {
            Color[] colors = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(colors);

            Color backgroundColor = colors[0];

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].Equals(backgroundColor))
                    colors[i] = Color.Transparent;
            }

            texture.SetData<Color>(colors);
        }

        // Function creates circles using texture + "mask".
        private void CreateCircles(ContentManager contentManager, string path, GraphicsDevice graphicsDevice)
        {
            Texture2D maskTex = contentManager.Load<Texture2D>(path);
            Color[] maskColor = new Color[maskTex.Width * maskTex.Height];

            maskTex.GetData<Color>(maskColor);

            // The dictionary is used to reach the wanted list using its color
            Dictionary<Color, List<Circle>> colorToList_Dictionary = new Dictionary<Color, List<Circle>>();

            Color headColor = maskColor[0];
            Color coreColor = maskColor[1];
            Color legColor = maskColor[2];

            headCircles = new List<Circle>();
            coreCircles = new List<Circle>();
            legsCircles = new List<Circle>();

            colorToList_Dictionary.Add(headColor, headCircles);
            colorToList_Dictionary.Add(coreColor, coreCircles);
            colorToList_Dictionary.Add(legColor, legsCircles);

            int row;
            int column;
            int relativeColumn;
            Vector2 relativePosition;
            float radius;
            Circle circle;
            Color currentColor;
            Rectangle rectangle;

            // Scan the texture, create circles and add them to the list
            for (column = 3; column < maskTex.Width; column++)
            {
                // Save current color in a variable for easier approach
                currentColor = maskColor[column];

                // If current color is not a wanted color then escape
                if (currentColor == Color.White || currentColor == Color.Black)
                    continue;

                // Create relative position
                for (row = 1; row < maskTex.Height - 1 && maskColor[(row + 1) * maskTex.Width + column] != Color.Black && maskColor[(row + 1) * maskTex.Width + column] != Color.White; row++) ;
                rectangle = rectangles[colorToList_Dictionary[currentColor].Count];
                relativePosition = new Vector2(column - rectangle.X, row);

                // Create radius
                for (relativeColumn = column; relativeColumn < maskTex.Width - 1 && maskColor[row * maskTex.Width + relativeColumn + 1] != Color.Black && maskColor[row * maskTex.Width + relativeColumn + 1] != Color.White; relativeColumn++) ;
                radius = relativeColumn - column;

                // Create the circle
                circle = new Circle(relativePosition, radius, graphicsDevice);
                
                // Add the circle to the currect list using the dictionary
                colorToList_Dictionary[currentColor].Add(circle);
            }
        }
        #endregion
    }
}
