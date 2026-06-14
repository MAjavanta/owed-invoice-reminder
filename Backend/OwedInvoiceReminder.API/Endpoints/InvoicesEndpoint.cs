using OwedInvoiceReminder.API.Models;
using OwedInvoiceReminder.API.Models.Enums;

namespace OwedInvoiceReminder.API.Endpoints;

public static class InvoicesEndpoint
{
    private static readonly List<Invoice> invoices = [
        new()
        {
            Id = 1,
            ClientName = "Mansoor",
            ClientEmail = "mahmad743@yahoo.co.uk",
            InvoiceRef = "INV-01",
            AmountPence = 2999,
            DueDate = new DateOnly(2026,6,15),
            Status = InvoiceStatus.Upcoming,
            DateCreated = new DateOnly(2026,6,8)
        },

        new()
        {
            Id = 2,
            ClientName = "Misha",
            ClientEmail = "mahmad743@yahoo.co.uk",
            InvoiceRef = "INV-02",
            AmountPence = 9999,
            DueDate = new DateOnly(2026,6,5),
            Status = InvoiceStatus.Overdue,
            DateCreated = new DateOnly(2026,6,1)
        },
        new()
        {
            Id = 3,
            ClientName = "Evelyn",
            ClientEmail = "evelyn.jean.chong@yahoo.co.uk",
            InvoiceRef = "INV-03",
            AmountPence = 999,
            DueDate = new DateOnly(2026,05,15),
            Status = InvoiceStatus.Paid,
            DateCreated = new DateOnly(2026,05,08)
        },
        new()
        {
            Id = 4,
            ClientName = "Dan",
            ClientEmail = "mahmad743@yahoo.co.uk",
            InvoiceRef = "INV-04",
            AmountPence = 299,
            DueDate = new DateOnly(2026,05,05),
            Status = InvoiceStatus.Late,
            DateCreated = new DateOnly(2026,04,08)
        }
    ];

    public static void MapInvoicesEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("api/v1");

        group.MapGet("/invoices", () => Results.Ok(invoices));
    }
}