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
    enum PortalPositions
    {
        LEFT,
        MIDDLE,
        RIGHT
    }
    class Portal : Animation
    {
        public Map destination;

        public Portal(Folders folder, SpriteBatch spriteBatch, Platform platform, PortalPositions portalPosition,
                            Color color, Vector2 scale, float layerDepth, int slowRate,
                            Map destination)
                : base(folder, spriteBatch, platform.CreatePositionUsingPortalPosition(portalPosition), color, scale, SpriteEffects.None, slowRate)
        {
            this.destination = destination;
        }
    }
}
