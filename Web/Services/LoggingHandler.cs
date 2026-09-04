using System.Text;

namespace Web.Services;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"--> {request.Method} {request.RequestUri}");

        foreach (var header in request.Headers)
        {
            sb.AppendLine($"    {header.Key}: {string.Join(", ", header.Value)}");
        }

        if (request.Content != null)
        {
            // Content-specific headers (Content-Type, etc.)
            foreach (var header in request.Content.Headers)
            {
                sb.AppendLine($"    {header.Key}: {string.Join(", ", header.Value)}");
            }

            var body = await request.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrEmpty(body))
            {
                sb.AppendLine($"    Body: {body}");
            }
        }

        _logger.LogInformation(sb.ToString());

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation($"<-- {(int)response.StatusCode} {response.ReasonPhrase} {request.RequestUri}");

        return response;
    }
}