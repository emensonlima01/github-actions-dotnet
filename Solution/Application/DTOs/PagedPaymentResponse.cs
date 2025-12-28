namespace Application.DTOs;

public record PagedPaymentResponse(
    IReadOnlyList<PaymentResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
);
