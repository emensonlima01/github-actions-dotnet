using Application.DTOs;
using Application.UseCases;
using Domain.Entities;
using Domain.Models;
using Domain.Repositories;
using Moq;

namespace UnitTest.UseCases;

public class GetPaymentsPagedUseCaseTests
{
    [Fact]
    public async Task Handle_ReturnsMappedPagedResponse()
    {
        var request = new GetPaymentsPagedRequest(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            PageNumber: 2,
            PageSize: 2);

        var pagedResult = new PagedResult<Payment>
        {
            Items =
            [
                new Payment
                {
                    Id = Guid.NewGuid(),
                    ClientIdentifier = "client-5",
                    Amount = 1.50m,
                    Description = "p1",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Status = PaymentStatus.Received
                },
                new Payment
                {
                    Id = Guid.NewGuid(),
                    ClientIdentifier = "client-6",
                    Amount = 2.50m,
                    Description = "p2",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    Status = PaymentStatus.Pending
                }
            ],
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = 5
        };

        var repoMock = new Mock<IPaymentRepository>(MockBehavior.Strict);
        repoMock
            .Setup(r => r.GetPagedAsync(request.StartDate, request.EndDate, request.PageNumber, request.PageSize))
            .ReturnsAsync(pagedResult);

        var useCase = new GetPaymentsPagedUseCase(repoMock.Object);

        var result = await useCase.Handle(request);

        Assert.Equal(request.PageNumber, result.PageNumber);
        Assert.Equal(request.PageSize, result.PageSize);
        Assert.Equal(pagedResult.TotalCount, result.TotalCount);
        Assert.Equal(pagedResult.TotalPages, result.TotalPages);
        Assert.Equal(pagedResult.HasPreviousPage, result.HasPreviousPage);
        Assert.Equal(pagedResult.HasNextPage, result.HasNextPage);
        Assert.Equal(pagedResult.Items.Count, result.Items.Count);
        Assert.All(result.Items, item => Assert.False(string.IsNullOrWhiteSpace(item.ClientIdentifier)));
    }
}
