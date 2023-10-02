using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BlazorDynamicFormGenerator;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using WilsonEvoModuleLibrary.Utility;
using System.Linq;
using FluentResults;
using ReadOnlyAttribute = BlazorDynamicFormGenerator.ReadOnlyAttribute;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services;

public sealed class NodeServiceMapper
{
    private readonly Dictionary<ulong, Type> _services;
    private readonly Dictionary<string, Type> _servicesmap;
    private readonly IServiceProvider _servicesProvider;

    public NodeServiceMapper(IServiceProvider service)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        _servicesProvider = service;
        _services = new Dictionary<ulong, Type>();
        _servicesmap = new Dictionary<string, Type>();
        AddService(ModuleLoader.GetNodeService(AppDomain.CurrentDomain.GetAssemblies()));
    }

    public Dictionary<string,ModuleNodeDefinition> GetDefinitions()
    {
        var result = new Dictionary<string,ModuleNodeDefinition>();
        foreach (var  service in _services)
        {
            var interfaceService = ModuleLoader.GetNodeServiceInterface(service.Value);
            var args = interfaceService.GenericTypeArguments;
            result.Add(args[0].FullName, GetDefinition(service.Value));
        }

        return result;
    }

    private ModuleNodeDefinition GetDefinition(Type type)
    {
        ModuleNodeDefinition definition = new ModuleNodeDefinition();
        foreach (PropertyInfo property in type.GetProperties())
        {
            DataTypeAttribute dataTypeAttribute = (DataTypeAttribute)((IEnumerable<object>)property.GetCustomAttributes(typeof(DataTypeAttribute), false)).First<object>();
            DisplayAttribute displayAttribute = (DisplayAttribute)((IEnumerable<object>)property.GetCustomAttributes(typeof(DisplayAttribute), false)).FirstOrDefault<object>();
            bool flag = (BlazorDynamicFormGenerator.ReadOnlyAttribute)((IEnumerable<object>)property.GetCustomAttributes(typeof(ReadOnlyAttribute), false)).FirstOrDefault<object>() != null;
            DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)((IEnumerable<object>)property.GetCustomAttributes(typeof(DefaultValueAttribute), false)).FirstOrDefault<object>();
            List<ValidationAttribute> list = property.GetCustomAttributes(typeof(ValidationAttribute), false).Cast<ValidationAttribute>().ToList<ValidationAttribute>();
            ModuleNodePropertyDefinition propertyDefinition = new ModuleNodePropertyDefinition()
            {
                Name = property.Name,
                DisplayName = displayAttribute?.Name,
                DataType = dataTypeAttribute?.DataType,
                CustomDataType = dataTypeAttribute?.CustomDataType,
                ValidationRules = list,
                ReadOnly = flag,
                DefaultValue = defaultValueAttribute?.Value
            };
            definition.PropertyDefinitions.Add(propertyDefinition);
        }
        return definition;
    }


    public async Task<ServiceResponse> ExecuteService(ServiceRequest request)
    {
        var session = request.SessionData;
        var node = await ReadSessionData(request);
        var response = new ServiceResponse();
        var output = "ok";
        var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x=>x.GetTypes()).SingleOrDefault(x=>x.FullName == session.ChannelType);
        var serviceType = GetService(node.GetType(), type); 
        var service = serviceType is not null ? _servicesProvider.GetService(ModuleLoader.GetNodeServiceInterface(serviceType)) : null;
        if (service is IExecutionService syncService)
        {
            await syncService.Execute(in node, ref session, ref output);
        }
        else if (service is IAsyncExecutionService asyncService && !session.WaitingCallback)
        {
            await asyncService.Execute(in node, ref session, ref output);
            session.WaitingCallback = true;
            session.ContinueExecution = false;
        }
        else if (service is IAsyncExecutionService asyncServiceCallback && session.WaitingCallback)
        {
            await asyncServiceCallback.ExecuteCallback(in node, ref session, ref output);
            session.WaitingCallback = false;
        }
        else
        {
            //this is shit where i go?
            session.ContinueExecution = false;
            session.WaitingCallback = false;
            session.IsFaulted = true;
            session.Exception = "Module service not found.";
        }

        session.CurrentOutput = output;
        response.SessionData = session;
        return response;
    }

    private async Task<object?> ReadSessionData(ServiceRequest request)
    {                      
        var ms = new MemoryStream(request.NodeData);

        using var reader = new BsonDataReader(ms);
        var o = (JObject)await JToken.ReadFromAsync(reader);
        //we need a mapper here                                  
        if (!_servicesmap.TryGetValue(request.Type, out var getType))
            return null;
        if (getType != null) 
            return o.ToObject(getType);

        return null;   
    }

    private void AddService(IEnumerable<Type> services)
    {
        foreach (var type in services)
        {
            var interfaceService = ModuleLoader.GetNodeServiceInterface(type);
            var lookup = ulong.MaxValue;
            var args = interfaceService.GenericTypeArguments;
            if (args.Length == 1)
            {
                lookup = Decode(args[0].GetHashCode(), 0);
                if (!_servicesmap.TryAdd(args[0].Name, args[0]))
                    throw new Exception("NodeServiceMapper, possible collision on service type");
            }
            else if (args.Length == 2)
            {
                lookup = Decode(args[0].GetHashCode(), args[1].GetHashCode());
                if (!_servicesmap.TryAdd(args[0].Name, args[0]))
                    throw new Exception("NodeServiceMapper, possible collision on service type");
            }

            if (lookup == ulong.MaxValue) throw new Exception("NodeServiceMapper, error lookup id is invalid");

            if (!_services.TryAdd(lookup, type))
                throw new Exception("NodeServiceMapper, possible collision on service type");
            
        }
    }

    private Type? GetService(Type t, Type s)
    {
        var idSingle = Decode(t.GetHashCode(), 0);
        if (_services.TryGetValue(idSingle, out var type))
            return type;
        if (s == null) return null;
        var idMultiple = Decode(t.GetHashCode(), s.GetHashCode());
        if (_services.TryGetValue(idMultiple, out var mType))
            return mType;
        return null;
    }

    private ulong Decode(int x, int y)
    {
        var id = x > y ? (uint) y | ((ulong) x << 32) : (uint) x | ((ulong) y << 32);
        return id;
    }
}