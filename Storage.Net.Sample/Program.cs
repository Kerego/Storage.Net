using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Storage.Net.Core;
using Storage.Net.Serializers;
using Storage.Net.StorageProviders;
using System.Linq;
using System.Text;
using System.Threading;

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

            Console.WriteLine("Concurrent writes and reads");

            var tasks = Enumerable.Repeat(Enumerable.Range(1, 3).ToList(), 3)
                .SelectMany(x => x)
                .Select(x => TryRawStorage(x.ToString(), rawMemoryStorage));

            await Task.WhenAll(tasks);
        }

        private static async Task TryStorage(string key, ObjectStorageClient fileStorage)
        {
            Model model = new Model { Value = 5 };

            Console.WriteLine($"thread [{Thread.CurrentThread.ManagedThreadId}] wrote message: [{model}] to key [{key}].");

            await fileStorage.WriteAsync(key, model);

            model = await fileStorage.ReadAsync<Model>(key);

            Console.WriteLine($"thread [{Thread.CurrentThread.ManagedThreadId}] read message: [{model}] from key [{key}].");
        }

        private static async Task TryRawStorage(string key, RawStorageClient fileStorage)
        {
            string message = $"hello storage {key}";

            Console.WriteLine($"thread [{Thread.CurrentThread.ManagedThreadId}] wrote message: [{message}] to key [{key}].");

            byte[] encodedMessage = Encoding.UTF8.GetBytes(message);
            await fileStorage.WriteAsync(key, encodedMessage);

            byte[] content = await fileStorage.ReadRawAsync(key);
            string decodedMessage = Encoding.UTF8.GetString(content);

            Console.WriteLine($"thread [{Thread.CurrentThread.ManagedThreadId}] read message: [{message}] from key [{key}].");
        }
    }
}
