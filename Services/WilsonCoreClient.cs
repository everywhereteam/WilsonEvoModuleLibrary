using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WilsonEvoModuleLibrary.Services;

public class WilsonCoreClient
{
    private readonly HttpClient _httpClient;

    public WilsonCoreClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<R> Start<R>(object channel, SessionData data, CancellationToken token = default)
    {
        data.ChannelType = channel.GetType().AssemblyQualifiedName;
        var postData = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://localhost:7080/api/flow/start", postData, token);
        // response.EnsureSuccessStatusCode();

        // var response = await _httpClient.PostAsJsonAsync("https://localhost:7080/api/flow/start", data, token);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(token);
        var obj = JsonConvert.DeserializeObject<SessionData>(responseBody);
        return (R) obj.Response;
    }

    public async Task<SessionData> Next(string sessionId, object response, CancellationToken token = default)
    {
        var result = await _httpClient.PostAsJsonAsync($"/next/{sessionId}", response, token);

        return null; //todo
    }

    public async Task<T?> GetServiceConfiguration<T>(SessionData session, CancellationToken token = default)
    {
        var result =
            await _httpClient.GetFromJsonAsync<T?>($"/configuration/{session.ProcessVersion}/{typeof(T).FullName}",
                token);

        return result; //todo
    }
}