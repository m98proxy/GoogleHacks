/* Copyright © 2016
 * Author: Gerallt Franke */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using X3D;
using X3D.Engine;

namespace GoogleHacks
{
    /// <summary>
    /// Displays a mini map in a 2D square depth-mask-locked to the top left hand corner of the screen,
    /// showing nearest roads defined as LineSets; with points interpolated for smooth paths.
    /// </summary>
    public class Minimap
    {
        #region Public Properties

        /// <summary>
        /// Position of minimap in 3D space.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Size of minimap in 3D space.
        /// </summary>
        public Vector3 Size { get; set; }

        /// <summary>
        /// Avatar position relative to minimap. Range: (0.0 - 1.0)
        /// </summary>
        public Vector3 AvatarPosition { get; set; }

        #endregion

        #region Private Fields

        private Shape rectShape;
        private Rectangle2D rect = new Rectangle2D();

        #endregion

        #region Constructors

        public Minimap(Vector3 size, Vector3 position)
        {
            AvatarPosition = new Vector3(0.5f, 0.5f, 0.5f);

            this.Size = size;
            this.Position = position;
        }

        #endregion

        #region Rendering Methods

        public void Initilize(SceneCamera camera)
        {
            rectShape = new Shape(geometry: rect, transform: Transform.CreateTranslation(camera.Position + camera.Forward));
        }

        public void Render(RenderingContext rc)
        {
            rectShape.Draw(rc);
        }

        #endregion
    }
}