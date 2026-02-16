using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class TareasController : Controller
{
    private readonly ITareasService _service;

    public TareasController(ITareasService service)
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
        try
        {
            var tareas = await _service.ObtenerTodos();

            var lista = tareas.Select(t => new VMTarea
            {
                Id = t.Id,
                Fecha = t.Fecha,
                FechaLimite = t.FechaLimite,
                Descripcion = t.Descripcion,

                IdPersonal = t.IdPersonal,
                Personal = t.IdPersonalNavigation.Nombre,

                IdEstado = t.IdEstado,
                Estado = t.IdEstadoNavigation.Nombre,

                IdUsuarioRegistra = t.IdUsuarioRegistra,
                FechaRegistra = t.FechaRegistra,
                UsuarioRegistra = t.IdUsuarioRegistraNavigation.Usuario,

                IdUsuarioModifica = t.IdUsuarioModifica,
                FechaModifica = t.FechaModifica,
                UsuarioModifica = t.IdUsuarioModificaNavigation != null
                    ? t.IdUsuarioModificaNavigation.Usuario
                    : ""
            }).ToList();

            return Ok(lista);
        }
        catch
        {
            return Ok(new System.Collections.Generic.List<VMTarea>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] VMTarea model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var tarea = new Tarea
        {
            Fecha = model.Fecha,
            FechaLimite = model.FechaLimite,
            IdPersonal = model.IdPersonal,
            Descripcion = model.Descripcion,
            IdEstado = model.IdEstado,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        };

        bool respuesta = await _service.Insertar(tarea);
        return Ok(new { valor = respuesta });
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMTarea model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var tarea = new Tarea
        {
            Id = model.Id,
            Fecha = model.Fecha,
            FechaLimite = model.FechaLimite,
            IdPersonal = model.IdPersonal,
            Descripcion = model.Descripcion,
            IdEstado = model.IdEstado,

            IdUsuarioModifica = idUsuario,
            FechaModifica = DateTime.Now
        };

        bool respuesta = await _service.Actualizar(tarea);
        return Ok(new { valor = respuesta });
    }

    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        bool respuesta = await _service.Eliminar(id);
        return Ok(new { valor = respuesta });
    }

    [HttpGet]
    public async Task<IActionResult> EditarInfo(int id)
    {
        var t = await _service.Obtener(id);

        if (t == null)
            return NotFound();

        var vm = new VMTarea
        {
            Id = t.Id,
            Fecha = t.Fecha,
            FechaLimite = t.FechaLimite,
            IdPersonal = t.IdPersonal,
            Descripcion = t.Descripcion,
            IdEstado = t.IdEstado,

            FechaRegistra = t.FechaRegistra,
            UsuarioRegistra = t.IdUsuarioRegistraNavigation?.Usuario,

            FechaModifica = t.FechaModifica,
            UsuarioModifica = t.IdUsuarioModificaNavigation?.Usuario
        };

        return Ok(vm);
    }
}
