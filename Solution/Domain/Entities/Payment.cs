namespace Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public string ClientIdentifier { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public PaymentStatus Status { get; set; }
}

public enum PaymentStatus
{
    Pending,
    Received,
    Cancelled
}
