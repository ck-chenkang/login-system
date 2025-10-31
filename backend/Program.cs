using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backend
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			// CORS for local dev (Vite default ports)
			builder.Services.AddCors(options =>
			{
				options.AddDefaultPolicy(policy =>
					policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
			});

			// MySQL login logger
			builder.Services.AddSingleton<backend.Services.ILoginLogger, backend.Services.MySqlLoginLogger>();

			// JWT authentication
			var jwtSection = builder.Configuration.GetSection("Jwt");
			var jwtKey = jwtSection.GetValue<string>("Key");
			if (string.IsNullOrWhiteSpace(jwtKey))
			{
				throw new InvalidOperationException("Jwt:Key is not configured");
			}
			var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
			builder.Services
				.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = jwtSection.GetValue<string>("Issuer"),
						ValidAudience = jwtSection.GetValue<string>("Audience"),
						IssuerSigningKey = signingKey
					};
				});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			// For local testing keep HTTP; comment out HTTPS redirection
			// app.UseHttpsRedirection();

			// Ensure DB table exists on startup
			using (var scope = app.Services.CreateScope())
			{
				var loggerSvc = scope.ServiceProvider.GetRequiredService<backend.Services.ILoginLogger>();
				loggerSvc.EnsureTableAsync().GetAwaiter().GetResult();
			}

			app.UseCors();
			app.UseAuthentication();
			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
