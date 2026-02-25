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
            UsuarioModifica = c.IdUsuarioModificaNavigation.Usuario,

             AsociacionAutomatica = (c.AsociacionAutomatica ?? 0) == 1,
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
            AsociacionAutomatica = model.AsociacionAutomatica ? 1 : 0,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        };

        bool respuesta = await _service.Insertar(prod, model.ClientesIds);
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

            AsociacionAutomatica = model.AsociacionAutomatica ? 1 : 0,

            IdUsuarioModifica = idUsuario,
            FechaModifica = DateTime.Now
        };

        bool respuesta = await _service.Actualizar(prod, model.ClientesIds);
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

            AsociacionAutomatica = (p.AsociacionAutomatica ?? 0) == 1,

            // ✅ clientes asignados
            ClientesIds = (p.ProductorasClientesAsignados ?? new List<ProductorasClientesAsignado>())
                .Select(x => x.IdCliente)
                .ToList(),

            FechaRegistra = p.FechaRegistra,
            UsuarioRegistra = p.IdUsuarioRegistraNavigation?.Usuario,

            FechaModifica = p.FechaModifica,
            UsuarioModifica = p.IdUsuarioModificaNavigation?.Usuario
        };

        return Ok(vm);
    }
}
