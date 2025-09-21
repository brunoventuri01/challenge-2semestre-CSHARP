using Advisor.Api.Domain.Models;
using Advisor.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB (SQLite local na pasta do projeto)
builder.Services.AddDbContext<AdvisorDbContext>(opt =>
    opt.UseSqlite("Data Source=advisor.db"));

// JSON global: evita ciclo de referência e deixa identado
builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opt.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();

// garante a pasta de export
var outDir = Path.Combine(AppContext.BaseDirectory, "out");
Directory.CreateDirectory(outDir);


app.UseSwagger();
app.UseSwaggerUI();

// ---------- ENDPOINT DE EXEMPLO ----------
string[] summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();
    return forecast;
});
// ----------------------------------------

// ===================== CRUD CLIENTES ===========================
app.MapGet("/api/clientes", async (AdvisorDbContext db) =>
    await db.Clientes.ToListAsync());

app.MapGet("/api/clientes/{id:int}", async (AdvisorDbContext db, int id) =>
{
    var c = await db.Clientes.FindAsync(id);
    return c is null ? Results.NotFound() : Results.Ok(c);
});

app.MapPost("/api/clientes", async (AdvisorDbContext db, Cliente c) =>
{
    db.Clientes.Add(c);
    await db.SaveChangesAsync();
    return Results.Created($"/api/clientes/{c.Id}", c);
});

app.MapPut("/api/clientes/{id:int}", async (AdvisorDbContext db, int id, Cliente input) =>
{
    var c = await db.Clientes.FindAsync(id);
    if (c is null) return Results.NotFound();

    c.Nome = input.Nome;
    c.Perfil = input.Perfil;
    c.AporteInicial = input.AporteInicial;
    c.LiquidezDesejada = input.LiquidezDesejada;
    c.Objetivo = input.Objetivo;
    c.PrazoObjetivo = input.PrazoObjetivo;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/clientes/{id:int}", async (AdvisorDbContext db, int id) =>
{
    var c = await db.Clientes.FindAsync(id);
    if (c is null) return Results.NotFound();
    db.Clientes.Remove(c);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ===================== CRUD ATIVOS =============================
app.MapGet("/api/ativos", async (AdvisorDbContext db) =>
    await db.Ativos.ToListAsync());

app.MapPost("/api/ativos", async (AdvisorDbContext db, Ativo a) =>
{
    db.Ativos.Add(a);
    await db.SaveChangesAsync();
    return Results.Created($"/api/ativos/{a.Id}", a);
});

app.MapPut("/api/ativos/{id:int}", async (AdvisorDbContext db, int id, Ativo input) =>
{
    var a = await db.Ativos.FindAsync(id);
    if (a is null) return Results.NotFound();

    a.Codigo = input.Codigo;
    a.Nome = input.Nome;
    a.Classe = input.Classe;
    a.Risco = input.Risco;
    a.Liquidez = input.Liquidez;
    a.RetornoEsperado = input.RetornoEsperado;
    a.ESG = input.ESG;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/ativos/{id:int}", async (AdvisorDbContext db, int id) =>
{
    var a = await db.Ativos.FindAsync(id);
    if (a is null) return Results.NotFound();
    db.Ativos.Remove(a);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ===================== IMPORTAR ATIVOS (JSON) ==================
app.MapPost("/api/ativos/import", async (AdvisorDbContext db) =>
{
    var path = Path.Combine(AppContext.BaseDirectory, "Files", "assets.json");
    if (!System.IO.File.Exists(path))
        return Results.NotFound("Arquivo Files/assets.json não encontrado.");

    var json = await System.IO.File.ReadAllTextAsync(path);
    var ativos = JsonSerializer.Deserialize<List<Ativo>>(json) ?? new();
    if (ativos.Count == 0) return Results.BadRequest("Nenhum ativo encontrado no arquivo JSON.");

    db.Ativos.AddRange(ativos);
    await db.SaveChangesAsync();
    return Results.Ok(new { importados = ativos.Count });
});

// ============== EXPORT ATIVOS (JSON para arquivo + download) ==============
app.MapGet("/api/ativos/export/json", async (AdvisorDbContext db) =>
{
    var dados = await db.Ativos.OrderBy(a => a.Id).ToListAsync();

    var options = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true
    };
    var json = JsonSerializer.Serialize(dados, options);

    var filePath = Path.Combine(AppContext.BaseDirectory, "out", "ativos_export.json");
    await System.IO.File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

    return Results.File(Encoding.UTF8.GetBytes(json), "application/json", "ativos_export.json");
});

// ===================== CRUD MACROS =============================
app.MapGet("/api/macros", async (AdvisorDbContext db) =>
    await db.Macros.OrderByDescending(m => m.Vigencia).ToListAsync());

app.MapPost("/api/macros", async (AdvisorDbContext db, MacroVariavel m) =>
{
    db.Macros.Add(m);
    await db.SaveChangesAsync();
    return Results.Created($"/api/macros/{m.Id}", m);
});

app.MapPut("/api/macros/{id:int}", async (AdvisorDbContext db, int id, MacroVariavel input) =>
{
    var m = await db.Macros.FindAsync(id);
    if (m is null) return Results.NotFound();

    m.Selic = input.Selic;
    m.Inflacao = input.Inflacao;
    m.Cambio = input.Cambio;
    m.Vigencia = input.Vigencia;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/macros/{id:int}", async (AdvisorDbContext db, int id) =>
{
    var m = await db.Macros.FindAsync(id);
    if (m is null) return Results.NotFound();
    db.Macros.Remove(m);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ===================== CRUD CARTEIRAS ==========================
app.MapGet("/api/carteiras", async (AdvisorDbContext db) =>
    await db.Carteiras
        .Include(c => c.Posicoes)
        .ThenInclude(p => p.Ativo)
        .ToListAsync());

app.MapGet("/api/carteiras/{id:int}", async (AdvisorDbContext db, int id) =>
{
    var carteira = await db.Carteiras
        .Include(c => c.Posicoes)
        .ThenInclude(p => p.Ativo)
        .FirstOrDefaultAsync(c => c.Id == id);

    return carteira is null ? Results.NotFound() : Results.Ok(carteira);
});

app.MapPost("/api/carteiras", async (AdvisorDbContext db, Carteira carteira) =>
{
    if (!await db.Clientes.AnyAsync(c => c.Id == carteira.ClienteId))
        return Results.BadRequest("ClienteId inválido.");

    var ids = carteira.Posicoes.Select(p => p.AtivoId).Distinct().ToList();
    var valid = await db.Ativos.CountAsync(a => ids.Contains(a.Id));
    if (valid != ids.Count) return Results.BadRequest("Algum AtivoId é inválido.");

    var total = carteira.Posicoes.Sum(p => p.Percentual);
    if (total <= 0 || total > 1) return Results.BadRequest("Soma dos percentuais deve ser > 0 e ≤ 1.");

    db.Carteiras.Add(carteira);
    await db.SaveChangesAsync();
    return Results.Created($"/api/carteiras/{carteira.Id}", carteira);
});

app.MapPut("/api/carteiras/{id:int}", async (AdvisorDbContext db, int id, Carteira input) =>
{
    var carteira = await db.Carteiras
        .Include(c => c.Posicoes)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (carteira is null) return Results.NotFound();

    carteira.ClienteId = input.ClienteId;
    carteira.Explicacao = input.Explicacao;
    carteira.Posicoes = input.Posicoes;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/carteiras/{id:int}", async (AdvisorDbContext db, int id) =>
{
    var carteira = await db.Carteiras.FindAsync(id);
    if (carteira is null) return Results.NotFound();

    db.Carteiras.Remove(carteira);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ========== EXPORT CARTEIRAS (JSON para arquivo + download) ==========
app.MapGet("/api/carteiras/export/json", async (AdvisorDbContext db) =>
{
    var carteiras = await db.Carteiras
        .Include(c => c.Posicoes)
        .ThenInclude(p => p.Ativo)
        .OrderBy(c => c.Id)
        .ToListAsync();

    var options = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true
    };
    var json = JsonSerializer.Serialize(carteiras, options);

    var filePath = Path.Combine(AppContext.BaseDirectory, "out", "carteiras_export.json");
    await System.IO.File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

    return Results.File(Encoding.UTF8.GetBytes(json), "application/json", "carteiras_export.json");
});

// ========== EXPORT CARTEIRA (TXT para arquivo + download) ==========
app.MapGet("/api/carteiras/{id:int}/export/txt", async (AdvisorDbContext db, int id) =>
{
    var carteira = await db.Carteiras
        .Include(c => c.Posicoes)
        .ThenInclude(p => p.Ativo)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (carteira is null) return Results.NotFound("Carteira não encontrada.");

    var sb = new StringBuilder();
    sb.AppendLine("======== CARTEIRA =========");
    sb.AppendLine($"Id: {carteira.Id}");
    sb.AppendLine($"ClienteId: {carteira.ClienteId}");
    sb.AppendLine($"CriadaEm: {carteira.CriadaEm:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine($"Explicacao: {carteira.Explicacao}");
    sb.AppendLine();
    sb.AppendLine("Posicoes:");
    sb.AppendLine("AtivoId | Codigo        | Nome                 | Percentual");
    sb.AppendLine("--------+---------------+---------------------+-----------");

    foreach (var p in carteira.Posicoes.OrderBy(p => p.AtivoId))
    {
        var codigo = p.Ativo?.Codigo ?? "-";
        var nome   = p.Ativo?.Nome ?? "-";
        sb.AppendLine($"{p.AtivoId,7} | {codigo,-13} | {nome,-19} | {p.Percentual:P1}");
    }

    var soma = carteira.Posicoes.Sum(p => p.Percentual);
    sb.AppendLine("----------------------------------------------");
    sb.AppendLine($"Soma dos percentuais: {soma:P2}");

    var txt = sb.ToString();
    var fileName = $"carteira_{id}.txt";
    var filePath = Path.Combine(AppContext.BaseDirectory, "out", fileName);
    await System.IO.File.WriteAllTextAsync(filePath, txt, Encoding.UTF8);

    return Results.File(Encoding.UTF8.GetBytes(txt), "text/plain", fileName);
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
