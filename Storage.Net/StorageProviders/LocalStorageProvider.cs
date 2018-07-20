using System.IO;
using System.Threading.Tasks;
using Storage.Net.Core.Abstractions.StorageProviders;

namespace Storage.Net.StorageProviders
{
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly string basePath;

        public LocalStorageProvider(string basePath)
        {
            this.basePath = basePath;
        }

        public bool IsThreadSafe { get; } = false;

        public async Task CreateAsync(string key, Stream data)
        {
            using (var stream = new FileStream(CombinedPath(key), FileMode.Create))
                await data.CopyToAsync(stream).ConfigureAwait(false);
        }

        public Task DeleteAsync(string key)
        {
            return Task.Run(() => File.Delete(CombinedPath(key)));
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.Run(() => File.Exists(CombinedPath(key)));
        }

        public Task<Stream> ReadAsync(string key)
        {
            Stream stream = new FileStream(CombinedPath(key), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Task.FromResult(stream);
        }

        private string CombinedPath(string key) => Path.Combine(basePath, key);
    }
}
