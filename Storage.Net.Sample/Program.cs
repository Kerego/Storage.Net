using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Storage.Net.Core;
using Storage.Net.Serializers;
using Storage.Net.StorageProviders;
using System.Linq;

namespace Storage.Net.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var jsonSerializer = new Serializers.JsonSerializer(new JsonSerializerSettings());
            var fileStorageProvider = new LocalStorageProvider(@".\");
            var memoryStorageProvider = new MemoryStorageProvider();

            var rawFileStorage = new RawStorageClient(fileStorageProvider);
            var rawMemoryStorage = new RawStorageClient(memoryStorageProvider);

            var fileStorage = new ObjectStorageClient(rawFileStorage, jsonSerializer);
            var memoryStorage = new ObjectStorageClient(rawMemoryStorage, jsonSerializer);

            Console.WriteLine("object memory storage");
            await TryStorage("test", memoryStorage);

            Console.WriteLine("object file storage");
            await TryStorage("test", fileStorage);

            Console.WriteLine("raw file storage");
            await TryRawStorage("test", rawFileStorage);

            Console.WriteLine("raw memory storage");
            await TryRawStorage("test", rawMemoryStorage);
        }

        private static async Task TryStorage(string key, ObjectStorageClient fileStorage)
        {
            await fileStorage.WriteAsync(key, new Model { Value = 5 });
            Model res = await fileStorage.ReadAsync<Model>(key);
            Console.WriteLine(res?.ToString() ?? "null"); ;
        }

        private static async Task TryRawStorage(string key, RawStorageClient fileStorage)
        {
            await fileStorage.WriteAsync(key, new byte[] { 1, 2, 3 });
            byte[] content = await fileStorage.ReadRawAsync(key);
            Console.WriteLine(string.Join(", ", content)); ;
        }
    }
}
