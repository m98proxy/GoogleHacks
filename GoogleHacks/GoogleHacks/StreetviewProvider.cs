/* Copyright © 2016
 * Author: Gerallt Franke */

using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X3D.Engine;
using OpenTK;
using X3D;

namespace Google
{
    /// <summary>
    /// Street view image provider
    /// </summary>
    public class StreetviewProvider
    {
        #region Public Properties

        public ProviderSettings Settings { get; set; }

        #endregion

        #region Constructors

        public StreetviewProvider(ProviderSettings settings)
        {
            this.Settings = settings;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fetch a view from Google Street View API using current camera frustum.
        /// </summary>
        /// <param name="camera">
        /// Current Scene Camera
        /// </param>
        /// <returns>
        /// A Bitmap image representing the current view camera frustum.
        /// </returns>
        public Bitmap FetchView(SceneCamera camera)
        {
            return FetchView(camera.Position, camera.Orientation);
        }

        /// <summary>
        /// Fetch a view from Google Street View API using current camera frustum.
        /// </summary>
        /// <param name="position">
        /// Current position in 3D space.
        /// </param>
        /// <param name="orientation">
        /// Current orientation in 3D space.
        /// </param>
        /// <returns>
        /// A Bitmap image representing the current view camera frustum.
        /// </returns>
        public Bitmap FetchView(Vector3 position, Quaternion orientation)
        {
            Vector3 orientationAngles;

            orientationAngles = QuaternionExtensions.ExtractPitchYawRoll(orientation);

            return FetchView(position, orientationAngles.X, orientationAngles.Y);
        }

        /// <summary>
        /// Fetch a view from Google Street View API using current camera frustum.
        /// </summary>
        /// <param name="position">
        /// Current position in 3D space.
        /// </param>
        /// <param name="orientation">
        /// Current orientation in 3D space (each component in radians.)
        /// (X: pitch, Y: yaw)
        /// </param>
        /// <returns>
        /// A Bitmap image representing the current view camera frustum.
        /// </returns>
        public Bitmap FetchView(Vector3 position, Vector2 orientation)
        {
            return FetchView(position, orientation.X, orientation.Y);
        }

        /// <summary>
        /// Fetch a view from Google Street View API using current camera frustum.
        /// </summary>
        /// <param name="position">
        /// Current position in 3D space.
        /// </param>
        /// <param name="pitch">
        /// Current camera pitch angle in radians.
        /// </param>
        /// <param name="yaw">
        /// Current camera yaw angle in radians.
        /// </param>
        /// <returns>
        /// A Bitmap image representing the current view camera frustum.
        /// </returns>
        public Bitmap FetchView(Vector3 position, float pitch, float yaw)
        {
            Bitmap image;
            StreetviewRequest request;
            StreetviewResponse response;
            Vector2 degrees;
            Vector2 uvEarthPosition;

            // Convert angles to degrees
            degrees = new Vector2((pitch / MathHelpers.PI2) * 360.0f, 
                                  (yaw / MathHelpers.PI2) * 360.0f);

            // Convert position to earth position
            uvEarthPosition = GoogleHacks.MathHelpers.CartesianToEarthUV(position, null);

            // Make street view request
            request = new StreetviewRequest()
            {
                Width = Settings.Width,
                Height = Settings.Height,
                ApiKey = Settings.ApiKey,
                Heading = degrees.Y,
                Pitch = degrees.X,
                Latitude = uvEarthPosition.X,
                Longitude = uvEarthPosition.Y
            };

            // Dispatch request to street view service
            response = DispatchRequest(request);

            image = response.Image;

            //var newSize = ImageTexture.GetTextureGLMaxSize(image);
            ImageTexture.Rescale(ref image, new Size(512, 512));

            image.RotateFlip(RotateFlipType.RotateNoneFlipX);

            return image;
        }

        /// <summary>
        /// Fetch a view from Google Street View API using the specified request.
        /// </summary>
        /// <param name="streetViewRequest">
        /// Street view request properties.
        /// </param>
        /// <returns>
        /// A Bitmap image representing the current view camera frustum.
        /// </returns>
        public StreetviewResponse DispatchRequest(StreetviewRequest streetViewRequest)
        {
            StreetviewResponse response;
            Bitmap image;
            MemoryStream ms;
            string url;
            object resource;

            image = null;
            url = streetViewRequest.GetServiceUrl();

            if(SceneManager.FetchSingle(url, out resource))
            {
                if(resource is MemoryStream)
                {
                    ms = resource as MemoryStream;

                    image = new Bitmap(ms);
                }
                else
                {
                    Console.WriteLine("[warning] INVALID RESOURCE TYPE " + streetViewRequest.ToString());
                }
            }
            else
            {
                Console.WriteLine("[warning] not found " + streetViewRequest.ToString());
            }

            
            response = new StreetviewResponse();

            response.Image = image;

            return response;
        }

        #endregion
    }
}
