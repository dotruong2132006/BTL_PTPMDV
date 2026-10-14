using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.BLL.Services;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QuanLyNganQuyCaNhan.API.Shared.Middleware;
using QuanLyNganQuyCaNhan.API.Shared.Extensions;
using QuanLyNganQuyCaNhan.API.Shared.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var khoaBiMat =builder.Configuration["Jwt:Key"];
var issuer =builder.Configuration["Jwt:Issuer"];
var audience =builder.Configuration["Jwt:Audience"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Nh?p Access Token theo d?ng: Bearer {token}"
        }
    );

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type =
                                Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        }
    );
});

var chuoiKetNoi =builder.Configuration.GetConnectionString("KetNoiCSDL");

builder.Services.AddSingleton<DapperConnectionFactory>(new DapperConnectionFactory(chuoiKetNoi!));
builder.Services.AddHealthChecks().AddCheck<KetNoiCSDLHealthCheck>("database", timeout: TimeSpan.FromSeconds(6));
builder.Services.AddScoped<INhatKyAuditService, NhatKyAuditService>();
builder.Services.AddScoped<DanhMucRepository>();
builder.Services.AddScoped<IDanhMucHeThongService, DanhMucHeThongService>();
builder.Services.AddScoped<NhatKyAuditRepository>();
builder.Services.AddScoped<AuditLogMiddleware>();

builder.Services.AddScoped<NguoiDungRepository>();
builder.Services.AddScoped<INguoiDungService, NguoiDungService>();
builder.Services.AddScoped<MatKhauService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<HaiFaService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey =new SymmetricSecurityKey(Encoding.UTF8.GetBytes(khoaBiMat!)),
            ClockSkew = TimeSpan.Zero
        };
});


var app = builder.Build();
app.SuDungIpTuApiGateway(builder.Configuration);
app.UseRouting();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditLogMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready").RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

app.Run();
