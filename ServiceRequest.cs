namespace WilsonPluginModels
{
    public class ServiceRequest
    {
        public SessionData SessionData { get; set; }
        public byte[] NodeData { get; set; }
        public string Type { get; set; }
    }
}