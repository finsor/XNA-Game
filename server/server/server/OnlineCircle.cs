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
    class OnlineCircle
    {
        private Vector2 position;
        public float radius;

        public OnlineCircle(Vector2 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }

        public OnlineCircle()
        {
        }

        public OnlineCircle GetActualCircle(OnlineSprite reference)
        {
            Vector2 actualPosition;
            Rectangle rectangle = (Rectangle)reference.sourceRectangle;
            Vector2 origin = reference.origin;

            actualPosition = reference.position + this.position - origin;

            if (reference.effects == SpriteEffects.FlipHorizontally)
            {
                actualPosition += Vector2.UnitX * (rectangle.Width - 2 * this.position.X);
            }

            return (new OnlineCircle(actualPosition, radius));
        }

        public void Update(Vector2 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }

        public bool DoesCollide(OnlineCircle anotherCircle)
        {
            float distance = Vector2.Distance(this.position, anotherCircle.position);
            return (distance <= this.radius + anotherCircle.radius);
        }
    }
}
