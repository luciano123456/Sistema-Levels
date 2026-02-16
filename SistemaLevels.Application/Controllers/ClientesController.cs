using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

[Authorize]
public class ClientesController : Controller
{
    private readonly IClientesService _service;

    public ClientesController(IClientesService service)
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
        var clientes = await _service.ObtenerTodos();

        var lista = clientes.Select(c => new VMCliente
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Telefono = c.Telefono,
            TelefonoAlternativo = c.TelefonoAlternativo,
            Email = c.Email,

            // IDs (para edición)
            IdProductora = c.IdProductora,
            IdPais = c.IdPais,
            IdProvincia = c.IdProvincia,
            IdTipoDocumento = c.IdTipoDocumento,
            IdCondicionIva = c.IdCondicionIva,

            NumeroDocumento = c.NumeroDocumento,
            Dni = c.Dni,

            Direccion = c.Direccion,
            Localidad = c.Localidad,
            EntreCalles = c.EntreCalles,
            CodigoPostal = c.CodigoPostal,

            // Nombres para grilla
            Productora = c.IdProductoraNavigation.Nombre,
            Pais = c.IdPaisNavigation.Nombre,
            Provincia = c.IdProvinciaNavigation.Nombre,
            TipoDocumento = c.IdTipoDocumentoNavigation != null
                ? c.IdTipoDocumentoNavigation.Nombre
                : "",
            CondicionIva = c.IdCondicionIvaNavigation != null
                ? c.IdCondicionIvaNavigation.Nombre
                : "",

            // Auditoría
            IdUsuarioRegistra = c.IdUsuarioRegistra,
            FechaRegistra = c.FechaRegistra,
            UsuarioRegistra = c.IdUsuarioRegistraNavigation.Usuario,

            IdUsuarioModifica = c.IdUsuarioModifica,
            FechaModifica = c.FechaModifica,
            UsuarioModifica = c.IdUsuarioModificaNavigation != null
                ? c.IdUsuarioModificaNavigation.Usuario
                : ""
        }).ToList();

        return Ok(lista);
    }


    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] VMCliente model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var cliente = new Cliente
        {
            Nombre = model.Nombre,
            Telefono = model.Telefono,
            TelefonoAlternativo = model.TelefonoAlternativo,
            Dni = model.Dni,
            IdTipoDocumento = model.IdTipoDocumento,
            NumeroDocumento = model.NumeroDocumento,
            Email = model.Email,
            IdProductora = model.IdProductora,
            IdPais = model.IdPais,
            IdProvincia = model.IdProvincia,
            Localidad = model.Localidad,
            EntreCalles = model.EntreCalles,
            Direccion = model.Direccion,
            CodigoPostal = model.CodigoPostal,
            IdCondicionIva = model.IdCondicionIva,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        };

        bool respuesta = await _service.Insertar(cliente);
        return Ok(new { valor = respuesta });
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMCliente model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id").Value);

        var cliente = new Cliente
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Telefono = model.Telefono,
            TelefonoAlternativo = model.TelefonoAlternativo,
            Dni = model.Dni,
            IdTipoDocumento = model.IdTipoDocumento,
            NumeroDocumento = model.NumeroDocumento,
            Email = model.Email,
            IdProductora = model.IdProductora,
            IdPais = model.IdPais,
            IdProvincia = model.IdProvincia,
            Localidad = model.Localidad,
            EntreCalles = model.EntreCalles,
            Direccion = model.Direccion,
            CodigoPostal = model.CodigoPostal,
            IdCondicionIva = model.IdCondicionIva,

            IdUsuarioModifica = idUsuario,
            FechaModifica = DateTime.Now,
        };

        bool respuesta = await _service.Actualizar(cliente);
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
        var c = await _service.Obtener(id);

        if (c == null)
            return NotFound();

        var vm = new VMCliente
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Telefono = c.Telefono,
            TelefonoAlternativo = c.TelefonoAlternativo,
            Dni = c.Dni,
            IdTipoDocumento = c.IdTipoDocumento,
            NumeroDocumento = c.NumeroDocumento,
            Email = c.Email,
            IdProductora = c.IdProductora,
            IdPais = c.IdPais,
            IdProvincia = c.IdProvincia,
            Localidad = c.Localidad,
            EntreCalles = c.EntreCalles,
            Direccion = c.Direccion,
            CodigoPostal = c.CodigoPostal,
            IdCondicionIva = c.IdCondicionIva,

            FechaRegistra = c.FechaRegistra,
            UsuarioRegistra = c.IdUsuarioRegistraNavigation?.Usuario,

            FechaModifica = c.FechaModifica,
            UsuarioModifica = c.IdUsuarioModificaNavigation?.Usuario
        };

        return Ok(vm);
    }
}
