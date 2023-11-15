using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Entities
{
    public class ServiceMappings
    {
        public ConcurrentDictionary<MapPath, Type> ServiceMap { get; set; } = new();

    }

    public struct MapPath
    {
        public string TaskType { get; }
        public string TaskChannel { get; }

        public MapPath(string taskType, string taskChannel)
        {
            TaskType = taskType;
            TaskChannel = taskChannel;
        }

        public override bool Equals(object obj)
        {
            return obj is MapPath key &&
                   TaskType == key.TaskType &&
                   TaskChannel == key.TaskChannel;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TaskType, TaskChannel);
        }
    }
}
