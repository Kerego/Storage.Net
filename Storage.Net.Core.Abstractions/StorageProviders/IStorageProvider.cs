using System;
using System.IO;
using System.Threading.Tasks;

namespace Storage.Net.Core.Abstractions.StorageProviders
{
    public interface IStorageProvider
    {
        bool IsThreadSafe { get; }
        Task<bool> ExistsAsync(string key);
        Task DeleteAsync(string key);
        Task CreateAsync(string key, Stream data);
        Task<Stream> ReadAsync(string key);
    }
}