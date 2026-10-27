using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OrderService.Configurations;
using OrderService.Consumers;
using OrderService.CQRS.Commands.Requests;
using OrderService.Infrastructure;
using Serilog;

// Konfiqurasiya oxunmamışdan əvvəl baş verən xətaları da yazmaq üçün müvəqqəti logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));


    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Login-dən aldığın JWT token-i daxil et"
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderRequest).Assembly));
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
    builder.Services.AddAuthorization();

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<OrderCreatedConsumer>();

        x.AddEntityFrameworkOutbox<AppDbContext>(o =>
        {
            o.UsePostgres(); // PostgreSQL üçün kilid sorğuları
            o.UseBusOutbox(); // Publish birbaşa RabbitMQ-ya yox, outbox-a yazır
            o.QueryDelay = TimeSpan.FromSeconds(1); // fon xidməti outbox-u saniyədə bir yoxlayır
        });

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"] ?? "admin");
                h.Password(builder.Configuration["RabbitMq:Password"] ?? "Admin123");
            });

            cfg.UseMessageRetry(r =>
            {
                r.Interval(3, TimeSpan.FromSeconds(2)); // 3 retry, aralarında 2 saniyə
                r.Ignore<ArgumentException>(); // bu xəta retry olunmur, birbaşa _error-a gedir
            });

            cfg.ConfigureEndpoints(context); // consumer üçün queue-nu avtomatik qurur
        });
    });

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "OrderService:"; // bütün açarların önünə prefiks əlavə olunur
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseExceptionHandler(); // ən əvvəl: sonrakı bütün middleware-lərin xətalarını tutur
    app.UseSerilogRequestLogging(); // hər sorğu üçün bir log sətri: metod, yol, status, müddət

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "OrderService failed to start");
}
finally
{
    Log.CloseAndFlush();
}