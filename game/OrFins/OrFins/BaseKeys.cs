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
    abstract class BaseKeys
    {
        public abstract bool UpKey();
        public abstract bool DownKey();
        public abstract bool RightKey();
        public abstract bool LeftKey();
        public abstract bool JumpKey();
        public abstract bool AttackKey();
        public abstract bool SkillKey();
        public abstract bool RopeKey();
        public abstract bool PickupKey();

        public abstract void Update();
    }
}
