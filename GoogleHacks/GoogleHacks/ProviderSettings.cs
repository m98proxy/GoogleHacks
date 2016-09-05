/* Copyright © 2016
 * Author: Gerallt Franke */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Google
{
    public class ProviderSettings
    {
        /// <summary>
        /// The Google Application API Key.
        /// If left blank, further requests and IP address can get blocked by Google.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The Width of the Streetview tile image. Default: 640
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The Size of the Streetview tile image. Default 640x480
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return new Vector2(Width, Height);
            }
            set
            {
                this.Width = (int)value.X;
                this.Height = (int)value.Y;
            }
        }

        /// <summary>
        /// The Width of the Streetview image. Default: 480
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The bounding volume of the virtual world.
        /// </summary>
        public Vector3? WorldSize { get; set; }

        public ProviderSettings()
        {
            Width = 640;
            Height = 480;
            ApiKey = string.Empty;
        }
    }
}
