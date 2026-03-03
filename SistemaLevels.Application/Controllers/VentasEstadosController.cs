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
    public class VentasEstadosController : Controller
    {
        private readonly IVentasEstadosService _VentasEstadosService;

        public VentasEstadosController(IVentasEstadosService VentasEstadosService)
        {
            _VentasEstadosService = VentasEstadosService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var VentasEstados = await _VentasEstadosService.ObtenerTodos();

            var lista = VentasEstados.Select(c => new VMGenericModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMGenericModel model)
        {
            var VentasEstado = new VentasEstado
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _VentasEstadosService.Insertar(VentasEstado);

            return Ok(new { valor = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMGenericModel model)
        {
            var VentasEstado = new VentasEstado
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _VentasEstadosService.Actualizar(VentasEstado);

            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _VentasEstadosService.Eliminar(id);

            return StatusCode(StatusCodes.Status200OK, new { valor = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
             var VentasEstado = await _VentasEstadosService.Obtener(id);

            if (VentasEstado != null)
            {
                return StatusCode(StatusCodes.Status200OK, VentasEstado);
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