using System;

namespace WilsonEvoModuleLibrary.Entities;

public class SessionMessage
{
    public DateTime Time { get; set; }
    public SessionMessageUser From { get; set; }
    public SessionMessageType Type { get; set; }
    public string Message { get; set; }
    public byte[]? MessageObject { get; set; }
}