/* Copyright © 2016
 * Author: Gerallt Franke */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

using OpenTK;
using Google;
using GoogleHacks;

using X3D;
using X3D.Engine;
using X3D.Core;

namespace GoogleHacksTest
{
    [TestClass]
    public class FetchViewTests
    {

        [TestMethod]
        public void TestFetchView()
        {
            var provider = Common.CreateProvider();

            var position = new Vector3(0, 0, 0);
            var orientation = new Vector2(0, 3.281218993749f); // (0, 188) degrees

            var image = provider.FetchView(position, orientation);

            Assert.IsNotNull(image);
            Assert.IsInstanceOfType(image, typeof(Bitmap));

            try
            {
                var test = "D:\\test-streetview.jpg";
                System.IO.File.Delete(test);
                image.Save(test);
            }
            catch { }
        }
    }
}
