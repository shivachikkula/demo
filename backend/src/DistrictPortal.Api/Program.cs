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

// CORS must run before HTTPS redirection - otherwise a redirect response (e.g. when
// running the "https" launch profile, which also binds the http:// address Angular
// calls) goes out without Access-Control-Allow-Origin, and the browser reports that
// as a CORS failure even though redirection, not CORS policy, is the actual cause.
app.UseCors(AngularDevCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<FileProcessingHub>("/hubs/file-processing");

app.Run();
