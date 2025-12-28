using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;

namespace Application.UseCases;

public class CancelPaymentUseCase(IPaymentRepository paymentRepository)
{
    public async Task<PaymentResponse?> Handle(Guid id)
    {
        var payment = await paymentRepository.GetByIdAsync(id);

        if (payment == null)
            return null;

        payment.Status = PaymentStatus.Cancelled;

        var result = await paymentRepository.UpdateAsync(payment);

        return new PaymentResponse(
            result.Id,
            result.ClientIdentifier,
            result.Amount,
            result.Description,
            result.CreatedAt,
            result.Status
        );
    }
}
