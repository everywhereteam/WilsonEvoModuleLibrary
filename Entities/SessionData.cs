using System;
using System.Collections.Generic;

namespace WilsonEvoModuleLibrary.Entities;
                     
public class SessionData
{
    public string? Id { get; set; }
    public int ProcessId { get; set; }
    public int ProcessVersion { get; set; }
   
    public string UserLanguage { get; set; } = "it-IT";
    public string? CommunicationChannel { get; set; }
    public string? ProcessNodeOutput { get; set; }
    public string? ProcessNodeId { get; set; }
    public string ProcessShortUrl { get; set; }
    public string? ProcessCurrentOutput { get; set; }
    public string? ProcessPoolId { get; set; }
    public Dictionary<string, int> ProcessOperationTracker { get; set; } = new();
    //channel
    public string? ChannelCustomerId { get; set; }
    public string? ChannelSender { get; set; }
    public string? ChannelRecipient { get; set; }
    public byte[]? ChannelInitialRequest { get; set; }
    public byte[]? ChannelFinalResponse { get; set; }
    public byte[]? ChannelConfiguration { get; set; }
    
    //session
    public Dictionary<string, object> Data { get; set; } = new();
    public Dictionary<string, string> ChannelData { get; set; } = new();

    //status
    public bool IsAwaitingCallback { get; set; } = false;
    public bool ContinueExecution { get; set; } = true;
    public bool SessionEncounteredError { get; set; } = false;
    public bool SessionCompleted { get; set; } = false;
    public bool SessionPreviouslyEnded { get; set; } = false;
    public string SessionExceptionDetails { get; set; } = string.Empty;
    public int? SessionStepId { get; set; }
    //stats
    public bool IsTestSession { get; set; }
    public string? PreviousSessionId { get; set; }
    public List<SessionLog> SessionLogs { get; set; } = new();
    public List<SessionMessage> SessionMessages { get; set; } = new();
    public bool IsAwatingResponse { get; set; } = false;
}

public class SessionLog
{
    //public DateTime Date { get; set; } = DateTime.Now;
    public string Message { get; set; }
    public string Type { get; set; }
}


public class SessionMessage
{
    //public DateTime Date { get; set; } = DateTime.Now;
    public string Message { get; set; }
    public string User { get; set; }
}

public static class SessionHelper
{
    public static void Await(this SessionData session)
    {
        session.IsAwaitingCallback = true;
        session.ContinueExecution = false;
    }

    public static void Error(this SessionData session, string errorMsg)
    {
        session.SessionEncounteredError = true;
        session.SessionExceptionDetails = errorMsg;
    }

    public static void AddMessage(this SessionData session, string user, string message)
    {
        session.SessionMessages.Add(new SessionMessage(){Message = message, User = user});
    }

}