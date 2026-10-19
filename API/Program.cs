using Abtracciones.Interfaces;
using Abtracciones.Modelos;
using API.Reglas;
using DA;
using DA.Contexto;
using Flujo;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();


builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("codigoTicket", typeof(API.Reglas.CodigoTicketConstraint));
});

// Inyección de dependencias
builder.Services.AddScoped<IClienteDA, ClienteDA>();
builder.Services.AddScoped<IClienteFlujo, ClienteFlujo>();
builder.Services.AddSingleton<ITicketDA, TicketDA>();
builder.Services.AddScoped<ITicketFlujo, TicketFlujo>();

var cadenaConexion = builder.Configuration.GetConnectionString("SistemaSoporte")
    ?? throw new InvalidOperationException("Falta la cadena de conexión para SistemaSoporte en la configuracion");

builder.Services.AddDbContext<SistemaSoporteContexto>(opciones =>
    opciones.UseSqlServer(cadenaConexion));


var app = builder.Build();

app.UseStatusCodePages();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseDefaultFiles();

// Habilita al servidor los archivos estáticos que viven en wwwroot (WebRoot)
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

#region Minimal API - Clientes

app.MapGet("/min/clientes", (IClienteFlujo flujo) => Results.Ok(flujo.Obtener()));

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

#endregion

#region Minimal API - Tickets

app.MapGet("/min/tickets/resumen", (ITicketFlujo ticketFlujo) =>
{
    return Results.Ok(ticketFlujo.ObtenerResumen());
})
.WithName("ObtenerResumenTickets")
.WithSummary("Devuelve el resumen de tickets por estado y prioridad")
.Produces(200);

app.MapPost("/min/tickets/{id:int}/reabrir", (int id, ITicketFlujo ticketFlujo) =>
{
    try
    {
        var ticket = ticketFlujo.Reabrir(id);
        return Results.Ok(ticket);
    }
    catch (Abtracciones.Excepciones.NotFoundException ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 404, title: "Not Found");
    }
    catch (Abtracciones.Excepciones.ReglaNegocio ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 409, title: "Conflict - Regla de Negocio");
    }
})
.WithName("ReabrirTicket")
.WithSummary("Reabre un ticket resuelto cambiándolo a EnProceso")
.Produces(200)
.ProducesProblem(404)
.ProducesProblem(409);

#endregion

app.Run();