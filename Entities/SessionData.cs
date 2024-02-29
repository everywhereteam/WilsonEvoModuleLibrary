using System;
using System.Collections.Generic;

namespace WilsonEvoModuleLibrary.Entities;

public class SessionData
{
    /// <summary>
    /// Iteration Data
    /// </summary>
    public string SessionId { get; set; }

    public int ProcessId { get; set; }
    public int ProcessVersion { get; set; } //can we remove this one?
    public string CurrentNodeId { get; set; }
    public string CurrentShortUrl { get; set; }
    public string CurrentOutput { get; set; }
    public string? CurrentPoolId { get; set; }

    public string CustomerId { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public bool Test { get; set; }
    public string ChannelType { get; set; }
    public string Output { get; set; }
    public byte[]? Request { get; set; }
    public byte[]? Response { get; set; }


    public bool WaitingCallback { get; set; }
    public bool ContinueExecution { get; set; } = true;
    public bool IsFaulted { get; set; }
    public bool IsEndedCorrectly { get; set; } = false;
    public bool ItWasAlreadyEnded { get; set; } = false;
    public string Exception { get; set; } = string.Empty;
    public Dictionary<string, object> VarData { get; set; } = new();
    public Dictionary<string, int> OperationTracker { get; set; } = new();
    public Dictionary<string, string> ChannelData { get; set; } = new();

    public byte[]? ServiceSecrets { get; set; }

    public List<SessionMessage> SessionMessages { get; set; } = new();

    public void SetError(string errorMsg)
    {
        WaitingCallback = false;
        IsFaulted = true;
        Exception = errorMsg;
        CurrentOutput = "error";
    }

    public void AddMessage(SessionMessageUser from, string message)
    {
        SessionMessages.Add(new SessionMessage
        {
            From = from,
            Message = message,
            Type = SessionMessageType.Message,
            Time = DateTime.Now,
            MessageObject = null
        });
    }
}