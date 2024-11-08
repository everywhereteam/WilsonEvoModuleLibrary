﻿using System.Collections.Generic;

namespace WilsonEvoModuleLibrary.Entities;

public record UpdateResult(string Message, bool Succes);

public record UpdateTaskData(string NodeId, string ModelType, string ChannelType, byte[]? data);

public record UpdateEnvironmentData(string ShortUrl, List<UpdateTaskData> Tasks);

public record UpdateModuleResponse(List<UpdateEnvironmentData> Environments);