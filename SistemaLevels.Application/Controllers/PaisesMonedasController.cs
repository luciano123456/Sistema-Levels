using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

namespace SistemaLevels.Application.Controllers
{
    [Authorize]
    public class PaisesMonedaController : Controller
    {
        private readonly IPaisesMonedaService _service;

        public PaisesMonedaController(IPaisesMonedaService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var monedas = await _service.ObtenerTodos();

            var lista = monedas.Select(m => new VMPaisesMoneda
            {
                Id = m.Id,
                IdPais = m.IdPais,
                Pais = m.IdPaisNavigation != null ? m.IdPaisNavigation.Nombre : "",
                Nombre = m.Nombre,
                Cotizacion = m.Cotizacion
            }).ToList();

            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
            var m = await _service.Obtener(id);
            if (m == null) return Ok(null);

            var vm = new VMPaisesMoneda
            {
                Id = m.Id,
                IdPais = m.IdPais,
                Pais = m.IdPaisNavigation != null ? m.IdPaisNavigation.Nombre : "",
                Nombre = m.Nombre,
                Cotizacion = m.Cotizacion
            };

            return Ok(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMPaisesMoneda model)
        {
            try
            {
                var entidad = new PaisesMoneda
                {
                    IdPais = model.IdPais,
                    Nombre = model.Nombre,
                    Cotizacion = model.Cotizacion
                };

                bool ok = await _service.Insertar(entidad);
                return Ok(new { valor = ok });
            }
            catch
            {
                return Ok(new { valor = false });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMPaisesMoneda model)
        {
            try
            {
                var entidad = new PaisesMoneda
                {
                    Id = model.Id,
                    IdPais = model.IdPais,
                    Nombre = model.Nombre,
                    Cotizacion = model.Cotizacion
                };

                bool ok = await _service.Actualizar(entidad);
                return Ok(new { valor = ok });
            }
            catch
            {
                return Ok(new { valor = false });
            }
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarMasivo([FromBody] List<VMMonedaActualizacion> lista)
        {
            try
            {
                if (lista == null || lista.Count == 0)
                    return Ok(new { valor = false });

                // 🔥 Convertimos a estructura limpia para BLL
                var dic = lista.ToDictionary(x => x.Id, x => x.Cotizacion);

                bool ok = await _service.ActualizarMasivo(dic);

                return Ok(new { valor = ok });
            }
            catch
            {
                return Ok(new { valor = false });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                bool ok = await _service.Eliminar(id);
                return Ok(new { valor = ok });
            }
            catch
            {
                return Ok(new { valor = false });
            }
        }
    }
}
