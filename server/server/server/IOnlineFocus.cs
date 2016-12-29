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
    interface IOnlineFocus
    {
        Texture2D texture { get; }
        Rectangle? sourceRectangle { get; }
        Vector2 position { get; }
        Vector2 origin { get; }
        SpriteEffects effects { get; }

        void Update();
    }
}
