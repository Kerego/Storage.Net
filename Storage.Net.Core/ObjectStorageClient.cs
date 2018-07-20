using System;
using System.Threading.Tasks;
using Storage.Net.Core.Abstractions.Serializers;

namespace Storage.Net.Core
{
    public class ObjectStorageClient
    {
        private readonly ISerializer _serializer;
        private readonly RawStorageClient _storageClient;

        public ObjectStorageClient(RawStorageClient storageClient, ISerializer serializer)
        {
            this._storageClient = storageClient;
            this._serializer = serializer;
        }

        public async Task WriteAsync<T>(string key, T obj, bool replace = true) where T : class
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException($"key should not be null or empty string", nameof(key));

            if (obj is null)
                throw new ArgumentNullException(nameof(obj), "object should not be null");


            using (var str = _serializer.Serialize(obj))
                await _storageClient.WriteAsync(key, str, replace);
        }

        public async Task<T> ReadAsync<T>(string key) where T : class
        {
            using (var stream = await _storageClient.ReadAsStreamAsync(key))
            {
                if (stream is null)
                    return null;
                else
                    return _serializer.Deserialize<T>(stream);
            }
        }
    }

}
