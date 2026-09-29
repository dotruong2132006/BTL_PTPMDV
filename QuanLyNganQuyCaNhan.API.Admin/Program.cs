using QuanLyNganQuyCaNhan.BLL.Interfaces;
using QuanLyNganQuyCaNhan.BLL.Services;
using QuanLyNganQuyCaNhan.DAL.Connections;
using QuanLyNganQuyCaNhan.DAL.Repositories;
using QuanLyNganQuyCaNhan.API.Admin.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

builder.Services.AddScoped<NguoiDungRepository>();
builder.Services.AddScoped<INguoiDungService, NguoiDungService>();
builder.Services.AddScoped<MatKhauService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<DatLaiMatKhauTokenRepository>();
builder.Services.AddScoped<IDatLaiMatKhauTokenService,DatLaiMatKhauTokenService>();
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
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();