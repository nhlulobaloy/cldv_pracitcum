using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;

namespace AgriStreamFunctions;

public class TriggerIrrigation
{
    private readonly ILogger _logger;

    public TriggerIrrigation(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TriggerIrrigation>();
    }

    [Function("TriggerIrrigation")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Adding irrigation command to Queue.");

        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var queueClient = new QueueClient(connectionString, "irrigation-queue");
        await queueClient.CreateIfNotExistsAsync();

        await queueClient.SendMessageAsync("StartIrrigation");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Irrigation command sent successfully.");
        return response;
    }
}