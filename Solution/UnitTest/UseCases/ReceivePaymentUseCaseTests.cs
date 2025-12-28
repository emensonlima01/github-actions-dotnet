using Application.DTOs;
using Application.UseCases;
using Domain.Entities;
using Domain.Repositories;
using Moq;

namespace UnitTest.UseCases;

public class ReceivePaymentUseCaseTests
{
    [Fact]
    public async Task Handle_ReturnsError_WhenPaymentAlreadyExists()
    {
        var request = new ReceivePaymentRequest("client-1", 120.50m, "desc");
        var repoMock = new Mock<IPaymentRepository>(MockBehavior.Strict);
        repoMock
            .Setup(r => r.GetByClientIdentifierAsync(request.ClientIdentifier))
            .ReturnsAsync(new Payment { Id = Guid.NewGuid() });

        var useCase = new ReceivePaymentUseCase(repoMock.Object);

        var result = await useCase.Handle(request);

        Assert.NotNull(result);
        repoMock.Verify(r => r.GetByClientIdentifierAsync(request.ClientIdentifier), Times.Once);
        repoMock.Verify(r => r.SaveAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SavesPayment_WhenPaymentDoesNotExist()
    {
        var request = new ReceivePaymentRequest("client-2", 99.99m, "new");
        var repoMock = new Mock<IPaymentRepository>(MockBehavior.Strict);
        repoMock
            .Setup(r => r.GetByClientIdentifierAsync(request.ClientIdentifier))
            .ReturnsAsync((Payment?)null);
        repoMock
            .Setup(r => r.SaveAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        var useCase = new ReceivePaymentUseCase(repoMock.Object);

        var before = DateTime.UtcNow;
        var result = await useCase.Handle(request);
        var after = DateTime.UtcNow;

        Assert.Null(result);
        repoMock.Verify(r => r.SaveAsync(It.Is<Payment>(p =>
            p.ClientIdentifier == request.ClientIdentifier &&
            p.Amount == request.Amount &&
            p.Description == request.Description &&
            p.Status == PaymentStatus.Received &&
            p.CreatedAt >= before &&
            p.CreatedAt <= after
        )), Times.Once);
    }
}
