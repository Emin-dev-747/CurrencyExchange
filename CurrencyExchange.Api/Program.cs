using CurrencyExchange.Api.Extentsions;
using CurrencyExchange.Application.Abstractions.Caching;
using CurrencyExchange.Application.Caching;
using CurrencyExchange.Application.Contracts;
using CurrencyExchange.Application.Policies;
using CurrencyExchange.Application.Repositories;
using CurrencyExchange.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var config = builder.Configuration;

            string connectionString = config.GetConnectionString("Database") ??
                                      throw new ArgumentNullException(nameof(config));

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

            string connectionStringRedis = config.GetConnectionString("Cache") ??
                                      throw new ArgumentNullException(nameof(config));

            builder.Services.AddStackExchangeRedisCache(options => options.Configuration = connectionStringRedis);

            builder.Services.AddSingleton<ICacheService, CacheService>();
            builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            builder.Services.AddSingleton(new ClientPolicy());

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                //app.ApplyMigrations();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/CurrencyExcangeRates", async(ICurrencyRepository repo,string defaultCurrency, CancellationToken ct) =>
            {
                return await repo.GetCacheOfCurrenciesAsync(defaultCurrency, ct);
            })
            .WithOpenApi();

            app.Run();
        }
    }
}
