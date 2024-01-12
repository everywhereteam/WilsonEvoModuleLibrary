using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Entities
{

    public record UpdateResult(string Message, bool Succes);

    public record UpdateTaskData(string ModelType,string ChannelType, byte[]? data);

    public record UpdateRequest(List<UpdateTaskData> Tasks);
}
