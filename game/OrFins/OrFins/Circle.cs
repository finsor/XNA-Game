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
    class Circle
    {
        private Vector2 position;
        private float radius;
        private Texture2D texture;

        #region Construction
        public Circle(Vector2 relativePosition, float radius, GraphicsDevice graphicsDevice)
        {
            this.position = relativePosition;
            this.radius = radius;
            this.texture = CreateCircle(graphicsDevice, (int)radius);
        }
        public Circle(Vector2 actualPosition, float radius, Texture2D texture)
        {
            this.position = actualPosition;
            this.radius = radius;
            this.texture = texture;
        }

        private Texture2D CreateCircle(GraphicsDevice graphicsDevice, int radius)
        {
            int diameter = radius * 2 + 2;
            Texture2D texture = new Texture2D(graphicsDevice, diameter, diameter);

            Color[] data = new Color[diameter * diameter];

            double angleStep = 1f / radius;

            for (double angle = 0; angle < Math.PI * 2; angle += angleStep)
            {
                int x = (int)Math.Round(radius + radius * Math.Cos(angle));
                int y = (int)Math.Round(radius + radius * Math.Sin(angle));

                data[y * diameter + x + 1] = Color.White;
            }

            texture.SetData(data);

            return texture;
        }
        #endregion

        #region Draw functions
        public void DrawObject(SpriteBatch spriteBatch, Vector2 windowScale)
        {
            spriteBatch.Draw(texture, position * windowScale, null, Color.Red, 0f, new Vector2(texture.Width / 2, texture.Height / 2), windowScale, SpriteEffects.None, 0);
        }
        #endregion

        #region Public functions
        public bool DoesCollide(Circle anotherCircle)
        {
            float distance = Vector2.Distance(this.position, anotherCircle.position);
            return (distance <= this.radius + anotherCircle.radius);
        }
        public void SendProfile(OnlineClient onlineClient)
        {
            onlineClient.Send(
                this.position.X.ToString(),
                this.position.Y.ToString(),
                this.radius.ToString());
        }
        public Circle GetActualCircle(Sprite sprite)
        {
            Vector2 actualPosition;

            Rectangle rectangle = (Rectangle)sprite.sourceRectangle;
            Vector2 origin = sprite.origin;

            actualPosition = sprite.position + this.position - origin;

            if (sprite.effects == SpriteEffects.FlipHorizontally)
            {
                actualPosition += Vector2.UnitX * (rectangle.Width - 2 * this.position.X);
            }

            return (new Circle(actualPosition, radius, texture));
        }
        #endregion
    }
}
