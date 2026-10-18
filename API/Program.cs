using Abtracciones.Interfaces;
using Scalar.AspNetCore;
using DA;
using Flujo;
using Abtracciones.Modelos;
using Microsoft.EntityFrameworkCore;
using DA.Contexto;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


//Inyeccion de dependencias
builder.Services.AddScoped<IClienteDA, ClienteDA>();
builder.Services.AddScoped<IClienteFlujo, ClienteFlujo>();

var cadenaConexion = builder.Configuration.GetConnectionString("SistemaSoporte")??
    throw new InvalidOperationException("Falta la cadena de conexión para SistemaSoporte en la configuracion") ;

builder.Services.AddDbContext<SistemaSoporteContexto>(opciones =>
    opciones.UseSqlServer(cadenaConexion));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/min/clientes",(IClienteFlujo flujo) => Results.Ok(flujo.Obtener()));

app.MapGet("/min/clientes/{id:int}", (int id, IClienteFlujo flujo) =>
{
    var CLIENTE = flujo.ObtenerPorId(id);
    return CLIENTE is null ? Results.NotFound() : Results.Ok(CLIENTE);
});

app.MapPost("/min/clientes", (Cliente cliente, IClienteFlujo flujo) =>
{
    var creado = flujo.Agregar(cliente);
    return Results.Created($"/min/clientes/{creado.Id}", creado);
});

//// Endpoint para buscar clientes por nombre con paginación
//app.MapGet("/min/clientes/buscar", (string? nombre, int pagina, IClienteFlujo flujo) =>
//{
//    var q = flujo.Obtener().AsEnumerable();
//    if (!string.IsNullOrWhiteSpace(nombre))
//    //Filtro opcional por nombre, ignorando mayúsculas y minúsculas
//     q = q.Where(c => c.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));
//    //Paginación: 5 elementos por página
//    return Results.Ok(q.Skip((Math.Max(pagina, 1)-1) *5).Take(5));
//});


//Nos permite que "/" sirva wwwroot/index.html, wwwroot/default.html o wwwroot/default.htm como página de inicio del sitio web
app.UseDefaultFiles();

//Habilita al servidor los archivos estáticos que viven en wwwroot(WebRoot)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
