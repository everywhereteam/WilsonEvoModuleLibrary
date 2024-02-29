namespace WilsonEvoModuleLibrary.Entities;

public record UpdateTaskData(string NodeId, string ModelType, string ChannelType, byte[]? data);