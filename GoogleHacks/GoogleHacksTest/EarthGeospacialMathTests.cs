using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace GoogleHacksTest
{
    [TestClass]
    public class EarthGeospacialMathTests
    {

        [TestMethod]
        public void TestCoordinateTranslations()
        {
            Vector2 originUv;
            Vector2 uv;
            Vector3 pos;
            Vector3 bbox;

            bbox = new Vector3(1000, 1000, 1000);
            uv = new Vector2(0, 188);

            // Translate the latitude and longitude to a world position
            pos = GoogleHacks.MathHelpers.EarthUVToCartesian(uv, bbox);

            Assert.IsTrue(pos.X == 0.0f);
            Assert.IsTrue((int)(pos.Y) == 22);
            Assert.IsTrue(pos.Z == 0.0f);

            // Find world-origin represented as uv
            originUv = GoogleHacks.MathHelpers.CartesianToEarthUV(Vector3.Zero, bbox);

            Assert.IsTrue(originUv.X == -90);
            Assert.IsTrue(originUv.Y == -180);


        }

    }
}
