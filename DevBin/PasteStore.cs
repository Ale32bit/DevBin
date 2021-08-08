using System.IO;
using System.IO.Compression;
using System.Text;

namespace DevBin
{
    public class PasteStore
    {
        private readonly string _dataPath;
        public PasteStore(string path)
        {
            _dataPath = path;
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
        }

        public string Read(string code)
        {
            var path = Path.Combine(_dataPath, code);

            if (!Exists(code))
                return string.Empty;

            using FileStream originalFileStream = File.OpenRead(path);
            using MemoryStream decompressedFileStream = new();
            using GZipStream gZipStream = new(originalFileStream, CompressionMode.Decompress);
            gZipStream.CopyTo(decompressedFileStream);

            return Encoding.UTF8.GetString(decompressedFileStream.ToArray());
        }

        public void Write(string code, string content)
        {
            var path = Path.Combine(_dataPath, code);

            byte[] byteArray = Encoding.UTF8.GetBytes(content);
            using MemoryStream originalStream = new(byteArray);
            using FileStream compressedFileStream = File.Create(path);
            using GZipStream gZipStream = new(compressedFileStream, CompressionMode.Compress);
            originalStream.CopyTo(gZipStream);
        }

        public bool Exists(string code)
        {
            return File.Exists(Path.Combine(_dataPath, code));
        }

        public void Delete(string code)
        {
            var path = Path.Combine(_dataPath, code);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
