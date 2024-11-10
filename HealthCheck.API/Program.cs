using HealthCheck.API.HealthChecks;
using HealthCheck.API.Models;
using HealthCheck.API.Persistence;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Data;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHealthChecks()
    .AddDbContextCheck<HealthCheckDbContext>()
    .AddSqlServer(connectionString, name: "Check SQL instance")
    .AddUrlGroup(new Uri("https://github.com/DevAlanM"), name: "GitHub DevAlanM");
    //.AddCheck<CustomHealthCheck>(name: "New Custom Check");

builder.Services.AddHealthChecksUI( options =>
{
    options.SetEvaluationTimeInSeconds(seconds: 5);
    options.MaximumHistoryEntriesPerEndpoint(maxValue: 10);
    options.AddHealthCheckEndpoint("API with UI HealthChecks", "/health");
}).AddInMemoryStorage(); 

builder.Services.AddDbContext<HealthCheckDbContext>(options =>
    options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(typeof(HealthCheckDbContext).Assembly.FullName)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "HealthCheck.Api",
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(x =>
    {
        x.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthCheck.Api");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    #region Add Better Response

    //ResponseWriter = async (context, report) =>
    //{
    //    context.Response.ContentType = "application/json";

    //    var response = new HealthCheckReponse
    //    {
    //        Status = report.Status.ToString(),
    //        HealthChecks = report.Entries.Select(x => new IndividualHealthCheckResponse
    //        {
    //            Component = x.Key,
    //            Status = x.Value.Status.ToString(),
    //            Description = x.Value.Description
    //        }),
    //        HealthCheckDuration = report.TotalDuration
    //    };

    //    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    //}

    #endregion

    #region Response UI

    Predicate = p => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse

    #endregion

});

app.UseHealthChecksUI(options => { 
    options.UIPath = "/dashboard"; 
    options.AddCustomStylesheet("style.css");
});

app.Run();
