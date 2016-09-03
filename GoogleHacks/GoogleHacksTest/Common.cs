/* Copyright © 2016
 * Author: Gerallt Franke */

using System;
using System.Drawing;

using OpenTK;
using Google;
using GoogleHacks;

using X3D;
using X3D.Engine;
using X3D.Core;

namespace GoogleHacksTest
{
    public class Common
    {
        public static StreetviewProvider CreateProvider()
        {
            StreetviewProvider provider;
            ProviderSettings settings;

            settings = new ProviderSettings()
            {
                ApiKey = "", // If left blank, further requests and IP address can get blocked by Google.
                WorldSize = new Vector3(1000, 1000, 1000)
            };

            provider = new StreetviewProvider(settings);

            return provider;
        }
    }
}
