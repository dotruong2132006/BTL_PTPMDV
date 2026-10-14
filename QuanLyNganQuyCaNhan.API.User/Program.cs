using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.BLL.Services;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QuanLyNganQuyCaNhan.API.Shared.Middleware;
using QuanLyNganQuyCaNhan.API.Shared.Extensions;
using QuanLyNganQuyCaNhan.API.User.Jobs;
using QuanLyNganQuyCaNhan.API.Shared.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var khoaBiMat = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddControllers();
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
                    Reference =new Microsoft.OpenApi.Models.OpenApiReference{Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,Id = "Bearer"}
                },
                Array.Empty<string>()
            }
        }
    );
});

var chuoiKetNoi =builder.Configuration.GetConnectionString("KetNoiCSDL");

builder.Services.AddSingleton<DapperConnectionFactory>(new DapperConnectionFactory(chuoiKetNoi!));
builder.Services.AddHealthChecks().AddCheck<KetNoiCSDLHealthCheck>("database", timeout: TimeSpan.FromSeconds(6));
builder.Services.AddScoped<ThongKeRepository>();
builder.Services.AddScoped<IThongKeService, ThongKeService>();
builder.Services.AddScoped<NhatKyAuditRepository>();
builder.Services.AddScoped<AuditLogMiddleware>();

builder.Services.AddScoped<NguoiDungRepository>();
builder.Services.AddScoped<INguoiDungService, NguoiDungService>();
builder.Services.AddScoped<MatKhauService>();

builder.Services.AddScoped<ViRepository>();
builder.Services.AddScoped<IViService, ViService>();
builder.Services.AddScoped<DanhMucRepository>();
builder.Services.AddScoped<IDanhMucService, DanhMucService>();
builder.Services.AddScoped<GiaoDichRepository>();
builder.Services.AddScoped<IGiaoDichService, GiaoDichService>();
builder.Services.AddScoped<NganSachRepository>();
builder.Services.AddScoped<INganSachService, NganSachService>();
builder.Services.AddScoped<NhacNhoRepository>();
builder.Services.AddScoped<INhacNhoService, NhacNhoService>();
builder.Services.AddScoped<GiaoDichFileService>();
builder.Services.AddScoped<BaoCaoThangRepository>();
builder.Services.AddScoped<IBaoCaoThangService, BaoCaoThangService>();
builder.Services.AddSingleton<BaoCaoPdfService>();
if (builder.Configuration.GetValue<bool>("BaoCao:BatWorker", true))
    builder.Services.AddHostedService<BaoCaoThangBackgroundService>();

if (builder.Configuration.GetValue<bool>("NhacNho:BatWorker", true))
    builder.Services.AddHostedService<NhacNhoBackgroundService>();

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
