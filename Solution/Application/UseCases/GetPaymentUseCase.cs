using Application.DTOs;
using Domain.Repositories;

namespace Application.UseCases;

public class GetPaymentUseCase(IPaymentRepository paymentRepository)
{
    public async Task<PaymentResponse?> Handle(Guid id)
    {
        var payment = await paymentRepository.GetByIdAsync(id);

        if (payment == null)
            return null;

        return new PaymentResponse(
            payment.Id,
            payment.ClientIdentifier,
            payment.Amount,
            payment.Description,
            payment.CreatedAt,
            payment.Status
        );
    }
}
