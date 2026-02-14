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
    public class PaisesController : Controller
    {
        private readonly IPaisService _PaisesService;

        public PaisesController(IPaisService PaisesService)
        {
            _PaisesService = PaisesService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var Paises = await _PaisesService.ObtenerTodos();

            var lista = Paises.Select(c => new VMPais
            {
                Id = c.Id,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMPais model)
        {
            var pais = new Pais
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _PaisesService.Insertar(pais);

            return Ok(new { valor = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMPais model)
        {
            var pais = new Pais
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _PaisesService.Actualizar(pais);

            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _PaisesService.Eliminar(id);

            return StatusCode(StatusCodes.Status200OK, new { valor = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
            var UsuariosPaise = await _PaisesService.Obtener(id);

            if (UsuariosPaise != null)
            {
                return StatusCode(StatusCodes.Status200OK, UsuariosPaise);
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