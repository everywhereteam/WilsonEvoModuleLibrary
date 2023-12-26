using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace WilsonEvoModuleLibrary.Utility;

public static class BinarySerialization
{
    public static byte[]? Serialize<T>(T data)
    {
        using var ms = new MemoryStream();
        using var writer = new BsonDataWriter(ms);
        var serializer = new JsonSerializer();
        serializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        serializer.Serialize(writer, data);
        return ms.ToArray();
    }

    public static T? Deserialize<T>(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BsonDataReader(ms);
        var serializer = new JsonSerializer();
        serializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        return serializer.Deserialize<T>(reader);
    }

    public static object? Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BsonDataReader(ms);
        var serializer = new JsonSerializer();
        serializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        return serializer.Deserialize(reader);
    }
}