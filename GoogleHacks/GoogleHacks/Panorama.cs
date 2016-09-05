/* Copyright © 2016
 * Author: Gerallt Franke */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using OpenTK;
using X3D;
using X3D.Engine;
using Google;

namespace GoogleHacks
{
    public class Panorama
    {
        private static Background skybox = new Background();
        private static StreetviewProvider provider;
        private static ProviderSettings settings;

        private static bool preRendered = false;

        private static Bitmap left, right, top, bottom, front, back;

        private static Shape shapeBox;

        public static Vector3 LastPosition;

        public static bool initilised = false;
        public static bool isLoaded = false;
        public static bool loadedFirst = false;
        public static int numSidesLoaded = 0;

        public static Vector3 origin;

        public static StreetviewProvider CreateProvider()
        {
            StreetviewProvider provider;

            settings = new ProviderSettings()
            {
                ApiKey = "AIzaSyD1u1BGcJCUUGbzw1iXNpYKnK-wRSW2EfY", // If left blank, further requests and IP address can get blocked by Google.
                WorldSize = new Vector3(1000000, 1000000, 1000000),
                Size = new Vector2(640, 640) // each tile must be a square for cube mapping to work
            };

            provider = new StreetviewProvider(settings);

            return provider;
        }

        public static void Reset(SceneCamera camera)
        {
            camera.Position = origin;
        }

        public static void Unload()
        {
            skybox.Unload();
        }

        private static void loadPanorama()
        {
            //TODO: stitch panorama removing seams 

            if (initilised)
            {
                Unload();
            }

            skybox.LoadFromBitmapSides(left, right, front, back, top, bottom);

            isLoaded = true;
            loadedFirst = true;
        }

        public static SceneCamera camera;

        public static void LookupPanorama(Vector3 position)
        {
            isLoaded = false;
            numSidesLoaded = 0;

            // Get 6 static images and build cubemapped panorama.

            //TODO: stitch panorama and remove seams 

            float pitchCalib = 0.024f;

            Task.Run(() =>
            {
                right = provider.FetchView(position, 0, MathHelpers.ThreePIOver2);
                ++numSidesLoaded;
            });

            Task.Run(() =>
            {
                left = provider.FetchView(position, 0, MathHelpers.PIOver2);
                ++numSidesLoaded;
            });

            Task.Run(() =>
            {
                // move pitch down by constant calibration value (note still need to implement image stitching as this value changes)

                front = provider.FetchView(position, -pitchCalib, 0);
                //front = new Bitmap(front.Width, front.Height, front.PixelFormat);

                ++numSidesLoaded;
            });

            Task.Run(() =>
            {
                // move pitch up by constant calibration value (note still need to implement image stitching as this value changes)

                back = provider.FetchView(position, pitchCalib, X3D.MathHelpers.PI); 
                //back = new Bitmap(back.Width, back.Height, back.PixelFormat);

                ++numSidesLoaded;
            });

            Task.Run(() =>
            {
                top = provider.FetchView(position, X3D.MathHelpers.PIOver2, 0);

                ++numSidesLoaded;
            });

            Task.Run(() =>
            {
                bottom = provider.FetchView(position, -X3D.MathHelpers.PIOver2, -X3D.MathHelpers.PIOver2);
                bottom.RotateFlip(RotateFlipType.Rotate90FlipNone);

                ++numSidesLoaded;
            });
        }

        public static void Initilize(SceneCamera camera)
        {
            Panorama.camera = camera;

            provider = CreateProvider();

            origin = MathHelpers.EarthUVToCartesian(new Vector2(-41.4404713f, 147.127295f), settings.WorldSize.Value);

            // Set camera position to a valid Street view location
            Reset(camera);

            // Lookup from street view and make a panorama
            LookupPanorama(camera.Position);

            shapeBox = new Shape(geometry: new Box(), transform: Transform.CreateTranslation(camera.Position + camera.Forward));

            LastPosition = camera.Position;

            initilised = true;
        }

        public static void Render(RenderingContext rc)
        {
            if(numSidesLoaded == 6)
            {
                loadPanorama();
                numSidesLoaded = -1;
            }

            rc.matricies.Scale = Vector3.One;

            //skybox.Draw(rc);

            if (!preRendered)
            {
                skybox.PreRenderOnce(rc);

                preRendered = true;
            }

            if (initilised && loadedFirst)
            {
                skybox.PreRender();
                skybox.Render(rc);
                skybox.PostRender(rc);
            }

            shapeBox.Draw(rc);
        }

        public static void Move(Vector3 direction, Vector3 position)
        {
            Vector2 uvEarth;

            // TODO: snap position to nearest road to solve panorama query errors

            LookupPanorama(position);

            // Translate position to geospacial earth coordinate for debugging
            uvEarth = MathHelpers.CartesianToEarthUV(position, settings.WorldSize.Value);

            Console.WriteLine("Panorama moved to ({0},{1})", uvEarth.X, uvEarth.Y);

            LastPosition = position;
        }
    }
}
