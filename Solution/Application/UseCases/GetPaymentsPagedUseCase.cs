using Application.DTOs;
using Domain.Repositories;

namespace Application.UseCases;

public class GetPaymentsPagedUseCase(IPaymentRepository paymentRepository)
{
    public async Task<PagedPaymentResponse> Handle(GetPaymentsPagedRequest request)
    {
        var pagedResult = await paymentRepository.GetPagedAsync(
            request.StartDate,
            request.EndDate,
            request.PageNumber,
            request.PageSize
        );

        var paymentResponses = pagedResult.Items
            .Select(p => new PaymentResponse(
                p.Id,
                p.ClientIdentifier,
                p.Amount,
                p.Description,
                p.CreatedAt,
                p.Status
            ))
            .ToList();

        return new PagedPaymentResponse(
            paymentResponses,
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalCount,
            pagedResult.TotalPages,
            pagedResult.HasPreviousPage,
            pagedResult.HasNextPage
        );
    }
}
