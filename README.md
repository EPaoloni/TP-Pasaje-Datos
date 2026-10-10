# TP Integrador: Gestión de Estado y Pasaje de Datos en ASP.NET Core MVC

**Universidad Nacional de La Matanza (UNLaM)**
Departamento de Ingeniería e Investigaciones Tecnológicas
Carrera: Licenciatura en Gestión de la Tecnología — Asignatura: Programación Avanzada 2

## 1. Introducción y Objetivos

En el desarrollo de aplicaciones web mediante el patrón Modelo-Vista-Controlador (MVC), el protocolo HTTP actúa como un canal de comunicación sin estado (stateless). Para construir experiencias dinámicas e interactivas manteniendo una estricta Separación de Responsabilidades (SoC), es fundamental dominar las herramientas que provee el framework para la transferencia de información y la persistencia temporal de datos.

**Objetivos pedagógicos del ejercicio:**

- Configurar el flujo de procesamiento de solicitudes HTTP mediante Middlewares en `Program.cs`.
- Aplicar mecanismos de pasaje de datos de corto plazo (`ViewData`, `ViewBag`, `TempData`) distinguiendo sus tipos de datos y ciclos de vida.
- Implementar el patrón de diseño ViewModel para la construcción de vistas fuertemente tipadas.
- Extender la interfaz `ISession` mediante métodos genéricos para permitir la serialización y deserialización de objetos complejos en formato JSON.

## 2. Descripción del Problema

El Departamento de Alumnos requiere un módulo web prototipo para gestionar la Inscripción de Estudiantes a Talleres Académicos. La aplicación debe permitir cargar solicitudes, desplegar listas de opciones institucionales y mantener un registro temporal del último estudiante procesado durante la sesión activa del navegador.

## 3. Requerimientos de Desarrollo

### Requerimiento 1: Configuración de Servicios y Middleware (`Program.cs`)

1. Registre el servicio de almacenamiento de caché en memoria mediante `builder.Services.AddDistributedMemoryCache()`.
2. Registre el servicio de sesión utilizando `builder.Services.AddSession()`, estableciendo un tiempo límite de inactividad (`IdleTimeout`) de 15 minutos y configurando la cookie de sesión como imperativa.
3. Habilite el middleware invocando `app.UseSession()`.

> **Nota de arquitectura:** asegúrese de ubicar `app.UseSession()` inmediatamente después de `app.UseRouting()` y antes de mapear los controladores.

```csharp
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
...
app.UseRouting();
app.UseSession(); // Ubicación requerida
app.MapDefaultControllerRoute();
```

### Requerimiento 2: Métodos de Extensión para Sesión (`Extensions/SessionExtensions.cs`)

Dado que `ISession` almacena nativamente únicamente tipos primitivos (`SetString` / `SetInt32`), cree una clase estática `SessionExtensions` dentro de la carpeta `Extensions`:

- `Set<T>(this ISession session, string clave, T valor)`: serializa el objeto con `JsonSerializer.Serialize`.
- `Get<T>(this ISession session, string clave)`: recupera la cadena JSON con `GetString` y la deserializa con `JsonSerializer.Deserialize<T>`.

```csharp
public static class SessionExtensions {
    public static void Set<T>(this ISession session, string clave, T valor) {
        session.SetString(clave, JsonSerializer.Serialize(valor));
    }
    public static T Get<T>(this ISession session, string clave) {
        var valor = session.GetString(clave);
        return valor == null ? default(T) : JsonSerializer.Deserialize<T>(valor);
    }
}
```

### Requerimiento 3: Modelo de Visualización (`Models/InscripcionViewModel.cs`)

Diseñe la clase `InscripcionViewModel` que actuará como modelo de la vista (desacoplado de la base de datos):

```csharp
namespace TuProyecto.Models {
    public class InscripcionViewModel {
        public string NombreAlumno { get; set; }
        public string Legajo { get; set; }
        public string TallerSeleccionado { get; set; }
        public DateTime FechaInscripcion { get; set; }
    }
}
```

### Requerimiento 4: Controlador (`Controllers/InscripcionesController.cs`)

1. **Acción `Index()` [HTTP GET]:**
   - Asigne el título de la pantalla mediante `ViewBag.Titulo`.
   - Cargue una lista de talleres académicos (`List<string>`) dentro de `ViewData["Talleres"]`.
   - Instancie un `InscripcionViewModel` por defecto y envíelo a la vista (`return View(model)`).
   - Verifique mediante `HttpContext.Session.Get<InscripcionViewModel>("UltimoProcesado")` si existe un alumno grabado en sesión.
2. **Acción `Registrar(InscripcionViewModel model)` [HTTP POST]:**
   - Capture los datos del formulario mediante Model Binding automático.
   - Actualice la variable de sesión `"UltimoProcesado"` utilizando el nuevo objeto.
   - Asigne un mensaje de confirmación a `TempData["MensajeResultado"]`.
   - Retorne una redirección HTTP 302 a la acción `Index` aplicando el patrón POST-Redirect-GET.

### Requerimiento 5: Vista Razor (`Views/Inscripciones/Index.cshtml`)

- Declare la vista como fuertemente tipada mediante `@model InscripcionViewModel`.
- Imprima el título cargado en el controlador usando `@ViewBag.Titulo`.
- Evalúe si `TempData["MensajeResultado"]` no es nulo y despliegue su contenido dentro de una caja de alerta.
- Despliegue el listado de talleres recorriendo `ViewData["Talleres"]` con el casteo explícito obligatorio (`as List<string>`).
- Recupere el objeto guardado en sesión (`Context.Session.Get<InscripcionViewModel>("UltimoProcesado")`) y muestre los datos del último registro.
- Renderice un formulario HTML (`<form action="/Inscripciones/Registrar" method="post">`) para capturar los datos y enviarlos vía POST.

## 4. Criterios de Evaluación

| Criterio | Exigencia técnica | Ponderación |
|---|---|---|
| 1. Configuración del Pipeline | Inclusión adecuada y ordenada de `AddDistributedMemoryCache`, `AddSession` y el middleware `app.UseSession()` en `Program.cs`. | 20% |
| 2. Uso de ViewData y ViewBag | Demostración práctica de la necesidad de casteo explícito en `ViewData` frente a la resolución dinámica de `ViewBag`. | 20% |
| 3. Persistencia con TempData | Verificación de que el mensaje de confirmación sobrevive a la redirección HTTP 302 sin perderse en el refresco. | 20% |
| 4. Tipado Fuerte (ViewModel) | Empleo de la directiva `@model` en la vista Razor y acceso directo a miembros con comprobación en compilación. | 20% |
| 5. Serialización JSON en Sesión | Correcta construcción y aplicación de los métodos de extensión genéricos `Set<T>` y `Get<T>` sobre `ISession`. | 20% |
