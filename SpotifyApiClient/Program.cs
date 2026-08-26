using SpotifyApiClient.Interfaces;
using SpotifyApiClient.Services;

var builder = WebApplication.CreateBuilder(args);

var _config = builder.Configuration;

// Setting up logger
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient("SpotifyApi", (HttpClient) =>
{
    var spotifyApiConfig = _config.GetSection("Spotify");

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

app.UseAuthorization();

app.MapControllers();

app.Run();
