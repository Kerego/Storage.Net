using System;
using System.IO;
using System.Threading.Tasks;
using Storage.Net.Core.Abstractions.StorageProviders;
using System.Linq;

namespace Storage.Net.Core
{
    public class RawStorageClient : IDisposable
    {
        private readonly KeyedSemaphoreSlim _semaphore = new KeyedSemaphoreSlim();
        private readonly IStorageProvider _storageProvider;

        public RawStorageClient(IStorageProvider storageProvider)
        {
            this._storageProvider = storageProvider;
        }

        public async Task WriteAsync(string key, byte[] data, bool replace = true)
        {
            //ideally this would be handled by non nullable reference type in C# 8
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException($"key should not be null or empty string", nameof(key));
            if (data is null)
                throw new ArgumentNullException(nameof(data), "Cannot write null data");

            using (var stream = new MemoryStream(data))
                await WriteAsync(key, stream, replace);
        }

        public async Task WriteAsync(string key, Stream data, bool replace = true)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException($"key should not be null or empty string", nameof(key));
            if (data is null)
                throw new ArgumentNullException(nameof(data), "Cannot write null data");

            if (!_storageProvider.IsThreadSafe)
                await _semaphore.WaitAsync(key);

            try
            {
                if (await _storageProvider.ExistsAsync(key).ConfigureAwait(false))
                {
                    if (replace)
                        await _storageProvider.DeleteAsync(key).ConfigureAwait(false);
                    else
                        throw new IOException("File already exists");
                }

                await _storageProvider.CreateAsync(key, data).ConfigureAwait(false);
            }
            finally
            {
                if (!_storageProvider.IsThreadSafe)
                    _semaphore.Release(key);
            }
        }

        public async Task<Stream> ReadAsStreamAsync(string key)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException($"key should not be null or empty string", nameof(key));

            if (!await _storageProvider.ExistsAsync(key))
                return null;

            if (!_storageProvider.IsThreadSafe)
                await _semaphore.WaitAsync(key);
            try
            {
                return await _storageProvider.ReadAsync(key);
            }
            finally
            {
                if (!_storageProvider.IsThreadSafe)
                    _semaphore.Release(key);
            }
        }

        public async Task<byte[]> ReadRawAsync(string key)
        {
            using (var stream = await ReadAsStreamAsync(key))
            {
                if (stream is null)
                    return null;

                if (stream is MemoryStream ms)
                    return ms.ToArray();

                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }

}
