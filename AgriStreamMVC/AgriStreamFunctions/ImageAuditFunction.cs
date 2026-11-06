using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AgriStreamFunctions;

public class ImageAuditFunction
{
    private readonly ILogger<ImageAuditFunction> _logger;

    public ImageAuditFunction(ILogger<ImageAuditFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ImageAuditFunction))]
    public async Task Run([BlobTrigger("drone-images/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name)
    {
        _logger.LogInformation($"Blob trigger function processed blob\n Name: {name} \n Size: {stream.Length} Bytes");

        // Write to Table Storage
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var tableClient = new TableClient(connectionString, "TelemetryLogs");
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity("ImageAudit", name)
        {
            { "FileName", name },
            { "Size", stream.Length },
            { "Status", "Processed" },
            { "ProcessedAt", DateTime.UtcNow }
        };

        await tableClient.AddEntityAsync(entity);
    }
}