using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

namespace AgriStreamFunctions;

public class GetTelemetry
{
    private readonly ILogger _logger;

    public GetTelemetry(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GetTelemetry>();
    }

    [Function("GetTelemetry")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Getting telemetry data from Table Storage.");

        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableClient = new TableClient(connectionString, "TelemetryLogs");

        var response = req.CreateResponse(HttpStatusCode.OK);

        try
        {
            var entities = tableClient.Query<TableEntity>().Take(10).ToList(); // Get latest 10
            await response.WriteAsJsonAsync(entities);
        }
        catch
        {
            await response.WriteStringAsync("No telemetry data found.");
        }

        return response;
    }
}