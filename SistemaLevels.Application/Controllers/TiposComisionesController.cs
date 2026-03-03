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
    public class TiposComisionesController : Controller
    {
        private readonly ITiposComisionesService _TiposComisionesService;

        public TiposComisionesController(ITiposComisionesService TiposComisionesService)
        {
            _TiposComisionesService = TiposComisionesService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var TiposComisiones = await _TiposComisionesService.ObtenerTodos();

            var lista = TiposComisiones.Select(c => new VMGenericModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMGenericModel model)
        {
            var TiposComision = new TiposComision
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _TiposComisionesService.Insertar(TiposComision);

            return Ok(new { valor = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMGenericModel model)
        {
            var TiposComision = new TiposComision
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _TiposComisionesService.Actualizar(TiposComision);

            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _TiposComisionesService.Eliminar(id);

            return StatusCode(StatusCodes.Status200OK, new { valor = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
             var TiposComision = await _TiposComisionesService.Obtener(id);

            if (TiposComision != null)
            {
                return StatusCode(StatusCodes.Status200OK, TiposComision);
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