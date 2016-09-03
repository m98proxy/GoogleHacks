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

        public static StreetviewProvider CreateProvider()
        {
            StreetviewProvider provider;

            settings = new ProviderSettings()
            {
                ApiKey = "", // If left blank, further requests and IP address can get blocked by Google.
                WorldSize = new Vector3(1000, 1000, 1000)
            };

            provider = new StreetviewProvider(settings);

            return provider;
        }

        public static void Reset(SceneCamera camera)
        {
            camera.Position = MathHelpers.EarthUVToCartesian(new Vector2(-41.4404713f, 147.127295f), settings.WorldSize);
        }

        public static void Unload()
        {
            skybox.Unload();
        }

        private static void loadSkybox()
        {
            skybox.LoadFromBitmapSides(left, right, front, back, top, bottom);

            isLoaded = true;
            loadedFirst = true;
        }

        public static void LookupPanorama(Vector3 position)
        {
            isLoaded = false;
            numSidesLoaded = 0;

            if (initilised)
            {
                Unload();
            }

            // get just 6 static images as a test

            //var test = new Bitmap("D:\\test-streetview.jpg");
            //var newSize = ImageTexture.GetTextureGLMaxSize(test);
            //ImageTexture.Rescale(ref test, new Size(512, 512));
            //left = right = front = back = top = bottom = test;

            Task.Run(() =>
            {
                right = provider.FetchView(position, 0, 4.5f + 0.17f);
                ++numSidesLoaded;
            });

            Task.Run(() =>
            {
                left = provider.FetchView(position, 0, 1.61f);
                ++numSidesLoaded;
            });

            Task.Run(() =>
            {
                front = provider.FetchView(position, 0, 0);
                ++numSidesLoaded;
            });

            Task.Run(() =>
            {
                back = provider.FetchView(position, 0, X3D.MathHelpers.PI + 0.7f);
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
                ++numSidesLoaded;
            });
        }

        public static void Initilize(SceneCamera camera)
        {
            provider = CreateProvider();

            // Set camera position to a valid Street view location
            Reset(camera);

            // Lookup from street view and make a panorama
            LookupPanorama(camera.Position);

            shapeBox = new Shape(new Box());

            LastPosition = camera.Position;

            initilised = true;
        }

        public static void Render(RenderingContext rc)
        {
            if(numSidesLoaded == 6)
            {
                loadSkybox();
                numSidesLoaded = -1;
            }

            rc.matricies.Scale = Vector3.One;

            if (!preRendered)
            {
                skybox.PreRenderOnce(rc);
                shapeBox.PreRenderOnce(rc);

                preRendered = true;
            }

            if (initilised && loadedFirst)
            {
                skybox.PreRender();
                skybox.Render(rc);
                skybox.PostRender(rc);
            }

            shapeBox.PreRender();
            shapeBox.Render(rc);
            shapeBox.PostRender(rc);
        }

        public static void Move(Vector3 direction, Vector3 position)
        {
            Vector2 uvEarth;

            LookupPanorama(position);

            // Translate position to geospacial earth coordinate for debugging
            uvEarth = MathHelpers.CartesianToEarthUV(position, settings.WorldSize);

            Console.WriteLine("Panorama moved to ({0},{1})", uvEarth.X, uvEarth.Y);

            LastPosition = position;
        }
    }
}
