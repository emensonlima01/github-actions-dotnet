using Application.UseCases;
using Domain.Entities;
using Domain.Repositories;
using Moq;

namespace UnitTest.UseCases;

public class CancelPaymentUseCaseTests
{
    [Fact]
    public async Task Handle_ReturnsNull_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var repoMock = new Mock<IPaymentRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Payment?)null);

        var useCase = new CancelPaymentUseCase(repoMock.Object);

        var result = await useCase.Handle(id);

        Assert.Null(result);
        repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CancelsPayment_WhenFound()
    {
        var id = Guid.NewGuid();
        var payment = new Payment
        {
            Id = id,
            ClientIdentifier = "client-4",
            Amount = 11.00m,
            Description = "to cancel",
            CreatedAt = DateTime.UtcNow.AddMinutes(-1),
            Status = PaymentStatus.Pending
        };
        var repoMock = new Mock<IPaymentRepository>(MockBehavior.Strict);
        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(payment);
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync((Payment p) => p);

        var useCase = new CancelPaymentUseCase(repoMock.Object);

        var result = await useCase.Handle(id);

        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Cancelled, result!.Status);
        repoMock.Verify(r => r.UpdateAsync(It.Is<Payment>(p => p.Status == PaymentStatus.Cancelled)), Times.Once);
    }
}
