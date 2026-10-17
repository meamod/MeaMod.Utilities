namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class EnumExtensionsTests
    {
        private enum Sample
        {
            [System.ComponentModel.Description("First value")]
            WithDescription,
            WithoutDescription,
        }

        [TestMethod]
        public void ReturnsTheDescriptionAttribute()
        {
            Assert.AreEqual("First value", Sample.WithDescription.GetDescription());
        }

        [TestMethod]
        public void ReturnsNullWhenThereIsNoAttribute()
        {
            Assert.IsNull(Sample.WithoutDescription.GetDescription());
        }

        [TestMethod]
        public void ReturnsNullForAnUndefinedValue()
        {
            Assert.IsNull(((Sample)99).GetDescription());
        }
    }
}
