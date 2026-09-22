using FileSystemMonitor.Api.Repositories;
using FileSystemMonitor.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ISnapshotRepository, JsonSnapshotRepository>();
builder.Services.AddScoped<IFileAnalyzer, FileAnalyzer>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Web", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Web");
app.MapControllers();

app.Run();
