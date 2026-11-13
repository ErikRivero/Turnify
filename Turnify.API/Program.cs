using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Turnify.Core.Data;
using Turnify.Core.Entities;
using Turnify.Core.Repositories;
using Turnify.Core.Services;
using Turnify.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Turnify.API.Services;
using Microsoft.OpenApi.Models;
namespace Turnify.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<TurnifyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TurnifyDB")
        ?? throw new InvalidOperationException(
            "No se encontró la conexión TurnifyDB.")));
            // Add services to the container.
            builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
            builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
            builder.Services.AddScoped<IBarberoRepository, BarberoRepository>();
            builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
            builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<ServicioService>();
            builder.Services.AddScoped<ClienteService>();
            builder.Services.AddScoped<BarberoService>();
            builder.Services.AddScoped<TurnoService>();
            builder.Services.AddScoped<DashboardService>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Turnify API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Pegá solamente el token JWT, sin escribir Bearer."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });
            builder.Services.AddScoped<
    IPasswordHasher<Usuario>,
    PasswordHasher<Usuario>>();

            builder.Services.AddScoped<AuthService>();
            var claveConfigurada = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Falta configurar Jwt:Key en los secretos de usuario.");

            var claveBytes = Convert.FromBase64String(claveConfigurada);

            if (claveBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "La clave JWT debe tener al menos 32 bytes.");
            }

            var claveFirma = new SymmetricSecurityKey(claveBytes);

            builder.Services.AddSingleton(new JwtService(claveFirma));

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = JwtService.Emisor,

                        ValidateAudience = true,
                        ValidAudience = JwtService.Audiencia,

                        ValidateLifetime = true,
                        RequireExpirationTime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = claveFirma,

                        RequireSignedTokens = true,
                        ValidAlgorithms = new[]
                        {
                SecurityAlgorithms.HmacSha256
                        },

                        NameClaimType = "unique_name",
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                {
                    policy.WithOrigins("http://127.0.0.1:5500")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();

                await UsuarioInicializador.CrearAsync(
                    scope.ServiceProvider,
                    app.Configuration);

                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("Frontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
