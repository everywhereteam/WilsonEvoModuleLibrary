namespace WilsonEvoModuleLibrary.Entities
{
    public class ServiceResponse
    {
        public SessionData SessionData { get; set; }

        public ServiceResponse() { }
        public ServiceResponse(SessionData sessionData)
        {
            SessionData = sessionData;
        }
    }
}