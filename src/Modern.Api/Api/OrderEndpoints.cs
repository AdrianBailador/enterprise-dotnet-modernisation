using Modern.Api.Application;
using Modern.Api.Domain;

namespace Modern.Api.Api;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        group.MapGet("/", GetAllOrders);
        group.MapGet("/{id:int}", GetOrder);
        group.MapPost("/", CreateOrder);
        group.MapPatch("/{id:int}/process", AdvanceStatus);
        group.MapPatch("/{id:int}/complete", CompleteOrder);
        group.MapDelete("/{id:int}", CancelOrder);

        return app;
    }

    private static async Task<IResult> GetAllOrders(
        OrderService service, CancellationToken ct)
    {
        var orders = await service.GetAllOrdersAsync(ct);
        return Results.Ok(orders.Select(ToDto));
    }

    private static async Task<IResult> GetOrder(
        int id, OrderService service, CancellationToken ct)
    {
        var order = await service.GetOrderAsync(id, ct);
        return order is null ? Results.NotFound() : Results.Ok(ToDto(order));
    }

    private static async Task<IResult> CreateOrder(
        CreateOrderRequest request, OrderService service, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.CustomerName)] = ["Customer name is required."]
            });

        if (request.Amount <= 0)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Amount)] = ["Amount must be greater than zero."]
            });

        var order = await service.CreateOrderAsync(
            request.CustomerName,
            Money.Of(request.Amount, request.Currency ?? "EUR"),
            ct);

        return Results.CreatedAtRoute("GetOrder", new { id = order.Id }, ToDto(order));
    }

    private static async Task<IResult> AdvanceStatus(
        int id, OrderService service, CancellationToken ct)
    {
        var updated = await service.AdvanceStatusAsync(id, ct);
        return updated ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> CompleteOrder(
        int id, OrderService service, CancellationToken ct)
    {
        var completed = await service.CompleteOrderAsync(id, ct);
        return completed ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> CancelOrder(
        int id, OrderService service, CancellationToken ct)
    {
        var cancelled = await service.CancelOrderAsync(id, ct);
        return cancelled ? Results.NoContent() : Results.NotFound();
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id, o.CustomerName, o.Total.Amount, o.Total.Currency,
        o.Status.ToString(), o.CreatedAt);
}

public record CreateOrderRequest(string CustomerName, decimal Amount, string? Currency);

public record OrderDto(
    int Id,
    string CustomerName,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt);
