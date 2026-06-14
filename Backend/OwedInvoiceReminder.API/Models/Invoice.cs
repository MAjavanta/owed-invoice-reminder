using OwedInvoiceReminder.API.Models.Enums;

namespace OwedInvoiceReminder.API.Models;

public class Invoice
{
    public int Id { get; set; }
    public required string ClientName { get; set; }
    public required string ClientEmail { get; set; }
    public string InvoiceRef { get; set; } = string.Empty;
    public int AmountPence { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly DateCreated { get; set; }
    public InvoiceStatus Status { get; set; }
}
