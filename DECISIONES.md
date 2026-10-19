**`DECISIONES.md`**
# Bitácora de Decisiones de Diseño y Arquitectura

## 1. Variante Personal
- **Nombre:** Héctor Méndez Guerrero
- **Variante asignada (v):** 1
- - **Tamaño máximo de página:** 25
- **Formato de código:** INC-NNNNNN
- **Regla especial (409):** No se permite cambiar la prioridad a 'Baja' si el ticket se encuentra en estado 'EnProceso'.

## 2. El ClienteId enviado al crear un ticket no existe: ¿responde 400, 404 o 422?

Responde 400 porque la petición enviada en el JSON contiene un valor que viola las reglas de validación de negocio, 404 y 422 no porque se reserva para cuando la URL solicitada en el endpoint no existe en el servidor

## 3.Un id como abc en /api/tickets/abc: ¿por qué la restricción de ruta responde 404 mientras que un body inválido responde 400? ¿Cuándo usaría cada mecanismo?

Porque no cumple con el formato dado de INC-NNNNNN y el 400 es porque a petición llegó al endpoint, pero su contenido es inválido.

## 4.¿Cómo comunica Flujo un error de regla de negocio al API sin depender de HTTP? Compare al menos dos alternativas (por ejemplo, excepciones personalizadas y un objeto de resultado), diga cuál eligió y qué costo tiene.

Se comunica porque la capa de Flujo detecta el incumplimiento de la regla y lanza la excepción. Luego, el controlador en la capa API la atrapa en un bloque try-catch y la traduce al código de estado HTTP correspondiente, Las excepciones en .NET tienen un costo pequeño de rendimiento al capturar el stack trace y el Objeto de Resultado no se usó porque la capa de Flujo no lanza excepciones, sino que devuelve un resultado que el controlador examina mediante condicionales

## 5. Paginación: ¿por qué limitar el tamaño máximo de página? ¿Qué devuelve una página fuera de rango y por qué? ¿Qué es la paginación por cursor y por qué no se usó aquí?

Para no saturar la ram, devuelve 200 pero vacía porque la consulta es válida, pero no hay datos que devolver.La paginación por cursor utiliza un identificador único ilugar de numero de pagina, no se usó porque la paginación por offset es más sencilla de implementar y suficiente para este trabajo.

## 6. ETag: ¿qué problema resuelve? ¿Fuerte o débil, y cómo lo calculó? ¿Qué situación no cubre?

Ahorra ancho de banda y procesamiento respondiendo 304 Not Modified cuando el ticket no ha cambiado, evitando reenviar los datos. Es un ETag Fuerte. Se calculó aplicando un hash SHA256 sobre la combinación de Id, FechaActualizacion.Ticks, Estado y Prioridad. No detecta cambios en datos relacionados que no actualicen la fecha de modificación del ticket

## 7. Una transición de estado inválida: ¿por qué 409 y no 400 ni 422?

es 409 porque no se puede completar por la condición o estado en el que se encuentra actualmente el ticket. y no 400 ni 422 porque la petición es válida, pero no puede ser procesada debido a la regla de negocio que se está violando.

## 8. Fuentes consultadas (enlaces).

https://datatracker.ietf.org/doc/html/rfc9457
https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-10.0
https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0&tabs=minimal-apis

## 9. Declaración de uso de IA (si aplica).

Hola tengo una tarea de progra avanzada en web

Construya sobre SistemaSoporte(API) el módulo de Tickets: un API en capas con enrutamiento avanzado, reglas de negocio y respuestas HTTP correctas, con una variante personal según su carnet y temas que debe investigar por su cuenta. Entregue el enlace a un repositorio público de GitHub con DECISIONES.md y README.md. Se permite IA si se declara

Puedes explicarme como hacerla por favor 

¡Hola Héctor! Esta es una práctica programada excelente y muy completa para el curso de Programación Avanzada en Web (SC-701).   Como el documento indica que debes partir del proyecto SistemaSoporte visto en clase (con sus 4 capas: Abstracciones, DA, Flujo y API), vamos a desglosar paso a paso todo lo que necesitas hacer para construir el módulo de Tickets.   Paso 1: Identificar tu Variante Personal (Carne / $v$)Lo primero es calcular tu variante en el punto 4 del documento:
   $$v = \text{último dígito de tu carnet} \pmod 5$$Dependiendo del resultado ($0, 1, 2, 3$ o $4$), se te asignará:Tamaño máximo de página (por ejemplo: 20, 25, 30, 40 o 50).   Formato del código de ticket (ej. TK-AAAA-NNNN, INC-NNNNNN, SP-AA-NNN, HD-NNNN o T-AAMMDD-NN).   Regla de negocio especial que debe retornar HTTP 409 Conflict si se viola.   Paso 2: Configurar las Capas1. Capa AbstraccionesAgrega las enumeraciones PrioridadTicket y EstadoTicket, además de la clase Ticket tal como lo especifica el PDF. También define aquí los DTOs para las peticiones/respuestas (ej. TicketCrearDto, TicketResumenDto, TicketPatchDto) y las interfaces de servicios.   2. Capa DA (Data Access)Crea TicketDA que implemente la interfaz respectiva. Los datos se mantendrán en una List<Ticket> en memoria con al menos 25 tickets generados al iniciar.   Nota: Recuerda que los ClienteId de los tickets deben coincidir con IDs existentes en ClienteDA.   3. Capa Flujo (Business/Logic)Contiene la lógica de negocio, validaciones de transiciones de estado (Abierto $\rightarrow$ EnProceso $\rightarrow$ Resuelto $\rightarrow$ Cerrado) y tu regla de negocio especial.   REGLA ARQUITECTÓNICA CLAVE: El proyecto Flujo NO debe referenciar librerías de ASP.NET Core (Microsoft.AspNetCore.*). Para comunicar errores al API, debes usar una abstracción (por ejemplo, un patrón Result<T> o Excepciones de Dominio personalizadas).   4. Capa APIContiene los Controllers y los Minimal API endpoints:   Controllers (Controllers/TicketsController.cs):GET /api/tickets (Paginación con filtros y headers X-Total-Count, X-Page, X-Page-Size).   GET /api/tickets/{id} (Con restricción de enteros $\ge 1$ y soporte de caché con ETag / If-None-Match / 304 Not Modified).   GET /api/tickets/codigo/{codigo} (Con una restricción de ruta personalizada IRouteConstraint).   POST /api/tickets (Crea ticket con estado Abierto, responde 201 Created con header Location).   PATCH /api/tickets/{id}/estado (Actualiza estado, agente y/o prioridad).   Minimal API (Program.cs):GET /min/tickets/resumen (MapGet con estadísticas).   POST /min/tickets/{id}/reabrir (MapPost para reabrir).   Vista HTML (wwwroot/tickets.html):Interfaz sencilla que consuma el API usando fetch, muestre los tickets en una tabla con filtros y botones de paginación.   Paso 3: Temas de Investigación RequeridosEl PDF solicita investigar 6 temas clave que deben implementarse en el código y justificarse en DECISIONES.md:   RFC 9457 / ProblemDetails: Usar builder.Services.AddProblemDetails() y retornar Problem() / ValidationProblem() con application/problem+json para errores $400$, $404$ y $409$.   Caché Condicional (ETag): Calcular un ETag (por ejemplo, un hash MD5/SHA256 rápido basado en la fecha de actualización y versión del ticket) y responder 304 Not Modified si coincide con If-None-Match.   Restricciones de Ruta Personalizadas: Crear una clase que implemente IRouteConstraint para validar con un Regex el formato del código según tu variante y registrarla en RouteOptions.ConstraintMap en Program.cs.   Binding y Conversión de Enums: Registrar JsonStringEnumConverter en la configuración de JSON para permitir recibir enumeraciones como texto en el body y query params.   OpenAPI / Scalar: Documentar respuestas (ProducesResponseType / ProducesProblem) para que Scalar muestre los contratos completos.   Manejo de Errores de Negocio: Elegir cómo la capa Flujo le avisa al API sobre errores (Result Pattern vs Excepciones).   Paso 4: Redacción de DECISIONES.md y README.mdEn la raíz del repositorio debes subir dos archivos esenciales:   README.md: Instrucciones de cómo clonar, compilar y ejecutar la solución.   DECISIONES.md: Responder detalladamente a las 9 preguntas planteadas en la sección 8 del PDF:   Tu variante ($v$) y nombre completo.   Justificación de respuesta HTTP ($400$ vs $404$ vs $422$) si el ClienteId no existe.   Diferencia entre $404$ por restricción de ruta y $400$ por body inválido.   Comparativa de mecanismos para comunicar errores entre Flujo y API.   Análisis de paginación (límites, fuera de rango, offset vs cursor).   Análisis de ETag (fuerte vs débil, cálculo y limitaciones).   Justificación de $409 Conflict$ en transiciones de estado inválidas.   Fuentes consultadas.   Declaración de uso de Inteligencia Artificial (prompt, respuesta y lo que corregiste/mejoraste).

Puedes generarme los 25 tickets que deben estar en una lista en TicketDA siguiendo la regla de v = 1

A continuación tienes la lista inicial de 25 tickets simulados respetando la enumeración PrioridadTicket y EstadoTicket, las fechas de creación y actualización, el formato de código INC-NNNNNN para $v = 1$, e IDs de clientes comunes (1 a 5):  
y me puso los tickets que puse en TicketDA.cs

"PUSE ALGO EN DESORDEN? ME ESTA DANDO UNOS ERRORES, PUEDES IDENTIFICAR POR QUE?"
Identificó que las llamadas a builder.Services se estaban ejecutando después de builder.Build(), reordenó el ciclo de vida del middleware en Program.cs

"¿Cómo hago la prueba de Caché Condicional en Scalar enviando el ETag?"
Explicó paso a paso cómo capturar la cabecera ETag devuelta por el endpoint GET /api/tickets/1 y cómo enviarla en las cabeceras de la siguiente petición utilizando la clave If-None-Match para verificar la respuesta HTTP 304 Not Modified.
