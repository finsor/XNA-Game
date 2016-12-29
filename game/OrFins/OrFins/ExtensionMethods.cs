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
    static class ExtensionMethods
    {
        public static Vector2 Vector(this MouseState state)
        {
            return new Vector2(state.X, state.Y);
        }

        public static Point Point(this MouseState state)
        {
            return new Point(state.X, state.Y);
        }

        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        public static bool Contains(this Rectangle rec, Vector2 vector)
        {
            return rec.Contains(vector.ToPoint());
        }

        public static bool LeftPressed(this MouseState state)
        {
            return (state.LeftButton == ButtonState.Pressed);
        }

        public static Rectangle Multiply(this Rectangle rectangle, Vector2 vector)
        {
            return new Rectangle(
                (int)(rectangle.X * vector.X),
                (int)(rectangle.Y * vector.Y),
                (int)(rectangle.Width * vector.X),
                (int)(rectangle.Height * vector.Y));
        }

        public static MapRecord At(this MapRecord[,] mapMask, Vector2 position)
        {
            return (mapMask[(int)position.X, (int)position.Y]);
        }
    }
}
