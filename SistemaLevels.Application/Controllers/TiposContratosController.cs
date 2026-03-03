using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;
using System.Diagnostics;

namespace SistemaLevels.Application.Controllers
{
    [Authorize]
    public class TiposContratosController : Controller
    {
        private readonly ITiposContratosService _TiposContratosService;

        public TiposContratosController(ITiposContratosService TiposContratosService)
        {
            _TiposContratosService = TiposContratosService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var TiposContratos = await _TiposContratosService.ObtenerTodos();

            var lista = TiposContratos.Select(c => new VMGenericModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMGenericModel model)
        {
            var TiposContrato = new TiposContrato
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _TiposContratosService.Insertar(TiposContrato);

            return Ok(new { valor = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMGenericModel model)
        {
            var TiposContrato = new TiposContrato
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _TiposContratosService.Actualizar(TiposContrato);

            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _TiposContratosService.Eliminar(id);

            return StatusCode(StatusCodes.Status200OK, new { valor = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
             var TiposContrato = await _TiposContratosService.Obtener(id);

            if (TiposContrato != null)
            {
                return StatusCode(StatusCodes.Status200OK, TiposContrato);
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}