﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using WilsonEvoModuleLibrary.Interfaces;
using WilsonEvoModuleLibrary.Utility;

namespace WilsonEvoModuleLibrary.Services;

public sealed class NodeServiceMapper
{
    private readonly Dictionary<ulong, Type> _services;
    private readonly IServiceProvider _servicesProvider;

    public NodeServiceMapper(IServiceProvider service)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        _servicesProvider = service;
        _services = new Dictionary<ulong, Type>();
        foreach (var assembly in assemblies)
        {
            var serviceTypes = ModuleLoader.GetNodeService(assembly);
            AddService(serviceTypes);
        }
    }

    public async Task<ServiceResponse> ExecuteService(ServiceRequest request)
    {
        var session = request.SessionData;
        var node = await ReadSessionData(request);
        var response = new ServiceResponse();
        var output = "ok";
        var type = Type.GetType(session.ChannelType);
        var serviceType = GetService(node.GetType(), type);
        var service = serviceType is not null ? _servicesProvider.GetService(serviceType) : null;
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

        session.CurrentOutput = output;
        response.SessionData = session;
        return response;
    }

    private async Task<INode?> ReadSessionData(ServiceRequest request)
    {                      
        var ms = new MemoryStream(request.NodeData);

        using var reader = new BsonDataReader(ms);
        var o = (JObject)await JToken.ReadFromAsync(reader);
        var getType = Type.GetType(request.Type);
        if (getType != null) 
            return (INode)o.ToObject(getType);

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
                lookup = Decode(args[0].GetHashCode(), 0);
            else if (args.Length == 2) lookup = Decode(args[0].GetHashCode(), args[1].GetHashCode());

            if (lookup == ulong.MaxValue) throw new Exception("NodeServiceMapper, error lookup id is invalid");

            if (!_services.TryAdd(lookup, interfaceService))
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