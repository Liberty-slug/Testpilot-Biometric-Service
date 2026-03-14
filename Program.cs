using BiometricService;
var builder = WebApplication.CreateBuilder(args);

// Load Mongo settings from appsettings.json
builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

// Register MongoService
builder.Services.AddSingleton<MongoService>(s =>
{
    var settings = builder.Configuration.GetSection("MongoSettings").Get<MongoSettings>()!;
    return new MongoService(settings.ConnectionString, settings.DatabaseName);
});

// Register template cache
builder.Services.AddSingleton<FingerprintTemplateStore>();

// Register FingerprintService
builder.Services.AddSingleton<FingerprintService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/students/with-fingerprints", async (MongoService mongoService) =>
{
    var students = await mongoService.GetStudentsWithFingerprintsAsync();
    return Results.Ok(students);
});

// Endpoint for Node.js to verify fingerprint
app.MapPost("/verify-fingerprint", async (FingerprintService fingerprintService, FingerprintRequest request) =>
{
    var matchedStudent = await fingerprintService.VerifyStudentAsync(request.Template);

    if (matchedStudent == null)
        return Results.NotFound(new { message = "No student matched" });

    return Results.Ok(matchedStudent);
});

app.Run();
