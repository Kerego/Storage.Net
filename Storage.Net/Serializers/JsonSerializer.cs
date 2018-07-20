using System;
using System.IO;
using Newtonsoft.Json;
using Storage.Net.Core.Abstractions.Serializers;

namespace Storage.Net.Serializers
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer(JsonSerializerSettings settings)
        {
            this._settings = settings;
        }

        public T Deserialize<T>(Stream data)
        {
            using (var reader = new StreamReader(data))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var ser = Newtonsoft.Json.JsonSerializer.Create(_settings);
                return ser.Deserialize<T>(jsonReader);
            }
        }

        public Stream Serialize(object obj)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.CloseOutput = false;
                var ser = Newtonsoft.Json.JsonSerializer.Create(_settings);
                ser.Serialize(jsonWriter, obj);
                jsonWriter.FlushAsync();
            }

            stream.Position = 0;
            return stream;
        }
    }
}
