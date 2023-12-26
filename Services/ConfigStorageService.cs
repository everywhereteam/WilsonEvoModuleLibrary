using FluentResults;
using Microsoft.Extensions.Caching.Memory;
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
            var key = $"{session.CurrentShortUrl}.{typeof(T)}";
            if (_cache.TryGetValue(key, out T? value))
            {
                if (value != null)
                {
                    return Result.Ok(value);
                }
            }

            if (session.ServiceSecrets.ContainsKey(typeof(T).Name))
            {
                var raw = session.ServiceSecrets[typeof(T).Name];
                if (raw == null)
                {                                     
                    return Result.Fail($"The configuration is empty or null for the type: {typeof(T).Name}");
                }

                var obj = BinarySerialization.Deserialize<T>(raw);
                if (obj == null)
                {                                    
                    return Result.Fail($"The configuration for the type: {typeof(T).Name} deserialized is null.");
                }
                _cache.Set(key, obj);
                return Result.Ok(obj);
            }
            else
            {                                    
                return Result.Fail($"No configuration found for the type: {typeof(T).Name}");
            }
        }
    }
}
