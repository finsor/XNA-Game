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
using MB = System.Windows.Forms.MessageBox;

namespace OrFins
{
    enum MovementCode
    {
        Static,
        Dependent,
        Linear
    }

    class BackGroundObject : Sprite
    {
        #region Data
        private int depth;
        private Vector2 originalPosition;
        private MovementCode movementCode;
        #endregion

        #region Construction
        public BackGroundObject(Folders folder, States state, SpriteBatch spriteBatch, Vector2 originalPosition, Vector2 scale, MovementCode movementCode, int depth = 1)
            : base(folder, state, spriteBatch, originalPosition, Color.White, 0f, scale, SpriteEffects.None, 0)
        {
            this.originalPosition = originalPosition;
            this.depth = depth;
            this.movementCode = movementCode;
        }

        public void InitializeScale(float mapWidth, float mapHeight)
        {
            this.scale *= new Vector2(
                mapWidth / texture.Width,
                mapHeight / texture.Height);
        }
        #endregion

        #region Draw functions
        public override void DrawObject(Vector2 windowScale)
        {
            switch (movementCode)
            {
                case MovementCode.Linear:

                    Vector2 movement = Vector2.UnitX * (float)Math.Truncate(rightmostPixel - position.X);

                    position -= movement;
                    base.DrawObject(windowScale);
                    position += 2 * movement;
                    base.DrawObject(windowScale);
                    position -= movement;
                    base.DrawObject(windowScale);

                    break;

                default:

                    base.DrawObject(windowScale);

                    break;
            }
        }
        #endregion

        #region Update functions
        public void Update(Camera camera, Map map)
        {
            switch (movementCode)
            {
                case MovementCode.Dependent:

                    this.position = originalPosition + (camera.position - originalPosition) / depth;

                    break;

                case MovementCode.Linear:

                    this.position -= Vector2.UnitX * 0.3f;
                    if (base.rightmostPixel < map.leftLimit)
                    {
                        this.position = new Vector2(map.rightLimit, this.position.Y);
                    }

                    break;
            }

            base.Update();
        }
        #endregion
    }
}
