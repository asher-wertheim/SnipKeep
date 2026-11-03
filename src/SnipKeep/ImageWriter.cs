using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SnipKeep
{
    internal enum SaveStatus
    {
        Saved,
        Duplicate,
        Failed,
    }

    internal sealed class SaveResult
    {
        public SaveStatus Status { get; set; }
        public string Path { get; set; }
        public Exception Error { get; set; }
    }

    /// <summary>Encodes screenshots as PNG and writes them to disk. Skips an image identical to the last one saved.</summary>
    internal sealed class ImageWriter
    {
        private readonly object _lock = new object();
        private byte[] _lastHash;

        /// <summary>Saves the image and disposes it. Safe to call from a background thread.</summary>
        public SaveResult Save(Image image, string folder, string namePattern, DateTime time)
        {
            try
            {
                byte[] png;
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Png);
                    png = stream.ToArray();
                }

                byte[] hash;
                using (var sha = SHA256.Create()) hash = sha.ComputeHash(png);

                lock (_lock)
                {
                    if (_lastHash != null && _lastHash.SequenceEqual(hash))
                    {
                        return new SaveResult { Status = SaveStatus.Duplicate };
                    }

                    var path = Write(png, folder, FileNamer.FormatOrDefault(namePattern, time));
                    _lastHash = hash;
                    return new SaveResult { Status = SaveStatus.Saved, Path = path };
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                                       ex is ArgumentException || ex is NotSupportedException ||
                                       ex is System.Runtime.InteropServices.ExternalException)
            {
                return new SaveResult { Status = SaveStatus.Failed, Error = ex };
            }
            finally
            {
                image.Dispose();
            }
        }

        private static string Write(byte[] png, string folder, string baseName)
        {
            // The folder is not created here. If the user deleted it, they should hear about it.
            if (!Directory.Exists(folder)) throw new DirectoryNotFoundException(folder);

            for (var attempt = 0; ; attempt++)
            {
                var path = FileNamer.UniquePath(folder, baseName, File.Exists);
                try
                {
                    using (var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        file.Write(png, 0, png.Length);
                    }
                    return path;
                }
                catch (IOException) when (attempt < 5 && File.Exists(path))
                {
                    // Another file took this name a moment ago. Pick the next free name.
                }
            }
        }
    }
}
