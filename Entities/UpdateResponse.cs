using System.Collections.Generic;

namespace WilsonEvoModuleLibrary.Entities
{

    public record UpdateResult(string Message, bool Succes);

    public record UpdateTaskData(string NodeId, string ModelType,string ChannelType, byte[]? data);

    public record UpdateRequest(string projectCode, List<UpdateTaskData> Tasks);
}
