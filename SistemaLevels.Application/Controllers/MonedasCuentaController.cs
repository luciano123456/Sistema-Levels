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
    public class MonedasCuentaController : Controller
    {
        private readonly IMonedasCuentaService _MonedasCuentaService;

        public MonedasCuentaController(IMonedasCuentaService MonedasCuentaService)
        {
            _MonedasCuentaService = MonedasCuentaService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var MonedasCuenta = await _MonedasCuentaService.ObtenerTodos();

            var lista = MonedasCuenta.Select(c => new VMGenericModelConfCombo
            {
                Id = c.Id,
                IdCombo = (int)c.IdMoneda,
                NombreCombo = c.IdMonedaNavigation.Nombre,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ListaMoneda(int idMoneda)
        {
            var MonedasCuenta = await _MonedasCuentaService.ObtenerMoneda(idMoneda);

            var lista = MonedasCuenta.Select(c => new VMGenericModelConfCombo
            {
                Id = c.Id,
                IdCombo = (int)c.IdMoneda,
                NombreCombo = c.IdMonedaNavigation.Nombre,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMGenericModelConfCombo model)
        {
            var pais = new MonedasCuenta
            {
                Id = model.Id,
                IdMoneda = model.IdCombo,
                Nombre = model.Nombre,
            };

            bool respuesta = await _MonedasCuentaService.Insertar(pais);

            return Ok(new { valor = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMGenericModelConfCombo model)
        {
            var pais = new MonedasCuenta
            {
                Id = model.Id,
                IdMoneda = model.IdCombo,
                Nombre = model.Nombre,
            };

            bool respuesta = await _MonedasCuentaService.Actualizar(pais);

            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _MonedasCuentaService.Eliminar(id);

            return StatusCode(StatusCodes.Status200OK, new { valor = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
            var resultBase = await _MonedasCuentaService.Obtener(id);

            var result = new VMGenericModelConfCombo
            {
                Id = resultBase.Id,
                IdCombo = (int)resultBase.IdMoneda,
                Nombre = resultBase.Nombre,
                NombreCombo = resultBase.IdMonedaNavigation.Nombre
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