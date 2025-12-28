namespace Application.DTOs;

public record ReceivePaymentRequest(
    string ClientIdentifier,
    decimal Amount,
    string Description
);
