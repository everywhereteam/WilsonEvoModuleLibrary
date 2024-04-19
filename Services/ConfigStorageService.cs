using System;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Utility;

namespace WilsonEvoModuleLibrary.Services
{
    public interface IConfigStorageService
    {
        Result<T> GetConfiguration<T>(SessionData session);
    }

    internal class ConfigStorageService : IConfigStorageService
    {
        private readonly IMemoryCache _cache;

        public ConfigStorageService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Result<T> GetConfiguration<T>(SessionData session)
        {
            var key = $"{session.ProcessShortUrl}";
            if (_cache.TryGetValue(key, out T? value))
            {
                if (value != null)
                {
                    return Result.Ok(value);
                }
            }


            var raw = session.ChannelConfiguration;
            if (raw == null)
            {
                return Result.Fail($"The configuration is empty or null for the type: {typeof(T).Name}");
            }

            var obj = BinarySerialization.Deserialize<T>(raw, (settings) =>
            {
                settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
                settings.TypeNameHandling = TypeNameHandling.Auto;
            });
            if (obj == null)
            {
                return Result.Fail($"The configuration for the type: {typeof(T).Name} deserialized is null.");
            }
            _cache.Set(key, obj, TimeSpan.FromSeconds(30));
            return Result.Ok(obj);

        }
    }
}
