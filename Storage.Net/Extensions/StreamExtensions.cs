using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Storage.Net.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<MemoryStream> ToMemoryStreamAsync(this Stream stream)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            if (stream.CanSeek)
                stream.Position = 0;

            var memoryStream = new MemoryStream();
            // memoryStream.CopyToAsync is synchroneus, schedule work to another thread in that case

            if (stream is MemoryStream ms)
                await Task.Run(() => stream.CopyTo(memoryStream)).ConfigureAwait(false);
            else
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}