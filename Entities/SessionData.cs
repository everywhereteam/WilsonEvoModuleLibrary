using System.Collections.Generic;

namespace WilsonEvoModuleLibrary.Entities;
                     
public class SessionData
{
    public int ProcessId { get; set; }
    public int ProcessVersion { get; set; }
    public string SessionId { get; set; }
    public string CustomerId { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public bool Test { get; set; }
    public string ChannelType { get; set; }
    public string Output { get; set; }
    /// <summary>
    /// This is the data incoming from the channel controller.
    /// </summary>
    public byte[]? Request { get; set; }
    /// <summary>
    /// This is for the data going out from the channel controller.
    /// </summary>
    public byte[]? Response { get; set; }
    public string CurrentNodeId { get; set; }
    public string CurrentShortUrl { get; set; }
    public string CurrentOutput { get; set; }
    public string? CurrentPoolId { get; set; }
    public bool WaitingCallback { get; set; } = false;
    public bool ContinueExecution { get; set; } = true;
    public bool IsFaulted { get; set; } = false;
    public bool IsEndedCorrectly { get; set; } = false;
    /// <summary>
    /// This is true when the session has been already ended, and will not go forward.
    /// </summary>
    public bool ItWasAlreadyEnded { get; set; } = false;
    public string Exception { get; set; } = string.Empty;
    public Dictionary<string, object> VarData { get; set; } = new();
    /// <summary>
    /// This is a special structure to verify that the session is not looping.
    /// </summary>
    public Dictionary<string, int> OperationTracker { get; set; } = new();
    /// <summary>
    /// This contains the special information for the channel in use.
    /// </summary>
    public Dictionary<string, string> ChannelData { get; set; } = new();
    /// <summary>
    /// This contains the secrets for the service, it's personal for modules.
    /// </summary>
    public byte[]? ServiceSecrets { get; set; }


}