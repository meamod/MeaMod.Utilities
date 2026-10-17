using System.Drawing;
using MeaMod.Utilities.UI.WinForms;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class CircularProgressControlTests
    {
        private static int CountNonWhitePixels(Bitmap bitmap)
        {
            int count = 0;
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    if (pixel.A != 0 && (pixel.R != 255 || pixel.G != 255 || pixel.B != 255))
                        count++;
                }
            }

            return count;
        }

        [TestMethod]
        public void DefaultsAreApplied()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl();

                Assert.AreEqual(60, control.Interval);
                Assert.AreEqual(Color.FromArgb(58, 58, 58), control.TickColor);
                Assert.AreEqual(CircularProgressControl.Direction.CLOCKWISE, control.Rotation);
                Assert.AreEqual(270f, control.StartAngle);
                Assert.AreEqual(new Size(28, 28), control.MinimumSize);
            });
        }

        [TestMethod]
        public void IntervalFallsBackToTheDefaultForNonPositiveValues()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl();

                control.Interval = 0;
                Assert.AreEqual(60, control.Interval);
                control.Interval = -5;
                Assert.AreEqual(60, control.Interval);
                control.Interval = 15;
                Assert.AreEqual(15, control.Interval);
            });
        }

        [TestMethod]
        public void PropertiesCanBeChanged()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl();

                control.Rotation = CircularProgressControl.Direction.ANTICLOCKWISE;
                control.TickColor = Color.Red;
                control.StartAngle = 10;

                Assert.AreEqual(CircularProgressControl.Direction.ANTICLOCKWISE, control.Rotation);
                Assert.AreEqual(Color.Red, control.TickColor);
                Assert.AreEqual(10f, control.StartAngle);
            });
        }

        [TestMethod]
        public void ResizingRaisesClientSizeChangedAndRecalculates()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl();
                bool raised = false;
                control.ClientSizeChanged += (_, _) => raised = true;

                control.Size = new Size(200, 200);
                Assert.IsTrue(raised, "the public ClientSizeChanged event did not fire");

                // Below the minimum: the control is clamped rather than failing.
                control.Size = new Size(10, 10);
                Assert.AreEqual(new Size(28, 28), control.Size);
            });
        }

        [TestMethod]
        public void PaintingDrawsSpokesAndRaisesThePaintEvent()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl { Size = new Size(100, 100), BackColor = Color.White };
                bool painted = false;
                control.Paint += (_, _) => painted = true;
                using var bitmap = new Bitmap(100, 100);

                control.DrawToBitmap(bitmap, new Rectangle(0, 0, 100, 100));

                Assert.IsTrue(painted, "the public Paint event did not fire");
                Assert.IsTrue(CountNonWhitePixels(bitmap) > 0, "nothing was drawn");
            });
        }

        [TestMethod]
        public void PaintingAnticlockwiseAlsoDraws()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl
                {
                    Size = new Size(60, 60),
                    BackColor = Color.White,
                    Rotation = CircularProgressControl.Direction.ANTICLOCKWISE,
                };
                using var bitmap = new Bitmap(60, 60);

                control.DrawToBitmap(bitmap, new Rectangle(0, 0, 60, 60));

                Assert.IsTrue(CountNonWhitePixels(bitmap) > 0, "nothing was drawn");
            });
        }

        [TestMethod]
        public void StartAdvancesTheAngleOnTheUiThreadAndStopHaltsIt()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl { Interval = 20 };
                float initial = control.StartAngle;

                control.Start();
                Assert.IsTrue(StaThread.PumpUntil(() => control.StartAngle != initial, 5000), "the timer never ticked");

                control.Stop();
                StaThread.Pump(100);
                float stopped = control.StartAngle;
                StaThread.Pump(200);
                Assert.AreEqual(stopped, control.StartAngle, "the angle kept changing after Stop()");
            });
        }

        [TestMethod]
        public void ClockwiseAngleWrapsAt360()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl { Interval = 20, StartAngle = 359 };

                control.Start();
                Assert.IsTrue(StaThread.PumpUntil(() => control.StartAngle != 359, 5000));
                control.Stop();

                Assert.IsTrue(control.StartAngle >= 0 && control.StartAngle < 359, control.StartAngle.ToString());
            });
        }

        [TestMethod]
        public void AnticlockwiseAngleWrapsAtMinus360()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl
                {
                    Interval = 20,
                    Rotation = CircularProgressControl.Direction.ANTICLOCKWISE,
                    StartAngle = -359,
                };

                control.Start();
                Assert.IsTrue(StaThread.PumpUntil(() => control.StartAngle != -359, 5000));
                control.Stop();

                Assert.IsTrue(control.StartAngle <= 0 && control.StartAngle > -359, control.StartAngle.ToString());
            });
        }

        [TestMethod]
        public void DisposeStopsTheTimerAndIsIdempotent()
        {
            StaThread.Run(() =>
            {
                var control = new CircularProgressControl { Interval = 20 };
                control.Start();
                StaThread.Pump(50);

                control.Dispose();
                Assert.IsTrue(control.IsDisposed);
                StaThread.Pump(100);
                control.Dispose();
            });
        }
    }
}
