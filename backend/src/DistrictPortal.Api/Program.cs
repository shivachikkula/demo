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
        // Browsers treat localhost and 127.0.0.1 as different origins, so both are
        // allowed here in case the Angular dev server is opened via either.
        .WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
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

// No UseHttpsRedirection(): Angular is explicitly configured to call
// http://localhost:5080 (see src/environments/environment.ts), and this API has no
// HTTPS deployment target to redirect to. Redirecting would send the browser to a
// different origin (and an untrusted local dev certificate), which it then reports
// as a CORS failure even though the redirect - not CORS policy - is the actual cause.
// This was confirmed by seeing 307s on every API/negotiate request in DevTools.
app.UseCors(AngularDevCorsPolicy);
app.UseAuthorization();

app.MapControllers();
app.MapHub<FileProcessingHub>("/hubs/file-processing");

app.Run();
