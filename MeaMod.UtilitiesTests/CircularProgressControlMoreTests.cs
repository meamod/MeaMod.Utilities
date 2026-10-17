using System.Drawing;
using MeaMod.Utilities.UI.WinForms;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class CircularProgressControlMoreTests
    {
        [TestMethod]
        public void TinyControlClampsTheRadiiToTheirMinimums()
        {
            StaThread.Run(() =>
            {
                using var control = new CircularProgressControl { MinimumSize = new Size(10, 10), BackColor = Color.White };

                control.Size = new Size(10, 10);
                Assert.AreEqual(new Size(10, 10), control.Size);

                // Painting at this size must still work with the clamped inner and outer radii.
                using var bitmap = new Bitmap(10, 10);
                control.DrawToBitmap(bitmap, new Rectangle(0, 0, 10, 10));
            });
        }
    }
}
