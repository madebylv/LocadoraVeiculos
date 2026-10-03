using Microsoft.EntityFrameworkCore;
using LocadoraVeiculos.Data;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Banco de Dados SQL Server
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração dos Controllers com proteção contra ciclos de referência JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Locadora de Veículos API",
        Version = "v1",
        Description = "API RESTful para o sistema de gestão de locação de veículos (LocadoraVeiculos) - PUC Minas TADS.",
        Contact = new OpenApiContact
        {
            Name = "Equipe TADS - PUC Minas",
            Email = "suporte@locadoraveiculos.com.br"
        }
    });

    // Localiza e inclui o arquivo XML de documentação dos comentários
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Locadora de Veículos API v1");
    c.RoutePrefix = string.Empty; // Swagger UI na raiz da aplicação
    c.DocumentTitle = "Locadora de Veículos - Documentação da API (Swagger)";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
