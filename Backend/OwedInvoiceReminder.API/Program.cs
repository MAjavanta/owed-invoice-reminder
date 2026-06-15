using OwedInvoiceReminder.API.Endpoints;

const string CORS_POLICY_WEB_FRONTEND = "Cors_Policy_Web";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(name: CORS_POLICY_WEB_FRONTEND,
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
});

var app = builder.Build();

/* // Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
} */

app.UseCors(CORS_POLICY_WEB_FRONTEND);


app.MapInvoicesEndpoint();

app.Run();
