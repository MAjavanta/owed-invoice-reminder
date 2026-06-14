namespace OwedInvoiceReminder.API.DTOs;

public record CreateInvoiceRequestDTO(
    string ClientName,
    string ClientEmail,
    string InvoiceRef,
    int AmountPence,
    DateOnly DueDate
);