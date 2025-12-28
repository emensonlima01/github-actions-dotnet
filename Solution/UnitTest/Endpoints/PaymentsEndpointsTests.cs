using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Application.UseCases;
using Domain.Entities;
using Domain.Models;
using Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebApi.Endpoints;

namespace UnitTest.Endpoints;

public class PaymentsEndpointsTests
{
    private static async Task<(WebApplication app, HttpClient client, Mock<IPaymentRepository> repoMock)> CreateAppAsync()
    {
        var repoMock = new Mock<IPaymentRepository>(MockBehavior.Strict);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IPaymentRepository>(repoMock.Object);
        builder.Services.AddScoped<ReceivePaymentUseCase>();
        builder.Services.AddScoped<GetPaymentUseCase>();
        builder.Services.AddScoped<CancelPaymentUseCase>();
        builder.Services.AddScoped<GetPaymentsPagedUseCase>();

        var app = builder.Build();
        app.MapPaymentsEndpoints();
        await app.StartAsync();

        return (app, app.GetTestClient(), repoMock);
    }

    [Fact]
    public async Task ReceivePayment_ReturnsConflict_WhenPaymentExists()
    {
        var (app, client, repoMock) = await CreateAppAsync();
        await using var _ = app;

        var request = new ReceivePaymentRequest("client-7", 10.00m, "dup");
        repoMock
            .Setup(r => r.GetByClientIdentifierAsync(request.ClientIdentifier))
            .ReturnsAsync(new Payment { Id = Guid.NewGuid() });

        var response = await client.PostAsJsonAsync("/api/payments/receive", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        repoMock.Verify(r => r.SaveAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task ReceivePayment_ReturnsAccepted_WhenPaymentIsNew()
    {
        var (app, client, repoMock) = await CreateAppAsync();
        await using var _ = app;

        var request = new ReceivePaymentRequest("client-8", 50.00m, "new");
        repoMock
            .Setup(r => r.GetByClientIdentifierAsync(request.ClientIdentifier))
            .ReturnsAsync((Payment?)null);
        repoMock
            .Setup(r => r.SaveAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        var response = await client.PostAsJsonAsync("/api/payments/receive", request);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        repoMock.Verify(r => r.SaveAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task GetPayment_ReturnsNotFound_WhenMissing()
    {
        var (app, client, repoMock) = await CreateAppAsync();
        await using var _ = app;

        var id = Guid.NewGuid();
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Payment?)null);

        var response = await client.GetAsync($"/api/payments/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPayment_ReturnsOk_WhenFound()
    {
        var (app, client, repoMock) = await CreateAppAsync();
        await using var _ = app;

        var id = Guid.NewGuid();
        var payment = new Payment
        {
            Id = id,
            ClientIdentifier = "client-9",
            Amount = 21.00m,
            Description = "ok",
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Received
        };
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(payment);

        var response = await client.GetAsync($"/api/payments/{id}");
        var body = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(payment.Id, body!.Id);
    }

    [Fact]
    public async Task CancelPayment_ReturnsNotFound_WhenMissing()
    {
        var (app, client, repoMock) = await CreateAppAsync();
        await using var _ = app;

        var id = Guid.NewGuid();
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Payment?)null);

        var response = await client.PutAsync($"/api/payments/{id}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelPayment_ReturnsOk_WhenFound()
    {
        var (app, client, repoMock) = await CreateAppAsync();
        await using var _ = app;

        var id = Guid.NewGuid();
        var payment = new Payment { Id = id, Status = PaymentStatus.Pending };
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(payment);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);

        var response = await client.PutAsync($"/api/payments/{id}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        repoMock.Verify(r => r.UpdateAsync(It.Is<Payment>(p => p.Status == PaymentStatus.Cancelled)), Times.Once);
    }

    [Fact]
    public async Task GetPaymentsPaged_ReturnsBadRequest_WhenPageNumberInvalid()
    {
        var (app, client, _) = await CreateAppAsync();
        await using var _ = app;

        var response = await client.GetAsync("/api/payments?pageNumber=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetPaymentsPaged_ReturnsBadRequest_WhenPageSizeInvalid(int pageSize)
    {
        var (app, client, _) = await CreateAppAsync();
        await using var _ = app;

        var response = await client.GetAsync($"/api/payments?pageSize={pageSize}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPaymentsPaged_ReturnsBadRequest_WhenDatesInvalid()
    {
        var (app, client, _) = await CreateAppAsync();
        await using var _ = app;

        var response = await client.GetAsync("/api/payments?startDate=2025-12-10&endDate=2025-12-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPaymentsPaged_ReturnsOk_WhenRequestValid()
    {
        var (app, client, repoMock) = await CreateAppAsync();
        await using var _ = app;

        var now = DateTime.UtcNow;
        var paged = new PagedResult<Payment>
        {
            Items =
            [
                new Payment
                {
                    Id = Guid.NewGuid(),
                    ClientIdentifier = "client-10",
                    Amount = 10.00m,
                    Description = "ok",
                    CreatedAt = now.AddDays(-1),
                    Status = PaymentStatus.Received
                }
            ],
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1
        };

        repoMock
            .Setup(r => r.GetPagedAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 10))
            .ReturnsAsync(paged);

        var response = await client.GetAsync("/api/payments?pageNumber=1&pageSize=10");
        var body = await response.Content.ReadFromJsonAsync<PagedPaymentResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Single(body!.Items);
    }
}
