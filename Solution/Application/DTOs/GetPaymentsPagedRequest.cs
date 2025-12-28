namespace Application.DTOs;

public record GetPaymentsPagedRequest(
    DateTime StartDate,
    DateTime EndDate,
    int PageNumber = 1,
    int PageSize = 10
);
