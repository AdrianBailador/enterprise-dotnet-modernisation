using Legacy.Api.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Legacy.Api.Services;

// ❌ Static class — untestable, no DI, no async, no resilience
public static class OrderService
{
    private static readonly string _connectionString =
        "Server=localhost;Database=OrdersDb;Trusted_Connection=True;";

    // ❌ Synchronous — blocks a thread for the entire DB round-trip
    public static Order? GetOrder(int orderId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open(); // blocks

            using var cmd = new SqlCommand("SELECT * FROM Orders WHERE OrderId = @id", conn);
            cmd.Parameters.AddWithValue("@id", orderId);

            using var reader = cmd.ExecuteReader(); // blocks
            if (!reader.Read()) return null;

            return new Order
            {
                OrderId    = reader.GetInt32("OrderId"),
                CustomerName = reader.GetString("CustomerName"),
                TotalAmount = reader.GetDecimal("TotalAmount"),
                StatusCode  = reader.GetInt32("StatusCode"),
                CreatedAt   = reader.GetDateTime("CreatedAt")
            };
        }
        catch (Exception ex)
        {
            // ❌ Swallowed exception — caller never knows what went wrong
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    // ❌ No retry, no circuit breaker, no timeout — one network blip fails the request
    public static bool UpdateStatus(int orderId, int statusCode)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand(
            "UPDATE Orders SET StatusCode = @status WHERE OrderId = @id", conn);
        cmd.Parameters.AddWithValue("@status", statusCode);
        cmd.Parameters.AddWithValue("@id", orderId);

        return cmd.ExecuteNonQuery() > 0;
    }
}
