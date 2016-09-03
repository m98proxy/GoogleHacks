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
        private static Background skybox; // use x3d-finely-sharpened to draw the skybox
        private static StreetviewProvider provider;
        private static ProviderSettings settings;

        private static bool preRendered = false;

        private static Bitmap left, right, top, bottom, front, back;

        private static Shape shapeBox;

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

        public static void Initilize(SceneCamera camera)
        {
            provider = CreateProvider();

            // Set camera position to a valid Street view location
            camera.Position = GoogleHacks.MathHelpers.EarthUVToCartesian(new Vector2(-41.4404713f, 147.127295f), settings.WorldSize);




            // get just 6 static images as a test

            var test = new Bitmap("D:\\test-streetview.jpg");
            var newSize = ImageTexture.GetTextureGLMaxSize(test);
            ImageTexture.Rescale(ref test, new Size(512, 512));
            //left = right = front = back = top = bottom = test;

            right = provider.FetchView(camera.Position, 0, 4.5f);
            left = provider.FetchView(camera.Position, 0, 1.61f);


            front = provider.FetchView(camera.Position, 0, X3D.MathHelpers.PI);
            back = provider.FetchView(camera.Position, 0, X3D.MathHelpers.PI2);

            top = provider.FetchView(camera.Position, X3D.MathHelpers.PIOver2, 0);
            bottom = provider.FetchView(camera.Position, -X3D.MathHelpers.PIOver2, 0);


            skybox = new Background();
            skybox.LoadFromBitmapSides(left, right, front, back, top, bottom);

            shapeBox = new Shape(new Box());
        }

        public static void Render(RenderingContext rc)
        {


            rc.matricies.Scale = Vector3.One;

            if (!preRendered)
            {
                skybox.PreRenderOnce(rc);
                shapeBox.PreRenderOnce(rc);

                preRendered = true;
            }

            skybox.PreRender();
            skybox.Render(rc);
            skybox.PostRender(rc);


            shapeBox.PreRender();
            shapeBox.Render(rc);
            shapeBox.PostRender(rc);


        }
    }
}
