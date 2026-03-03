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
    public class OpcionesBinariasController : Controller
    {
        private readonly IOpcionesBinariasService _OpcionesBinariasService;

        public OpcionesBinariasController(IOpcionesBinariasService OpcionesBinariasService)
        {
            _OpcionesBinariasService = OpcionesBinariasService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var OpcionesBinarias = await _OpcionesBinariasService.ObtenerTodos();

            var lista = OpcionesBinarias.Select(c => new VMGenericModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMGenericModel model)
        {
            var OpcionesBinaria = new OpcionesBinaria
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _OpcionesBinariasService.Insertar(OpcionesBinaria);

            return Ok(new { valor = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMGenericModel model)
        {
            var OpcionesBinaria = new OpcionesBinaria
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _OpcionesBinariasService.Actualizar(OpcionesBinaria);

            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _OpcionesBinariasService.Eliminar(id);

            return StatusCode(StatusCodes.Status200OK, new { valor = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
             var OpcionesBinaria = await _OpcionesBinariasService.Obtener(id);

            if (OpcionesBinaria != null)
            {
                return StatusCode(StatusCodes.Status200OK, OpcionesBinaria);
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