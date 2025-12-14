using System.Net.Http.Json;
using System.Text.Json;

namespace EsiaUserMfe.Utils;

public class CustomJsonHandler : DelegatingHandler
{
    private readonly JsonSerializerOptions _options;

    public CustomJsonHandler(JsonSerializerOptions options)
    {
        _options = options;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is JsonContent jsonContent)
        {
            var raw = await jsonContent.ReadFromJsonAsync<object>();
            request.Content = JsonContent.Create(raw, options: _options);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}