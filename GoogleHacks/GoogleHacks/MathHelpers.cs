/* Copyright © 2016
 * Author: Gerallt Franke */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace GoogleHacks
{
    public class MathHelpers : X3D.MathHelpers
    {
        /// <summary>
        /// Translates a cartesian coordinate in 3D space to a geospacial point 
        /// (UV Spherical coordinate) within the bounds of the earth.
        /// </summary>
        /// <param name="position">
        /// Position of camera frustum in 3D space.
        /// </param>
        /// <param name="boundingVolume">
        /// Formal: The bounding volume of the virtual world in 3D space.
        /// Informal: The size of the virtual world.
        /// </param>
        /// <returns>
        /// A Vector2 in the following format:
        /// (X: Latitude, Y: Longitude)
        /// </returns>
        public static Vector2 CartesianToEarthUV(Vector3 position, Vector3 boundingVolume)
        {
            Vector2 uv;
            float longitude; // x-axis
            float latitude;  // y-axis
            Vector3 bbox;
            Vector3 positionRatio;

            bbox = boundingVolume;

            // Normalize the 3D position between [0, 1]
            positionRatio = new Vector3(position.X / bbox.X, position.Y / bbox.Y, position.Z / bbox.Z);

            // Convert to geospacial
            latitude = (positionRatio.X * 180.0f) - 90.0f; // clamp latitude range between: -90, +90
            longitude = (positionRatio.Y * 360.0f) - 180.0f; // clamp longitide range between: -180, +180

            // Return as a vector
            uv = new Vector2(latitude, longitude);

            return uv;
        }

        /// <summary>
        /// Translate a UV Spherical coordinate (Earth lat/lng) to a coordinate in 3D world space.
        /// </summary>
        /// <param name="uvAngle">
        /// UV angle around the earth, latitude and longitude respectfully.
        /// X between (-90, +90)
        /// Y between (-180, 180)
        /// </param>
        /// <param name="boundingVolume">
        /// Formal: The bounding volume of the virtual world in 3D space.
        /// Informal: The size of the virtual world.
        /// </param>
        /// <returns>
        /// Position in 3D space.
        /// </returns>
        public static Vector3 EarthUVToCartesian(Vector2 uvAngle, Vector3 boundingVolume)
        {
            Vector3 pos;
            Vector3 bbox;
            Vector3 positionRatio;

            bbox = boundingVolume;

            uvAngle.X = X3D.MathHelpers.ClampDegrees(uvAngle.X);
            uvAngle.Y = X3D.MathHelpers.ClampDegrees(uvAngle.Y);

            // Normalize from geospacial clamping between [0, 1]
            positionRatio = new Vector3(X3D.MathHelpers.ClampCircularShift(uvAngle.X + 90.0f, 0, 90.0f) / 180.0f, // normalize latitude component
                                        X3D.MathHelpers.ClampDegrees((uvAngle.Y + 180.0f)) / 360.0f, 0); // normalize longitude component
            

            // Translate to cartesian 3D spacial coordiante within world bounds
            pos = new Vector3(positionRatio.X * bbox.X, positionRatio.Y * bbox.Y, 0);

            return pos;
        }
    }
}
