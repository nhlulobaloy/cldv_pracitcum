using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace AgriStreamFunctions;

public class ProcessIrrigation
{
    private readonly ILogger<ProcessIrrigation> _logger;

    public ProcessIrrigation(ILogger<ProcessIrrigation> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ProcessIrrigation))]
    public void Run([QueueTrigger("irrigation-queue", Connection = "AzureWebJobsStorage")] string myQueueItem)
    {
        _logger.LogInformation($"Queue trigger function processed: {myQueueItem}");

        // Update SQL Database
        string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

        try
        {
            using (SqlConnection conn = new SqlConnection(sqlConnectionString))
            {
                conn.Open();
                // For demonstration, we just update the first user's role to simulate operational status change
                string query = "UPDATE Users SET Role = 'Irrigating' WHERE Username = 'farmer1'";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            _logger.LogInformation("SQL Database updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"SQL Error: {ex.Message}");
        }
    }
}