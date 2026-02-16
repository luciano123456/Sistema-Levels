using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

[Authorize]
public class RepresentantesController : Controller
{
    private readonly IRepresentantesService _service;

    public RepresentantesController(IRepresentantesService service)
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
            var reps = await _service.ObtenerTodos();

            var lista = reps.Select(c => new VMRepresentante
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Dni = c.Dni,
                NumeroDocumento = c.NumeroDocumento,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Email = c.Email,
                IdPais = c.IdPais,
                Pais = c.IdPaisNavigation.Nombre,

                TipoDocumento = c.IdTipoDocumentoNavigation.Nombre,

                IdUsuarioRegistra = c.IdUsuarioRegistra,
                FechaRegistra = c.FechaRegistra,
                UsuarioRegistra = c.IdUsuarioRegistraNavigation.Usuario,

                IdUsuarioModifica = c.IdUsuarioModifica,
                FechaModifica = c.FechaModifica,
                UsuarioModifica = c.IdUsuarioModificaNavigation.Usuario
            }).ToList();

            return Ok(lista);

        } catch (Exception ex)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] VMRepresentante model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var rep = new Representante
        {
            Nombre = model.Nombre,
            Dni = model.Dni,
            IdPais = model.IdPais,
            IdTipoDocumento = model.IdTipoDocumento,
            NumeroDocumento = model.NumeroDocumento,
            Direccion = model.Direccion,
            Telefono = model.Telefono,
            Email = model.Email,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        };

        bool respuesta = await _service.Insertar(rep);
        return Ok(new { valor = respuesta });
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMRepresentante model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var rep = new Representante
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Dni = model.Dni,
            IdPais = model.IdPais,
            IdTipoDocumento = model.IdTipoDocumento,
            NumeroDocumento = model.NumeroDocumento,
            Direccion = model.Direccion,
            Telefono = model.Telefono,
            Email = model.Email,

            IdUsuarioModifica = idUsuario,
            FechaModifica = DateTime.Now
        };

        bool respuesta = await _service.Actualizar(rep);
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
        var rep = await _service.Obtener(id);

        if (rep == null)
            return NotFound();

        var vm = new VMRepresentante
        {
            Id = rep.Id,
            Nombre = rep.Nombre,
            Dni = rep.Dni,
            IdPais = rep.IdPais,
            IdTipoDocumento = rep.IdTipoDocumento,
            NumeroDocumento = rep.NumeroDocumento,
            Direccion = rep.Direccion,
            Telefono = rep.Telefono,
            Email = rep.Email,

            FechaRegistra = rep.FechaRegistra,
            UsuarioRegistra = rep.IdUsuarioRegistraNavigation?.Usuario,

            FechaModifica = rep.FechaModifica,
            UsuarioModifica = rep.IdUsuarioModificaNavigation?.Usuario
        };

        return Ok(vm);
    }



}
