using SupportFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("SupportFlow")
    ?? throw new InvalidOperationException(
        "Connection string 'SupportFlow' not found."
        )
    );
    builder.Services.AddControllers();
    var app = builder.Build();
    app.MapControllers();
    app.Run();