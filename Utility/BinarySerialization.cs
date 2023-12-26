﻿using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace WilsonEvoModuleLibrary.Utility;

public static class BinarySerialization
{
    public static byte[]? Serialize<T>(T data, Action<JsonSerializer>? settings = null)
    {
        using var ms = new MemoryStream();
        using var writer = new BsonDataWriter(ms);
        var serializer = new JsonSerializer();
        serializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        settings?.Invoke(serializer);
        serializer.Serialize(writer, data);
        return ms.ToArray();
    }

    public static T? Deserialize<T>(byte[] data, Action<JsonSerializer>? settings = null)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BsonDataReader(ms);
        var serializer = new JsonSerializer();
        serializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        settings?.Invoke(serializer);
        return serializer.Deserialize<T>(reader);
    }

    public static object? Deserialize(byte[] data, Action<JsonSerializer>? settings = null)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BsonDataReader(ms);
        var serializer = new JsonSerializer();
        serializer.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        settings?.Invoke(serializer);
        return serializer.Deserialize(reader);
    }
}