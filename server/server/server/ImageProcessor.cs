using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace server
{
    class ImageProcessor
    {

        #region DATA
        public List<OnlineCircle> headCircles { get; private set; }
        public List<OnlineCircle> coreCircles { get; private set; }
        public List<OnlineCircle> legsCircles { get; private set; }

        public List<Rectangle> rectangles { get; private set; }
        public List<Vector2> origins { get; private set; }
        public Texture2D texture { get; private set; }
        #endregion

        #region Construction
        public ImageProcessor(ContentManager contentManager, string path)
        {
            rectangles = new List<Rectangle>();
            origins = new List<Vector2>();

            CreatePage(contentManager, path);
            RemoveBackground();

            if (File.Exists("Content/" + path + "mask.xnb"))
                CreateCircles(contentManager, path + "mask");
        }

        private void CreatePage(ContentManager contentManager, string name)
        {
            texture = contentManager.Load<Texture2D>(name);
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
        private void RemoveBackground()
        {
            Color[] colorMat = new Color[texture.Width * texture.Height];

            texture.GetData<Color>(colorMat);

            Color changeToTransparent = colorMat[0];

            for (int i = 0; i < colorMat.Length; i++)
            {
                if (colorMat[i] == changeToTransparent)
                    colorMat[i] = Color.Transparent;
            }

            texture.SetData<Color>(colorMat);
        }
        private void CreateCircles(ContentManager contentManager, string path)
        {
            Texture2D maskTex = contentManager.Load<Texture2D>(path);
            Color[] maskColor = new Color[maskTex.Width * maskTex.Height];

            maskTex.GetData<Color>(maskColor);


            // The dictionary is used to reach the wanted list using its color
            Dictionary<Color, List<OnlineCircle>> colorToList_Dictionary = new Dictionary<Color, List<OnlineCircle>>();

            Color headColor = maskColor[0];
            Color coreColor = maskColor[1];
            Color legColor = maskColor[2];

            headCircles = new List<OnlineCircle>();
            coreCircles = new List<OnlineCircle>();
            legsCircles = new List<OnlineCircle>();

            colorToList_Dictionary.Add(headColor, headCircles);
            colorToList_Dictionary.Add(coreColor, coreCircles);
            colorToList_Dictionary.Add(legColor, legsCircles);

            int row;
            int column;
            int relativeColumn;
            Vector2 relativePosition;
            float radius;
            OnlineCircle circle;
            Color currentColor;
            Rectangle rectangle;
            Vector2 origin;

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
                origin = origins[colorToList_Dictionary[currentColor].Count];
                relativePosition = new Vector2(column - rectangle.X, row);
                //relativePosition = -Vector2.UnitY * (texture.Height - row);

                // Create radius
                for (relativeColumn = column; relativeColumn < maskTex.Width - 1 && maskColor[row * maskTex.Width + relativeColumn + 1] != Color.Black && maskColor[row * maskTex.Width + relativeColumn + 1] != Color.White; relativeColumn++) ;
                radius = relativeColumn - column;

                // Create the circle
                circle = new OnlineCircle(relativePosition, radius);

                // Add the circle to the currect list using the dictionary
                colorToList_Dictionary[currentColor].Add(circle);
            }
        }
        #endregion
    }
}
