using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace server
{
    static class ExtensionMethods
    {
        public static MapRecord At(this MapRecord[,] mapMask, Vector2 position)
        {
            return mapMask[(int)position.X, (int)position.Y];
        }
    }
}
