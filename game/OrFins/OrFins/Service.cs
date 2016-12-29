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
    static class Service
    {
        #region Data
        private static Random random = new Random();
        public static int screenHeight { get; private set; }
        public static int screenWidth { get; private set; }
        public static Texture2D rectangleTexture { get; private set; }
        public static Texture2D pixel { get; private set; }
        #endregion

        public static void Initialize(Texture2D rectangleTexture, Texture2D pixel, int screenHeight, int screenWidth)
        {
            Service.rectangleTexture = rectangleTexture;
            Service.pixel = pixel;
            Service.screenHeight = screenHeight;
            Service.screenWidth = screenWidth;
            Service.random = new Random();
        }

        public static int Random(int min, int max)
        {
            return random.Next(min, max + 1);
        }
    }
}
