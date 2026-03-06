using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpsMonitor.Api.HostedServices;
using OpsMonitor.Api.Infrastructure;
using OpsMonitor.Api.Localization;
using OpsMonitor.Api.Middleware;
using OpsMonitor.Api.Options;
using OpsMonitor.Api.Security;
using OpsMonitor.Api.Services;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection("Security"));
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection("Seed"));
builder.Services.Configure<MonitoringOptions>(builder.Configuration.GetSection("Monitoring"));
builder.Services.AddSingleton<ITextLocalizer, TextLocalizer>();
builder.Services.AddSingleton<IApiErrorFactory, ApiErrorFactory>();

builder.Services.AddSingleton<ISqlSugarClient>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default")
                           ?? "Data Source=opsmonitor.db";
    return new SqlSugarClient(new ConnectionConfig
    {
        ConnectionString = connectionString,
        DbType = DbType.Sqlite,
        IsAutoCloseConnection = true,
        InitKeyType = InitKeyType.Attribute
    });
});

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
var key = Encoding.UTF8.GetBytes(jwt.SigningKey.PadRight(32, '0')[..32]);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                if (context.Response.HasStarted)
                {
                    return;
                }

                context.HandleResponse();
                var errorFactory = context.HttpContext.RequestServices.GetRequiredService<IApiErrorFactory>();
                var payload = errorFactory.Create(context.HttpContext, ErrorCodes.Auth.Unauthorized);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            },
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted)
                {
                    return;
                }

                var errorFactory = context.HttpContext.RequestServices.GetRequiredService<IApiErrorFactory>();
                var payload = errorFactory.Create(context.HttpContext, ErrorCodes.Auth.Forbidden);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient("probe");
builder.Services.AddHttpClient<IDingTalkNotifier, DingTalkNotifier>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IConfigEncryptionService, ConfigEncryptionService>();
builder.Services.AddScoped<IDbBootstrapper, DbBootstrapper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMonitorService, MonitorService>();
builder.Services.AddScoped<IChannelService, ChannelService>();
builder.Services.AddScoped<IProbeService, ProbeService>();
builder.Services.AddScoped<IAlertEngineService, AlertEngineService>();
builder.Services.AddScoped<IAlertQueryService, AlertQueryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IProbeDispatchQueue, ProbeDispatchQueue>();

builder.Services.AddHostedService<SchedulerHostedService>();
builder.Services.AddHostedService<ProbeWorkerHostedService>();
builder.Services.AddHostedService<RetentionHostedService>();

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errorFactory = context.HttpContext.RequestServices.GetRequiredService<IApiErrorFactory>();
            var payload = errorFactory.Create(context.HttpContext, ErrorCodes.Common.InvalidRequest);
            return new BadRequestObjectResult(payload);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpsMonitor API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Input: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var bootstrapper = scope.ServiceProvider.GetRequiredService<IDbBootstrapper>();
    await bootstrapper.InitializeAsync();
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseMiddleware<AuditMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
