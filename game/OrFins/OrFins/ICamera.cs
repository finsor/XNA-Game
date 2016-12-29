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
    interface ICamera
    {
        Matrix mat { get; }
        IFocus focus { get; }
        Vector2 zoom { get; }
        Viewport view { get; }
        Vector2 position { get; }
    }
}
