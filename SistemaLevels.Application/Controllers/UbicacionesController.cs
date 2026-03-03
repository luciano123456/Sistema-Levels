using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Common;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

[Authorize]
public class UbicacionesController : Controller
{
    private readonly IUbicacionesService _service;

    public UbicacionesController(IUbicacionesService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Lista()
    {
        var data = await _service.ObtenerTodos();

        var lista = data.Select(x => new VMUbicacion
        {
            Id = x.Id,
            Descripcion = x.Descripcion,
            Espacio = x.Espacio,
            Direccion = x.Direccion
        }).ToList();

        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] VMUbicacion model)
    {
        var entity = new Ubicacion
        {
            Descripcion = model.Descripcion,
            Espacio = model.Espacio,
            Direccion = model.Direccion
        };

        var result = await _service.Insertar(entity);

        return Ok(new
        {
            valor = result.Ok,
            mensaje = result.Mensaje,
            tipo = result.Tipo,
            idReferencia = result.IdReferencia
        });
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMUbicacion model)
    {
        var entity = new Ubicacion
        {
            Id = model.Id,
            Descripcion = model.Descripcion,
            Espacio = model.Espacio,
            Direccion = model.Direccion
        };

        var result = await _service.Actualizar(entity);

        return Ok(new
        {
            valor = result.Ok,
            mensaje = result.Mensaje,
            tipo = result.Tipo,
            idReferencia = result.IdReferencia
        });
    }

    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        var result = await _service.Eliminar(id);

        return Ok(new
        {
            valor = result.Ok,
            mensaje = result.Mensaje,
            tipo = result.Tipo,
            idReferencia = result.IdReferencia
        });
    }

    [HttpGet]
    public async Task<IActionResult> EditarInfo(int id)
    {
        var u = await _service.Obtener(id);
        if (u == null) return NotFound();

        return Ok(new
        {
            u.Id,
            u.Descripcion,
            u.Espacio,
            u.Direccion
        });
    }
}