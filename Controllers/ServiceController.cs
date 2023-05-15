using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using WilsonPluginInterface.Services;
using WilsonPluginModels;
using WilsonPluginModels.Interfaces;

namespace WilsonPluginInterface.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly NodeServiceMapper _mapper;

    public ServiceController(NodeServiceMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpPost("/execute")]
    public async Task<ServiceResponse> ExecAsync(ServiceRequest request)
    {
        INode node = null;
        var ms = new MemoryStream(request.NodeData);

        using (var reader = new BsonDataReader(ms))
        {
            var o = (JObject) await JToken.ReadFromAsync(reader);
            var getType = Type.GetType(request.Type);
            node = (INode) o.ToObject(getType);
        }

        return await _mapper.ExecuteService(node, request.SessionData);
    }

    [HttpGet("/info")]
    public IActionResult GetServiceInfo()
    {
        return Ok();
    }
}