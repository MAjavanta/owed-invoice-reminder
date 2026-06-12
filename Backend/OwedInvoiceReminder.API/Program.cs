
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var invoices = new List<Invoice>()
{
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
    },
};

app.MapGet("/invoices", () => Results.Ok(invoices));

app.Run();

public class Invoice
{
    public int Id { get; set; }
    public string ClientName { get; set; }
    public string ClientEmail { get; set; }
    public string InvoiceRef { get; set; }
    public int AmountPence { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly DateCreated { get; set; }
    public InvoiceStatus Status { get; set; }
}

public enum InvoiceStatus
{
    Upcoming,
    Overdue,
    Paid,
    Late
}