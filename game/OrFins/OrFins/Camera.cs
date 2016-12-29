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
    class Camera : ICamera
    {
        #region DATA
        public Matrix mat { get; private set; }
        public IFocus focus { get; private set; }
        public Vector2 zoom { get; private set; }
        public Viewport view { get; private set; }
        public Vector2 position { get; private set; }
        public Vector2 originalPosition { get; private set; }
        #endregion

        #region Construction
        public Camera(IFocus focus, Vector2 zoom, Viewport view)
        {
            this.focus = focus;
            this.zoom = zoom;
            this.view = view;
            this.originalPosition = new Vector2(view.Width / 2, view.Height / 2);
            this.position = originalPosition;
        }
        #endregion

        #region Update functions
        public void UpdateMat(Map map, Vector2 windowScale)
        {
            Vector3 minus = new Vector3(-position * windowScale, 0);
            Vector3 plus = new Vector3(originalPosition * windowScale, 0);

            mat =
                Matrix.CreateTranslation(minus) *
                Matrix.CreateTranslation(plus);

            position = Vector2.Lerp(Vector2.Clamp(focus.position, map.TopLeftCorner + originalPosition, map.BottomRightCorner - originalPosition), position, 0.9f);
        }
        #endregion
    }
}
