using System.Drawing;
using MeaMod.Utilities.UI.WinForms;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class ServiceStatusLabelTests
    {
        [TestMethod]
        public void DefaultsAndSetters()
        {
            StaThread.Run(() =>
            {
                using var label = new ServiceStatusLabel();

                Assert.AreEqual("Status", label.Status);
                Assert.AreEqual(Color.Green, label.StatusColour);

                label.Status = "Running";
                label.StatusColour = Color.Red;

                Assert.AreEqual("Running", label.Status);
                Assert.AreEqual(Color.Red, label.StatusColour);
            });
        }

        [TestMethod]
        public void PaintingDrawsTheText()
        {
            StaThread.Run(() =>
            {
                using var label = new ServiceStatusLabel
                {
                    Size = new Size(240, 30),
                    BackColor = Color.White,
                    Status = "Running",
                    StatusColour = Color.Red,
                };
                using var bitmap = new Bitmap(240, 30);

                label.DrawToBitmap(bitmap, new Rectangle(0, 0, 240, 30));

                int painted = 0;
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                        if (bitmap.GetPixel(x, y).ToArgb() != Color.White.ToArgb())
                            painted++;

                Assert.IsGreaterThan(0, painted, "nothing was drawn");
            });
        }
    }
}
