namespace MeaMod.Utilities.Zip
{
    /// <summary>Progress of a <see cref="ZipFileExtensions.ExtractToDirectory(System.IO.Compression.ZipArchive, string, System.IProgress{ZipProgress})"/> call.</summary>
    public class ZipProgress
    {
        /// <summary>Creates a new instance of this type.</summary>
        /// <param name="total">The number of entries in the archive.</param>
        /// <param name="processed">The number of entries extracted so far, including the current one.</param>
        /// <param name="currentItem">The full name of the entry being extracted.</param>
        public ZipProgress(int total, int processed, string currentItem)
        {
            Total = total;
            Processed = processed;
            CurrentItem = currentItem;
        }

        /// <summary>Gets the number of entries in the archive.</summary>
        public int Total { get; }

        /// <summary>Gets the number of entries extracted so far, including the current one.</summary>
        public int Processed { get; }

        /// <summary>Gets the full name of the entry being extracted.</summary>
        public string CurrentItem { get; }
    }
}
