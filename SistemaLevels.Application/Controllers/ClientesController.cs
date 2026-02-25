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

    /* ===============================
       LISTA (GRILLA)
    =============================== */

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

            Dni = c.Dni,
            IdTipoDocumento = c.IdTipoDocumento,
            NumeroDocumento = c.NumeroDocumento,

            IdPais = c.IdPais,
            IdProvincia = c.IdProvincia,
            Localidad = c.Localidad,
            EntreCalles = c.EntreCalles,
            Direccion = c.Direccion,
            CodigoPostal = c.CodigoPostal,
            IdCondicionIva = c.IdCondicionIva,

            AsociacionAutomatica = c.AsociacionAutomatica,

            Pais = c.IdPaisNavigation != null ? c.IdPaisNavigation.Nombre : "",
            Provincia = c.IdProvinciaNavigation != null ? c.IdProvinciaNavigation.Nombre : "",
            TipoDocumento = c.IdTipoDocumentoNavigation != null ? c.IdTipoDocumentoNavigation.Nombre : "",
            CondicionIva = c.IdCondicionIvaNavigation != null ? c.IdCondicionIvaNavigation.Nombre : "",

            // texto de productoras (si querés mostrarlo en grilla)
            Productora = (c.ClientesProductoras != null && c.ClientesProductoras.Count > 0)
                ? (c.ClientesProductoras.Count == 1
                    ? (c.ClientesProductoras.First().IdProductoraNavigation != null
                        ? c.ClientesProductoras.First().IdProductoraNavigation.Nombre
                        : "1 asignada")
                    : $"{c.ClientesProductoras.Count} asignadas")
                : "",

            IdUsuarioRegistra = c.IdUsuarioRegistra,
            FechaRegistra = c.FechaRegistra,
            UsuarioRegistra = c.IdUsuarioRegistraNavigation != null ? c.IdUsuarioRegistraNavigation.Usuario : "",

            IdUsuarioModifica = c.IdUsuarioModifica,
            FechaModifica = c.FechaModifica,
            UsuarioModifica = c.IdUsuarioModificaNavigation != null ? c.IdUsuarioModificaNavigation.Usuario : ""
        }).ToList();

        return Ok(lista);
    }

    /* ===============================
       INSERTAR
    =============================== */

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

            IdPais = model.IdPais,
            IdProvincia = model.IdProvincia,
            Localidad = model.Localidad,
            EntreCalles = model.EntreCalles,
            Direccion = model.Direccion,
            CodigoPostal = model.CodigoPostal,

            IdCondicionIva = model.IdCondicionIva,
            AsociacionAutomatica = model.AsociacionAutomatica,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        };

        bool respuesta = await _service.Insertar(cliente, model.ProductorasIds ?? new List<int>());
        return Ok(new { valor = respuesta });
    }

    /* ===============================
       ACTUALIZAR
    =============================== */

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMCliente model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        model.ProductorasIds ??= new List<int>();

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
            IdPais = model.IdPais,
            IdProvincia = model.IdProvincia,
            Localidad = model.Localidad,
            EntreCalles = model.EntreCalles,
            Direccion = model.Direccion,
            CodigoPostal = model.CodigoPostal,
            IdCondicionIva = model.IdCondicionIva,
            AsociacionAutomatica = model.AsociacionAutomatica,
            IdUsuarioModifica = idUsuario,
            FechaModifica = DateTime.Now
        };

        bool respuesta =
            await _service.Actualizar(cliente, model.ProductorasIds);

        return Ok(new { valor = respuesta });
    }
    /* ===============================
       ELIMINAR
    =============================== */

    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        bool respuesta = await _service.Eliminar(id);
        return Ok(new { valor = respuesta });
    }

    /* ===============================
       EDITAR INFO (MODAL)
       - Devuelve SOLO productoras MANUALES del cliente
       - No mezcla automáticas / no mezcla asignadas desde productora
    =============================== */

    [HttpGet]
    public async Task<IActionResult> EditarInfo(int id)
    {
        var c = await _service.Obtener(id);

        if (c == null)
            return NotFound();

        var manual =
     c.ClientesProductoras
         .Where(x => x.OrigenAsignacion == 2) // CLIENTE
         .Select(x => x.IdProductora)
         .ToList();

        var automaticas =
            c.ClientesProductoras
                .Where(x => x.OrigenAsignacion == 3)
                .Select(x => x.IdProductora)
                .ToList();

        var desdeProductora =
            c.ClientesProductoras
                .Where(x => x.OrigenAsignacion == 1) // PRODUCTORA
                .Select(x => x.IdProductora)
                .ToList();

        return Ok(new
        {
            c.Id,
            c.Nombre,
            c.Telefono,
            c.TelefonoAlternativo,
            c.Email,

            c.IdPais,
            c.IdProvincia,
            c.IdTipoDocumento,
            c.NumeroDocumento,
            c.IdCondicionIva,

            c.Direccion,
            c.Localidad,
            c.EntreCalles,
            c.CodigoPostal,

            c.AsociacionAutomatica,

            ProductorasManualIds = manual,
            ProductorasAutomaticasIds = automaticas,
            ProductorasDesdeProductoraIds = desdeProductora,

            c.FechaRegistra,
            UsuarioRegistra = c.IdUsuarioRegistraNavigation?.Usuario,
            c.FechaModifica,
            UsuarioModifica = c.IdUsuarioModificaNavigation?.Usuario
        });
    }
}