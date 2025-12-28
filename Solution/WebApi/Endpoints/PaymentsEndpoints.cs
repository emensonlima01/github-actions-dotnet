using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.UseCases;

namespace WebApi.Endpoints;

public static class PaymentsEndpoints
{
    public static void MapPaymentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/payments")
            .WithTags("Payments");

        group.MapPost("/receive", ReceivePayment)
            .WithName("ReceivePayment");

        group.MapGet("/{id}", GetPayment)
            .WithName("GetPayment");

        group.MapPut("/{id}/cancel", CancelPayment)
            .WithName("CancelPayment");

        group.MapGet("/", GetPaymentsPaged)
            .WithName("GetPaymentsPaged");
    }

    private static async Task<IResult> ReceivePayment(
        [FromBody] ReceivePaymentRequest request,
        [FromServices] ReceivePaymentUseCase useCase)
    {
        var error = await useCase.Handle(request);
        
        if (error != null)
            return Results.Conflict(new { message = error });

        return Results.Accepted();
    }

    private static async Task<IResult> GetPayment(
        [FromRoute] Guid id,
        [FromServices] GetPaymentUseCase useCase)
    {
        var payment = await useCase.Handle(id);

        if (payment == null)
            return Results.NotFound(new { message = "Payment not found" });

        return Results.Ok(payment);
    }

    private static async Task<IResult> CancelPayment(
        [FromRoute] Guid id,
        [FromServices] CancelPaymentUseCase useCase)
    {
        var payment = await useCase.Handle(id);

        if (payment == null)
            return Results.NotFound(new { message = "Payment not found" });

        return Results.Ok(payment);
    }

    private static async Task<IResult> GetPaymentsPaged(
        [FromServices] GetPaymentsPagedUseCase useCase,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1)
            return Results.BadRequest(new { message = "Page number must be greater than 0" });

        if (pageSize < 1 || pageSize > 100)
            return Results.BadRequest(new { message = "Page size must be between 1 and 100" });

        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        if (start > end)
            return Results.BadRequest(new { message = "Start date must be before end date" });

        var request = new GetPaymentsPagedRequest(start, end, pageNumber, pageSize);
        var result = await useCase.Handle(request);

        return Results.Ok(result);
    }
}
