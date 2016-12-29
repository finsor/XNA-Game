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
    interface IFocus
    {
        SpriteBatch spriteBatch { get;}
        Texture2D texture { get; }
        Rectangle? sourceRectangle { get; }
        Vector2 position { get; }
        Color color { get; }
        Vector2 origin { get; }
        float rotation { get; }
        Vector2 scale { get; }
        SpriteEffects effects { get; }
        float layerDepth { get; }

        void DrawObject(Vector2 windowScale);
        void Update();
    }
}
