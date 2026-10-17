using System.Reflection;

namespace MeaMod.Utilities.Tests
{
    /// <summary>
    /// Locates files under the testfiles folder. That folder is git-ignored, so the catalog and signature fixtures
    /// exist only on machines where they were generated; tests that need one report inconclusive elsewhere.
    /// </summary>
    internal static class Fixtures
    {
        public static readonly string Root = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "testfiles");

        public static string Require(string relativePath)
        {
            string path = Path.Combine(Root, relativePath);
            if (!File.Exists(path) && !Directory.Exists(path))
                Assert.Inconclusive("fixture not present on this machine (the testfiles folder is git-ignored): " + path);
            return path;
        }
    }
}
