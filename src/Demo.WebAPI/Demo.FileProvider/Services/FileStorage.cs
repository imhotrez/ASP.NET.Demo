using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Demo.FileProvider.Services {
    public class FileStorage {
        private readonly IConfiguration configuration;
        private const string OriginalFileName = "Original";
        private const string PreviewFileName = "Preview";

        public FileStorage(IConfiguration configuration) { this.configuration = configuration; }

        public async Task<bool> Save(long id, byte[] originalBody, byte[] previewBody, CancellationToken token) {
            var folderPath = GetFolderPath(id);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var originalFilePath = Path.Combine(folderPath, OriginalFileName);
            var previewFilePath = Path.Combine(folderPath, PreviewFileName);

            await File.WriteAllBytesAsync(originalFilePath, originalBody, token);
            await File.WriteAllBytesAsync(previewFilePath, previewBody, token);
            return true;
        }

        public async Task<byte[]> GetPreview(long id, CancellationToken token) {
            return await Get(id, PreviewFileName, token);
        }

        public async Task<byte[]> GetOriginal(long id, CancellationToken token) {
            return await Get(id, OriginalFileName, token);
        }

        private async Task<byte[]> Get(long id, string filename, CancellationToken token) {
            var folderPath = GetFolderPath(id);
            var originalFilePath = Path.Combine(folderPath, filename);
            var result = await File.ReadAllBytesAsync(originalFilePath, token);
            return result;
        }

        private string GetFolderPath(long id) {
            var stringId = id.ToString();
            var folderPath = stringId.Aggregate(string.Empty, (current, t) => current + $"{t}\\");
            var rootStorageFolder = configuration["RootStorageFolder"];
            if (string.IsNullOrWhiteSpace(rootStorageFolder)) {
                throw new Exception(
                    "В конфигурации сервиса-поставщике файлов отсутствует адрес корневой папки хранилища");
            }

            return Path.Combine(rootStorageFolder, folderPath);
        }
    }
}