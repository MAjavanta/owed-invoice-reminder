using OwedInvoiceReminder.API.DTOs;
using OwedInvoiceReminder.API.Models;
using OwedInvoiceReminder.API.Models.Enums;

namespace OwedInvoiceReminder.API.Endpoints;

public static class InvoicesEndpoint
{
    const string GET_INVOICE_ENDPOINT = "GetInvoice";
    private static readonly List<Invoice> _invoices = [
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
        var group = app.MapGroup("api/v1/invoices");

        group.MapGet("/", () => Results.Ok(_invoices));

        group.MapGet("/{id:int}", (int id) =>
        {
            var invoice = _invoices.Find(invoice => invoice.Id == id);
            return invoice is null
                ? Results.NotFound()
                : Results.Ok(invoice);
        })
        .WithName(GET_INVOICE_ENDPOINT);

        group.MapPost("/", (CreateInvoiceRequestDTO dto) =>
        {
            var maxIndex = _invoices.Max(invoice => invoice.Id);
            var invoice = new Invoice()
            {
                Id = maxIndex + 1,
                ClientName = dto.ClientName,
                ClientEmail = dto.ClientEmail,
                InvoiceRef = dto.InvoiceRef,
                AmountPence = dto.AmountPence,
                DueDate = dto.DueDate,
                Status = InvoiceStatus.Upcoming,
                DateCreated = DateOnly.FromDateTime(DateTime.Now)
            };
            _invoices.Add(invoice);
            return Results.CreatedAtRoute(
                GET_INVOICE_ENDPOINT, new { id = invoice.Id }, invoice
            );
        });
    }
}