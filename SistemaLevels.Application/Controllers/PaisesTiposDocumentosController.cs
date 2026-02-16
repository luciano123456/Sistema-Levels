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
    public class PaisesTiposDocumentosController : Controller
    {
        private readonly IPaisesTiposDocumentosService _PaisesTiposDocumentosService;

        public PaisesTiposDocumentosController(IPaisesTiposDocumentosService PaisesTiposDocumentosService)
        {
            _PaisesTiposDocumentosService = PaisesTiposDocumentosService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var PaisesTiposDocumentos = await _PaisesTiposDocumentosService.ObtenerTodos();

            var lista = PaisesTiposDocumentos.Select(c => new VMGenericModelConfCombo
            {
                Id = c.Id,
                IdCombo = c.IdPais,
                NombreCombo = c.IdPaisNavigation.Nombre,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMGenericModelConfCombo model)
        {
            var pais = new PaisesTiposDocumento
            {
                Id = model.Id,
                IdPais = model.IdCombo,
                Nombre = model.Nombre,
            };

            bool respuesta = await _PaisesTiposDocumentosService.Insertar(pais);

            return Ok(new { valor = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMGenericModelConfCombo model)
        {
            var pais = new PaisesTiposDocumento
            {
                Id = model.Id,
                IdPais = model.IdCombo,
                Nombre = model.Nombre,
            };

            bool respuesta = await _PaisesTiposDocumentosService.Actualizar(pais);

            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _PaisesTiposDocumentosService.Eliminar(id);

            return StatusCode(StatusCodes.Status200OK, new { valor = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
            var resultBase = await _PaisesTiposDocumentosService.Obtener(id);

            var result = new VMGenericModelConfCombo
            {
                Id = resultBase.Id,
                IdCombo = resultBase.IdPais,
                Nombre = resultBase.Nombre,
                NombreCombo = resultBase.IdPaisNavigation.Nombre
            };

            if (result != null)
            {
                return StatusCode(StatusCodes.Status200OK, result);
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