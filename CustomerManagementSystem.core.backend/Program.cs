using CustomerManagementSystem.core.backend.Entities.AppDataContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories & Services Registration
builder.Services.AddScoped<CustomerManagementSystem.core.backend.Repositories.Interface.ICustomerRepository, CustomerManagementSystem.core.backend.Repositories.Implement.CustomerRepository>();
builder.Services.AddScoped<CustomerManagementSystem.core.backend.Services.Interface.ICustomerService, CustomerManagementSystem.core.backend.Services.Implement.CustomerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
