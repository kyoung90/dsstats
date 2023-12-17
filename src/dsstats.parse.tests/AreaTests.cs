using dsstats.parser;

namespace dsstats.parse.tests
{
    [TestClass]
    public class AreaTests
    {
        [TestMethod]
        public void PointNotInsideTest()
        {
            Area area = Parser.Area2;

            var result = area.IsPointInside(new(255, 255));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PointInsideTest()
        {
            Area area = Parser.Area2;

            var result = area.IsPointInside(new(180, 180));

            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow(180, 181)]
        [DataRow(180, 182)]
        [DataRow(180, 183)]
        [DataRow(180, 184)]
        [DataRow(180, 185)]
        [DataRow(180, 186)]
        [DataRow(180, 187)]
        [DataRow(180, 188)]
        [DataRow(190, 170)]
        [DataRow(190, 171)]
        [DataRow(190, 172)]
        [DataRow(190, 173)]
        [DataRow(190, 174)]
        [DataRow(190, 175)]
        public void PointInsideTests(int x, int y)
        {
            Area area = Parser.Area2;
            Point point = new(x, y);
            var result = area.IsPointInside(point);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [DataRow(180, 181)]
        [DataRow(180, 182)]
        [DataRow(180, 183)]
        [DataRow(180, 184)]
        [DataRow(180, 185)]
        [DataRow(180, 186)]
        [DataRow(180, 187)]
        [DataRow(180, 188)]
        [DataRow(190, 170)]
        [DataRow(190, 171)]
        [DataRow(190, 172)]
        [DataRow(190, 173)]
        [DataRow(190, 174)]
        [DataRow(190, 175)]
        public void PointOutsideTests(int x, int y)
        {
            Area area = Parser.Area1;
            Point point = new(x, y);
            var result = area.IsPointInside(point);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MidPointTest()
        {
            var result = Area.Midpoint(new(160, 152), new(189, 163), new(171, 181));

            Assert.AreEqual(new Point(170, 162), result);
        }

        [TestMethod]
        public void MoveTeam1Test()
        {
            var result = Area.Midpoint(new(160, 152), new(189, 163), new(171, 181));

            var movedArea = Parser.Area2.MoveTowards(result);

            Assert.AreEqual(new Area(new(173, 147), new(155, 165), new(167, 177), new(185, 159)), movedArea);
        }

        [TestMethod]
        public void MoveTeam2Test()
        {
            var result = Area.Midpoint(new(96, 88), new(84, 58), new(66, 76));

            var movedArea = Parser.Area2.MoveTowards(result);

            Assert.AreEqual(new Area(new(89, 63), new(71, 81), new(83, 93), new(101, 75)), movedArea);
        }
    }
}