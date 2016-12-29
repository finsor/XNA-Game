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
    class UserKeys : BaseKeys
    {
        private KeyboardState keyboardState;
        private Keys up, down, right, left, jump, attack, skill, rope, pickup;

        #region Construction
        public UserKeys(Keys up, Keys down, Keys right, Keys left, Keys jump, Keys attack, Keys skill, Keys rope, Keys pickup)
        {
            this.up = up;
            this.down = down;
            this.right = right;
            this.left = left;
            this.jump = jump;
            this.attack = attack;
            this.skill = skill;
            this.rope = rope;
            this.pickup = pickup;
        }
        #endregion

        #region Key functions
        public override bool UpKey()
        {
            return keyboardState.IsKeyDown(up);
        }

        public override bool DownKey()
        {
            return keyboardState.IsKeyDown(down);
        }

        public override bool RightKey()
        {
            return keyboardState.IsKeyDown(right);
        }

        public override bool LeftKey()
        {
            return keyboardState.IsKeyDown(left);
        }

        public override bool JumpKey()
        {
            return keyboardState.IsKeyDown(jump);
        }

        public override bool AttackKey()
        {
            return keyboardState.IsKeyDown(attack);
        }

        public override bool SkillKey()
        {
            return keyboardState.IsKeyDown(skill);
        }

        public override bool RopeKey()
        {
            return keyboardState.IsKeyDown(rope);
        }

        public override bool PickupKey()
        {
            return keyboardState.IsKeyDown(pickup);
        }
        #endregion

        public override void Update()
        {
            keyboardState = Keyboard.GetState();
        }
    }
}
