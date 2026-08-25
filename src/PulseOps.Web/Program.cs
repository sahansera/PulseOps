using System.Net.Mime;

const string Page = """
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>PulseOps</title>
  <style>
    :root { color-scheme: dark; font-family: Inter, system-ui, sans-serif; }
    body { margin: 0; background: #0b1020; color: #e7eaf0; }
    main { max-width: 900px; margin: 64px auto; padding: 0 24px; }
    h1 { font-size: 48px; margin-bottom: 8px; }
    p { color: #aab2c5; }
    .service { margin-top: 24px; padding: 20px; border: 1px solid #27304a; border-radius: 14px; background: #12192b; }
    .status { display: inline-block; padding: 4px 10px; border-radius: 999px; background: #242d43; font-size: 13px; }
    code { color: #9bdcff; }
  </style>
</head>
<body>
  <main>
    <h1>PulseOps</h1>
    <p>A small service reliability platform that will grow with the blog series.</p>
    <section id="services"><p>Loading services...</p></section>
  </main>
  <script>
    fetch('/api/services')
      .then(response => response.json())
      .then(services => {
        document.querySelector('#services').innerHTML = services.map(service => `
          <article class="service">
            <strong>${service.name}</strong>
            <p><code>${service.id}</code></p>
            <span class="status">${service.status}</span>
          </article>`).join('');
      })
      .catch(error => {
        document.querySelector('#services').innerHTML = `<p>Couldn't load services: ${error}</p>`;
      });
  </script>
</body>
</html>
""";

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddHttpClient("pulseops-api", client =>
{
    client.BaseAddress = new Uri("https+http://api");
});

var app = builder.Build();

app.MapGet("/", () => Results.Content(Page, MediaTypeNames.Text.Html));

app.MapGet("/api/services", async (IHttpClientFactory factory, CancellationToken cancellationToken) =>
{
    var client = factory.CreateClient("pulseops-api");
    using var response = await client.GetAsync("/services", cancellationToken);
    var content = await response.Content.ReadAsStringAsync(cancellationToken);

    return Results.Content(
        content,
        response.Content.Headers.ContentType?.ToString() ?? MediaTypeNames.Application.Json,
        statusCode: (int)response.StatusCode);
});

app.MapDefaultEndpoints();
app.Run();
