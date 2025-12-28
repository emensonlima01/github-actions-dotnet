using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;

namespace Application.UseCases;

public class ReceivePaymentUseCase(IPaymentRepository paymentRepository)
{
    public async Task<string?> Handle(ReceivePaymentRequest request)
    {
        var existingPayment = await paymentRepository.GetByClientIdentifierAsync(request.ClientIdentifier);
        
        if (existingPayment != null)
        {
            return $"Payment with ClientIdentifier '{request.ClientIdentifier}' already exists";
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            ClientIdentifier = request.ClientIdentifier,
            Amount = request.Amount,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Received
        };

        await paymentRepository.SaveAsync(payment);
        return null;
    }
}
