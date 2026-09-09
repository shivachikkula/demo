using DistrictPortal.Api.Data;
using DistrictPortal.Api.Data.Seed;
using DistrictPortal.Api.Hubs;
using DistrictPortal.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevCorsPolicy = "AngularDev";

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=districtportal.db"));

builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection("BlobStorage"));
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

builder.Services.AddSingleton<ISubmissionProcessingQueue, SubmissionProcessingQueue>();
builder.Services.AddHostedService<SubmissionProcessingWorker>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<ILeaRosterService, LeaRosterService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    SeedData.EnsureSeeded(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors(AngularDevCorsPolicy);
app.UseAuthorization();

app.MapControllers();
app.MapHub<FileProcessingHub>("/hubs/file-processing");

app.Run();
