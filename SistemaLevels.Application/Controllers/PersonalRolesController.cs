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
    public class PersonalRolController : Controller
    {
        private readonly IPersonalRolService _PersonalRolService;

        public PersonalRolController(IPersonalRolService PersonalRolService)
        {
            _PersonalRolService = PersonalRolService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var PersonalRol = await _PersonalRolService.ObtenerTodos();

            var lista = PersonalRol.Select(c => new VMGenericModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
            }).ToList();

            return Ok(lista);
        }


        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMGenericModel model)
        {
            var PersonalRol = new PersonalRol
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _PersonalRolService.Insertar(PersonalRol);

            return Ok(new { valor = respuesta });
        }

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMGenericModel model)
        {
            var PersonalRol = new PersonalRol
            {
                Id = model.Id,
                Nombre = model.Nombre,
            };

            bool respuesta = await _PersonalRolService.Actualizar(PersonalRol);

            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _PersonalRolService.Eliminar(id);

            return StatusCode(StatusCodes.Status200OK, new { valor = respuesta });
        }

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
             var PersonalRol = await _PersonalRolService.Obtener(id);

            if (PersonalRol != null)
            {
                return StatusCode(StatusCodes.Status200OK, PersonalRol);
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