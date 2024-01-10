using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using WilsonEvoModuleLibrary.Entities;
using WilsonEvoModuleLibrary.Services.Core.Interfaces;

namespace WilsonEvoModuleLibrary.Services;

public sealed class NodeServiceMapper
{
    private readonly ModelsConfiguration _config;
    private readonly ServiceMappings _map;
    private readonly IServiceProvider _servicesProvider;

    public NodeServiceMapper(IServiceProvider service, ServiceMappings map, ModelsConfiguration config)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        _servicesProvider = service;
        _map = map;
        _config = config;
    }

    //private ModuleNodeDefinition GetDefinition(Type type)
    //{
    //    ModuleNodeDefinition definition = new ModuleNodeDefinition();
    //    foreach (PropertyInfo property in type.GetProperties())
    //    {
    //        DataTypeAttribute dataTypeAttribute = (DataTypeAttribute)((IEnumerable<object>)property.GetCustomAttributes(typeof(DataTypeAttribute), false)).First<object>();
    //        DisplayAttribute displayAttribute = (DisplayAttribute)((IEnumerable<object>)property.GetCustomAttributes(typeof(DisplayAttribute), false)).FirstOrDefault<object>();
    //        bool flag = (BlazorDynamicFormGenerator.ReadOnlyAttribute)((IEnumerable<object>)property.GetCustomAttributes(typeof(ReadOnlyAttribute), false)).FirstOrDefault<object>() != null;
    //        DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)((IEnumerable<object>)property.GetCustomAttributes(typeof(DefaultValueAttribute), false)).FirstOrDefault<object>();
    //        List<ValidationAttribute> list = property.GetCustomAttributes(typeof(ValidationAttribute), false).Cast<ValidationAttribute>().ToList<ValidationAttribute>();
    //        ModuleNodePropertyDefinition propertyDefinition = new ModuleNodePropertyDefinition()
    //        {
    //            Name = property.Name,
    //            DisplayName = displayAttribute?.Name,
    //            DataType = dataTypeAttribute?.DataType,
    //            CustomDataType = dataTypeAttribute?.CustomDataType,
    //            ValidationRules = list,
    //            ReadOnly = flag,
    //            DefaultValue = defaultValueAttribute?.Value
    //        };
    //        definition.PropertyDefinitions.Add(propertyDefinition);
    //    }
    //    return definition;
    //}


    public async Task<ServiceResponse> ExecuteService(ServiceRequest request)
    {
        var session = request.SessionData;

        var response = new ServiceResponse();
        var output = "ok";
            
        if (!_map.ServiceMap.TryGetValue(new MapPath(request.Type, request.SessionData.ChannelType), out var serviceType))
        {
            if (!_map.ServiceMap.TryGetValue(new MapPath(request.Type, string.Empty), out serviceType))
            {
                session.Exception = "Missing service from module.";
                session.ContinueExecution = false;
                session.WaitingCallback = false;
                session.IsFaulted = true;
            }               
        }
        if(serviceType != null)
        {
            using var scope = _servicesProvider.CreateScope();
            var service = scope.ServiceProvider.GetService(serviceType);
            //TODO: add verification for the type
            var node = await ReadSessionData(request, serviceType.GenericTypeArguments[0]);

            if (service is IExecutionService syncService)
            {
                await syncService.Execute(in node, ref session ,ref output);
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
        }

        session.CurrentOutput = output;
        response.SessionData = session;
        return response;
    }

    private async Task<object?> ReadSessionData(ServiceRequest request, Type? type)
    {
        var ms = new MemoryStream(request.NodeData);
        await using var reader = new BsonDataReader(ms);
        var o = (JObject)await JToken.ReadFromAsync(reader);
        return type != null ? o.ToObject(type) : null;
    }
}