namespace WilsonEvoModuleLibrary.Entities;

public class ServiceResponse
{
    public ServiceResponse()
    {
    }

    public ServiceResponse(SessionData sessionData)
    {
        SessionData = sessionData;
    }

    public SessionData SessionData { get; set; }

    public bool IsFailed { get; set; } = false;
    public string? ExceptionMessage { get; set; }
    

    //Metrics
    public long ExecutionTime { get; set; }

}