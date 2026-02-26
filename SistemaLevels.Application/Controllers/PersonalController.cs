using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

[Authorize]
public class PersonalController : Controller
{
    private readonly IPersonalService _service;

    public PersonalController(IPersonalService service)
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
        var listaBase = await _service.ObtenerTodos();

        var lista = listaBase.Select(c => new VMPersonal
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Dni = c.Dni,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion,
            NumeroDocumento = c.NumeroDocumento,
            FechaNacimiento = c.FechaNacimiento,

            Pais = c.IdPaisNavigation.Nombre,
            TipoDocumento = c.IdTipoDocumentoNavigation != null ? c.IdTipoDocumentoNavigation.Nombre : "",
            CondicionIva = c.IdCondicionIvaNavigation != null ? c.IdCondicionIvaNavigation.Nombre : "",

            IdUsuarioRegistra = c.IdUsuarioRegistra,
            FechaRegistra = c.FechaRegistra,
            UsuarioRegistra = c.IdUsuarioRegistraNavigation.Usuario,

            IdUsuarioModifica = c.IdUsuarioModifica,
            FechaModifica = c.FechaModifica,
            UsuarioModifica = c.IdUsuarioModificaNavigation != null ? c.IdUsuarioModificaNavigation.Usuario : ""
        }).ToList();

        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> ListaFiltrada([FromBody] VMPersonalFiltro f)
    {
        var query = await _service.ListarFiltrado(
            f.Nombre,
            f.IdPais,
            f.IdTipoDocumento,
            f.IdCondicionIva,
            f.IdRol,
            f.IdArtista
        );

        var lista = query.Select(c => new VMPersonal
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Dni = c.Dni,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion,
            NumeroDocumento = c.NumeroDocumento,
            FechaNacimiento = c.FechaNacimiento,

            Pais = c.IdPaisNavigation.Nombre,
            TipoDocumento = c.IdTipoDocumentoNavigation != null ? c.IdTipoDocumentoNavigation.Nombre : "",
            CondicionIva = c.IdCondicionIvaNavigation != null ? c.IdCondicionIvaNavigation.Nombre : "",

            IdUsuarioRegistra = c.IdUsuarioRegistra,
            FechaRegistra = c.FechaRegistra,
            UsuarioRegistra = c.IdUsuarioRegistraNavigation.Usuario,

            IdUsuarioModifica = c.IdUsuarioModifica,
            FechaModifica = c.FechaModifica,
            UsuarioModifica = c.IdUsuarioModificaNavigation != null ? c.IdUsuarioModificaNavigation.Usuario : ""
        }).ToList();

        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] VMPersonal model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var personal = new Personal
        {
            Nombre = model.Nombre,
            Dni = model.Dni,
            Telefono = model.Telefono,
            Email = model.Email,
            Direccion = model.Direccion,
            IdPais = model.IdPais,
            IdTipoDocumento = model.IdTipoDocumento,
            NumeroDocumento = model.NumeroDocumento,
            IdCondicionIva = model.IdCondicionIva,
            FechaNacimiento = model.FechaNacimiento,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        };

        var rolesIds = model.RolesIds ?? new List<int>();
        var artistasIds = model.ArtistasIds ?? new List<int>();

        bool respuesta = await _service.Insertar(personal, rolesIds, artistasIds);
        return Ok(new { valor = respuesta });
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMPersonal model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var personal = new Personal
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Dni = model.Dni,
            Telefono = model.Telefono,
            Email = model.Email,
            Direccion = model.Direccion,
            IdPais = model.IdPais,
            IdTipoDocumento = model.IdTipoDocumento,
            NumeroDocumento = model.NumeroDocumento,
            IdCondicionIva = model.IdCondicionIva,
            FechaNacimiento = model.FechaNacimiento,

            IdUsuarioModifica = idUsuario,
            FechaModifica = DateTime.Now
        };

        var rolesIds = model.RolesIds ?? new List<int>();
        var artistasIds = model.ArtistasIds ?? new List<int>();

        bool respuesta = await _service.Actualizar(personal, rolesIds, artistasIds);
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
        var p = await _service.Obtener(id);
        if (p == null) return NotFound();

        var rolesIds = await _service.ObtenerRolesIds(id);
        var artistasIds = await _service.ObtenerArtistasIds(id);

        var vm = new VMPersonal
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Dni = p.Dni,
            Telefono = p.Telefono,
            Email = p.Email,
            Direccion = p.Direccion,
            IdPais = p.IdPais,
            IdTipoDocumento = p.IdTipoDocumento,
            NumeroDocumento = p.NumeroDocumento,
            IdCondicionIva = p.IdCondicionIva,
            FechaNacimiento = p.FechaNacimiento,

            FechaRegistra = p.FechaRegistra,
            UsuarioRegistra = p.IdUsuarioRegistraNavigation?.Usuario ?? "",

            FechaModifica = p.FechaModifica,
            UsuarioModifica = p.IdUsuarioModificaNavigation?.Usuario ?? "",

            RolesIds = rolesIds,
            ArtistasIds = artistasIds
        };

        return Ok(vm);
    }
}
