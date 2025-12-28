using Application.UseCases;
using Domain.Entities;
using Domain.Repositories;
using Moq;

namespace UnitTest.UseCases;

public class GetPaymentUseCaseTests
{
    [Fact]
    public async Task Handle_ReturnsNull_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var repoMock = new Mock<IPaymentRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Payment?)null);

        var useCase = new GetPaymentUseCase(repoMock.Object);

        var result = await useCase.Handle(id);

        Assert.Null(result);
        repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsMappedResponse_WhenFound()
    {
        var id = Guid.NewGuid();
        var payment = new Payment
        {
            Id = id,
            ClientIdentifier = "client-3",
            Amount = 42.75m,
            Description = "paid",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Status = PaymentStatus.Received
        };
        var repoMock = new Mock<IPaymentRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(payment);

        var useCase = new GetPaymentUseCase(repoMock.Object);

        var result = await useCase.Handle(id);

        Assert.NotNull(result);
        Assert.Equal(payment.Id, result!.Id);
        Assert.Equal(payment.ClientIdentifier, result.ClientIdentifier);
        Assert.Equal(payment.Amount, result.Amount);
        Assert.Equal(payment.Description, result.Description);
        Assert.Equal(payment.CreatedAt, result.CreatedAt);
        Assert.Equal(payment.Status, result.Status);
    }
}
