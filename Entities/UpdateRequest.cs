using System.Collections.Generic;

namespace WilsonEvoModuleLibrary.Entities;

public record UpdateRequest(string projectCode, List<UpdateTaskData> Tasks);