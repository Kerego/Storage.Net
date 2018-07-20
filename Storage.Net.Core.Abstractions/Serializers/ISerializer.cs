using System.IO;

namespace Storage.Net.Core.Abstractions.Serializers
{
    public interface ISerializer
    {
        Stream Serialize(object obj);
        T Deserialize<T>(Stream data);
    }
}