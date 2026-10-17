using MeaMod.Utilities.NetworkTools.Extensions;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class IEnumerableExtensionsTests
    {
        [TestMethod]
        public void EachRunsTheActionForEveryItemAndReturnsTheSameSequence()
        {
            var source = new List<int> { 1, 2, 3 };
            var seen = new List<int>();

            IEnumerable<int> result = source.Each(seen.Add);

            Assert.AreSequenceEqual(source, seen);
            Assert.AreSame(source, result);
        }

        private static readonly int[] instance = [1];

        [TestMethod]
        public void EachRejectsNullArguments()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ((IEnumerable<int>)null!).Each(_ => { }));
            Assert.ThrowsExactly<ArgumentNullException>(() => instance.Each(null!));
        }
    }
}
