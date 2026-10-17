using MeaMod.Utilities.NetworkTools.Extensions;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class IEnumerableExtensionsTests
    {
        [TestMethod]
        public void EachRunsTheActionForEveryItemAndReturnsTheSameSequence()
        {
            var source = new List<int> { 1, 2, 3 };
            var seen = new List<int>();

            IEnumerable<int> result = source.Each(seen.Add);

            CollectionAssert.AreEqual(source, seen);
            Assert.AreSame(source, result);
        }

        [TestMethod]
        public void EachRejectsNullArguments()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).Each(_ => { }));
            Assert.ThrowsExactly<ArgumentNullException>(() => new[] { 1 }.Each(null!));
        }
    }
}
