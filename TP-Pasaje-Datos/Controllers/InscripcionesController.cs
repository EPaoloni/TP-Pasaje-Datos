using Microsoft.AspNetCore.Mvc;
using TP_Pasaje_Datos.Extensions;
using TP_Pasaje_Datos.Models;

namespace TP_Pasaje_Datos.Controllers
{
    public class InscripcionesController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Titulo = "Inscripción a Talleres Académicos";

            ViewData["Talleres"] = new List<string>
            {
                "Programación Web",
                "Bases de Datos",
                "Inteligencia Artificial",
                "Ciberseguridad",
                "Metodologías Ágiles"
            };

            var ultimoProcesado = HttpContext.Session.Get<InscripcionViewModel>("UltimoProcesado");
            ViewBag.HayUltimoProcesado = ultimoProcesado != null;

            var model = new InscripcionViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Registrar(InscripcionViewModel model)
        {
            model.FechaInscripcion = DateTime.Now;

            HttpContext.Session.Set("UltimoProcesado", model);

            TempData["MensajeResultado"] = $"La inscripción de {model.NombreAlumno} (Legajo {model.Legajo}) al taller {model.TallerSeleccionado} se registró correctamente.";

            return RedirectToAction("Index");
        }
    }
}
