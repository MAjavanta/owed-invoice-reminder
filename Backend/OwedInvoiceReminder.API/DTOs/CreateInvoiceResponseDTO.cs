using OwedInvoiceReminder.API.Models.Enums;

namespace OwedInvoiceReminder.API.DTOs;

public record CreateInvoiceResponseDTO(
    int Id,
    string ClientName,
    string ClientEmail,
    string InvoiceRef,
    int AmountPence,
    DateOnly DueDate,
    InvoiceStatus Status
);