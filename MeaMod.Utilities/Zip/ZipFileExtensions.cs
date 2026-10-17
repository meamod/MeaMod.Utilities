using System;
using System.IO.Compression;
using System.IO;

namespace MeaMod.Utilities.Zip
{
    /// <summary>
    /// https://stackoverflow.com/a/43662519
    /// </summary>
    public static class ZipFileExtensions
    {
        /// <summary>
        /// Extracts all of the files in the archive to <paramref name="destinationDirectoryName"/>, reporting progress after each entry.
        /// </summary>
        /// <param name="source">The archive to extract.</param>
        /// <param name="destinationDirectoryName">The directory to extract into.</param>
        /// <param name="progress">Optional progress callback.</param>
        public static void ExtractToDirectory(this ZipArchive source, string destinationDirectoryName, IProgress<ZipProgress> progress)
        {
            ExtractToDirectory(source, destinationDirectoryName, progress, overwrite: false);
        }

        /// <summary>
        /// Extracts all of the files in the archive to <paramref name="destinationDirectoryName"/>, reporting progress after each entry.
        /// </summary>
        /// <param name="source">The archive to extract.</param>
        /// <param name="destinationDirectoryName">The directory to extract into.</param>
        /// <param name="progress">Optional progress callback.</param>
        /// <param name="overwrite">True to overwrite existing files.</param>
        /// <exception cref="IOException">An entry would extract outside <paramref name="destinationDirectoryName"/>.</exception>
        public static void ExtractToDirectory(this ZipArchive source, string destinationDirectoryName, IProgress<ZipProgress> progress, bool overwrite)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (destinationDirectoryName == null)
                throw new ArgumentNullException(nameof(destinationDirectoryName));

            // Rely on Directory.CreateDirectory for validation of destinationDirectoryName.

            // Note that this will give us a good DirectoryInfo even if destinationDirectoryName exists:
            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            // The containment check below must compare against the directory *with* a trailing separator,
            // otherwise "C:\out-evil\x" would be accepted as being inside "C:\out".
            char lastChar = destinationDirectoryFullPath[destinationDirectoryFullPath.Length - 1];
            if (lastChar != Path.DirectorySeparatorChar && lastChar != Path.AltDirectorySeparatorChar)
                destinationDirectoryFullPath += Path.DirectorySeparatorChar;

            int count = 0;
            foreach (ZipArchiveEntry entry in source.Entries)
            {
                count++;
                string fileDestinationPath = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, entry.FullName));

                if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("File is extracting to outside of the folder specified.");

                progress?.Report(new ZipProgress(source.Entries.Count, count, entry.FullName));

                if (Path.GetFileName(fileDestinationPath).Length == 0)
                {
                    // If it is a directory:

                    if (entry.Length != 0)
                        throw new IOException("Directory entry with data.");

                    Directory.CreateDirectory(fileDestinationPath);
                }
                else
                {
                    // If it is a file:
                    // Create containing directory:
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDestinationPath));
                    entry.ExtractToFile(fileDestinationPath, overwrite: overwrite);
                }
            }
        }
    }
}
