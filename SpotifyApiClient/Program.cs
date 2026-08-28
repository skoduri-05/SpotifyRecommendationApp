using SpotifyApiClient.Interfaces;
using SpotifyApiClient.Services;

var builder = WebApplication.CreateBuilder(args);

var _config = builder.Configuration;

// Setting up logger
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
// Allowed origins for CORS are read from configuration (appsettings). Falls back to localhost if not present.
var allowedOrigins = _config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:7032" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalhostPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
// Configure a named HttpClient for Spotify API calls. Authorization header will be set at startup.
builder.Services.AddHttpClient("SpotifyApi", client =>
{
    client.BaseAddress = new Uri("https://api.spotify.com/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("SpotifyApiClient/1.0");
});
builder.Services.AddScoped<ISpotifyApiService, SpotifyApiService>();
builder.Services.AddScoped<ISpotifyAuthService, SpotifyAuthService>();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SpotifyApiClient v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("LocalhostPolicy");

app.UseAuthorization();

app.MapControllers();

// Initialize Spotify auth (obtain an access token and set client header) before handling requests.
using (var scope = app.Services.CreateScope())
{
    var auth = scope.ServiceProvider.GetRequiredService<ISpotifyAuthService>();
    await auth.InitializeAsync();
}

app.Run();
