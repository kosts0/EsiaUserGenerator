namespace EsiaUserGenerator.Logs;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var activity = System.Diagnostics.Activity.Current;

        _logger.LogInformation(
            "HTTP OUTGOING REQUEST {Method} {Url} | TraceId={TraceId} SpanId={SpanId}",
            request.Method,
            request.RequestUri,
            activity?.TraceId.ToString(),
            activity?.SpanId.ToString()
        );

        if (request.Content != null)
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Request Body: {Body}", body);
        }

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation(
            "HTTP OUTGOING RESPONSE {StatusCode} | TraceId={TraceId} SpanId={SpanId}",
            response.StatusCode,
            activity?.TraceId.ToString(),
            activity?.SpanId.ToString()
        );

        if (response.Content != null)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Response Body: {Body}", body);
        }

        return response;
    }
}