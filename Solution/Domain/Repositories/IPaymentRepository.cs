using Domain.Entities;
using Domain.Models;

namespace Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment> SaveAsync(Payment payment);
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByClientIdentifierAsync(string clientIdentifier);
    Task<Payment> UpdateAsync(Payment payment);
    Task<PagedResult<Payment>> GetPagedAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize);
}
