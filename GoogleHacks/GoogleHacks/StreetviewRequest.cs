/* Copyright © 2016
 * Author: Gerallt Franke */

using System;
using System.Collections.Generic;

namespace Google
{
    /// <summary>
    /// Street view API service request model.
    /// </summary>
    public class StreetviewRequest
    {
        /// <summary>
        /// The Google Application API Key.
        /// If left blank, further requests and IP address can get blocked by Google.
        /// </summary>
        public string ApiKey;

        /// <summary>
        /// The Width of the Streetview image.
        /// </summary>
        public int Width = 640;

        /// <summary>
        /// The Height of the Streetview image.
        /// </summary>
        public int Height = 480;

        /// <summary>
        /// The UV spherical latitude coordinate component.
        /// (-90, +90)
        /// </summary>
        public double Latitude = 46.414382;

        /// <summary>
        /// The UV spherical longitude coordinate component.
        /// (-180, +180)
        /// </summary>
        public double Longitude = 10.013988;

        /// <summary>
        /// The heading angle of the camera [0 - 360].
        /// </summary>
        public double Heading = 186;

        /// <summary>
        /// The heading angle of the camera [0 - 360].
        /// </summary>
        public double Pitch = -0.76;

        /// <summary>
        /// The field-of-view angle of the camera. [0 - 120]
        /// </summary>
        public double Fov = 90;

        /// <summary>
        /// The Google Streetview Service API URL.
        /// </summary>
        private string service = "https://maps.googleapis.com/maps/api/streetview?size={0}x{1}&location={2},{3}&heading={4}&pitch={5}&fov={6}&key={7}";


        /// <summary>
        /// Builds the google street view service url given Streetview properties.
        /// </summary>
        /// <returns>
        /// The Service URL.
        /// </returns>
        public string GetServiceUrl()
        {
            string url;

            url = string.Format(service, Width, Height, Latitude, Longitude, Heading, Pitch, Fov, ApiKey);

            return url;
        }

        /// <summary>
        /// Build a string representing the current request state.
        /// </summary>
        /// <returns>
        /// A string representation of the current properties in the street view request.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Image At LtLng:({0}, {1}), Py:({2}, {3}) service url {4}", Latitude, Longitude, Pitch, Heading, GetServiceUrl());
        }
    }
}
