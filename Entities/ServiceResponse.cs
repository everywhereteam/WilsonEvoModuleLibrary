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
}