using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Storage.Net.Extensions;
using Storage.Net.Core.Abstractions.StorageProviders;
using System.Collections.Generic;

namespace Storage.Net.StorageProviders
{
    public class MemoryStorageProvider : IStorageProvider
    {
        ConcurrentDictionary<string, byte[]> dictionary = new ConcurrentDictionary<string, byte[]>();

        public bool IsThreadSafe { get; } = true;

        public async Task CreateAsync(string key, Stream data)
        {
            MemoryStream memoryStream = await data.ToMemoryStreamAsync().ConfigureAwait(false);
            dictionary.TryAdd(key, memoryStream.ToArray());
        }

        public Task DeleteAsync(string key)
        {
            dictionary.TryRemove(key, out var stream);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key) =>
            Task.FromResult(dictionary.ContainsKey(key));

        public Task<Stream> ReadAsync(string key)
        {
            Stream result = null;
            if (dictionary.TryGetValue(key, out byte[] byteContent))
                result = new MemoryStream(byteContent);

            return Task.FromResult(result);
        }
    }
}
