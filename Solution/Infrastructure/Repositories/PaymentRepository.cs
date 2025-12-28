using Domain.Entities;
using Domain.Models;
using Domain.Repositories;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IMongoCollection<Payment> _paymentsCollection;

    public PaymentRepository(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _paymentsCollection = database.GetCollection<Payment>(settings.Value.PaymentsCollectionName);
    }

    public async Task<Payment> SaveAsync(Payment payment)
    {
        await _paymentsCollection.InsertOneAsync(payment);
        return payment;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Payment>.Filter.Eq(p => p.Id, id);
        return await _paymentsCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Payment?> GetByClientIdentifierAsync(string clientIdentifier)
    {
        var filter = Builders<Payment>.Filter.Eq(p => p.ClientIdentifier, clientIdentifier);
        return await _paymentsCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Payment> UpdateAsync(Payment payment)
    {
        var filter = Builders<Payment>.Filter.Eq(p => p.Id, payment.Id);
        await _paymentsCollection.ReplaceOneAsync(filter, payment);
        return payment;
    }

    public async Task<PagedResult<Payment>> GetPagedAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
    {
        var filter = Builders<Payment>.Filter.And(
            Builders<Payment>.Filter.Gte(p => p.CreatedAt, startDate),
            Builders<Payment>.Filter.Lte(p => p.CreatedAt, endDate)
        );

        var totalCount = await _paymentsCollection.CountDocumentsAsync(filter);

        var items = await _paymentsCollection
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PagedResult<Payment>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = (int)totalCount
        };
    }
}
