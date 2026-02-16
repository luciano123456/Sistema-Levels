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
    public class TareasEstadosController : Controller
    {
        private readonly ITareasEstadosService _TareasEstadosService;

        public TareasEstadosController(ITareasEstadosService TareasEstadosService)
        {
            _TareasEstadosService = TareasEstadosService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var TareasEstados = await _TareasEstadosService.ObtenerTodos();

            var lista = TareasEstados.Select(c => new VMTareasEstados
            {
                Id = c.Id,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMTareasEstados model)
        {
            var TareasEstado = new TareasEstado
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _TareasEstadosService.Insertar(TareasEstado);

            return Ok(new { valor = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMTareasEstados model)
        {
            var TareasEstado = new TareasEstado
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _TareasEstadosService.Actualizar(TareasEstado);

            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _TareasEstadosService.Eliminar(id);

            return StatusCode(StatusCodes.Status200OK, new { valor = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
             var TareasEstado = await _TareasEstadosService.Obtener(id);

            if (TareasEstado != null)
            {
                return StatusCode(StatusCodes.Status200OK, TareasEstado);
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