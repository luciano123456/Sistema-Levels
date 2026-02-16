using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

[Authorize]
public class ProductorasController : Controller
{
    private readonly IProductorasService _service;

    public ProductorasController(IProductorasService service)
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
        var reps = await _service.ObtenerTodos();

        var lista = reps.Select(c => new VMProductora
        {
            Id = c.Id,
            Nombre = c.Nombre,
            NombreRepresentante = c.NombreRepresentante,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion,

            Pais = c.IdpaisNavigation.Nombre,
            TipoDocumento = c.IdTipoDocumentoNavigation.Nombre,
            CondicionIva = c.IdCondicionIvaNavigation.Nombre,
            Provincia = c.IdProvinciaNavigation.Nombre,

            IdUsuarioRegistra = c.IdUsuarioRegistra,
            FechaRegistra = c.FechaRegistra,
            UsuarioRegistra = c.IdUsuarioRegistraNavigation.Usuario,

            IdUsuarioModifica = c.IdUsuarioModifica,
            FechaModifica = c.FechaModifica,
            UsuarioModifica = c.IdUsuarioModificaNavigation.Usuario
        }).ToList();

        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] VMProductora model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var prod = new Productora
        {
            Nombre = model.Nombre,
            NombreRepresentante = model.NombreRepresentante,
            Telefono = model.Telefono,
            TelefonoAlternativo = model.TelefonoAlternativo,
            Dni = model.Dni,
            Idpais = model.Idpais,
            IdTipoDocumento = model.IdTipoDocumento,
            NumeroDocumento = model.NumeroDocumento,
            IdCondicionIva = model.IdCondicionIva,
            Email = model.Email,
            IdProvincia = model.IdProvincia,
            Localidad = model.Localidad,
            EntreCalles = model.EntreCalles,
            Direccion = model.Direccion,
            CodigoPostal = model.CodigoPostal,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        };

        bool respuesta = await _service.Insertar(prod);
        return Ok(new { valor = respuesta });
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMProductora model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var prod = new Productora
        {
            Id = model.Id,
            Nombre = model.Nombre,
            NombreRepresentante = model.NombreRepresentante,
            Telefono = model.Telefono,
            TelefonoAlternativo = model.TelefonoAlternativo,
            Dni = model.Dni,
            Idpais = model.Idpais,
            IdTipoDocumento = model.IdTipoDocumento,
            NumeroDocumento = model.NumeroDocumento,
            IdCondicionIva = model.IdCondicionIva,
            Email = model.Email,
            IdProvincia = model.IdProvincia,
            Localidad = model.Localidad,
            EntreCalles = model.EntreCalles,
            Direccion = model.Direccion,
            CodigoPostal = model.CodigoPostal,

            IdUsuarioModifica = idUsuario,
            FechaModifica = DateTime.Now
        };

        bool respuesta = await _service.Actualizar(prod);
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

        if (p == null)
            return NotFound();

        var vm = new VMProductora
        {
            Id = p.Id,
            Nombre = p.Nombre,
            NombreRepresentante = p.NombreRepresentante,
            Telefono = p.Telefono,
            TelefonoAlternativo = p.TelefonoAlternativo,
            Dni = p.Dni,
            Idpais = p.Idpais,
            IdTipoDocumento = p.IdTipoDocumento,
            NumeroDocumento = p.NumeroDocumento,
            IdCondicionIva = p.IdCondicionIva,
            Email = p.Email,
            IdProvincia = p.IdProvincia,
            Localidad = p.Localidad,
            EntreCalles = p.EntreCalles,
            Direccion = p.Direccion,
            CodigoPostal = p.CodigoPostal,

            FechaRegistra = p.FechaRegistra,
            UsuarioRegistra = p.IdUsuarioRegistraNavigation?.Usuario,

            FechaModifica = p.FechaModifica,
            UsuarioModifica = p.IdUsuarioModificaNavigation?.Usuario
        };

        return Ok(vm);
    }
}
