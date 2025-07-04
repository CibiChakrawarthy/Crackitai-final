using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var app = builder.Build();

var frontendPath = Path.Combine(builder.Environment.ContentRootPath, "..", "frontend");
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath)
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(frontendPath)
});

app.MapPost("/api/transcribe", async (HttpContext http, IHttpClientFactory httpClientFactory) =>
{
    if (!http.Request.HasFormContentType)
    {
        return Results.BadRequest();
    }

    var form = await http.Request.ReadFormAsync();
    if (!form.Files.Any())
    {
        return Results.BadRequest();
    }

    await using var stream = form.Files[0].OpenReadStream();

    var client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer YOUR_OPENAI_API_KEY");

    using var content = new MultipartFormDataContent();
    content.Add(new StreamContent(stream), "file", form.Files[0].FileName);
    content.Add(new StringContent("whisper-1"), "model");

    var response = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", content);
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<TranscribeResponse>();

    return Results.Json(new { text = result?.Text });
});

app.MapPost("/api/chat", async (HttpContext http, IHttpClientFactory httpClientFactory) =>
{
    var request = await http.Request.ReadFromJsonAsync<ChatRequest>();
    if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
    {
        return Results.BadRequest();
    }

    var client = httpClientFactory.CreateClient();
    // Replace YOUR_OPENAI_API_KEY with your key or load from configuration
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer YOUR_OPENAI_API_KEY");

    var openAiRequest = new
    {
        model = "gpt-3.5-turbo",
        messages = new[]
        {
            new { role = "user", content = request.Prompt }
        }
    };

    var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", openAiRequest);
    response.EnsureSuccessStatusCode();
    var completion = await response.Content.ReadFromJsonAsync<OpenAiResponse>();
    var content = completion?.Choices?[0]?.Message?.Content;

    return Results.Json(new { response = content });
});

app.Run();

record ChatRequest(string Prompt);
record OpenAiResponse(OpenAiChoice[] Choices);
record OpenAiChoice(OpenAiMessage Message);
record OpenAiMessage(string Content);
record TranscribeResponse(string Text);
