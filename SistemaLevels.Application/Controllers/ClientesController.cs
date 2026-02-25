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

        var lista = clientes
            .Select(c => new VMCliente
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

                // nombres descriptivos (para grilla)
                Pais = c.IdPaisNavigation != null ? c.IdPaisNavigation.Nombre : "",
                Provincia = c.IdProvinciaNavigation != null ? c.IdProvinciaNavigation.Nombre : "",
                TipoDocumento = c.IdTipoDocumentoNavigation != null ? c.IdTipoDocumentoNavigation.Nombre : "",
                CondicionIva = c.IdCondicionIvaNavigation != null ? c.IdCondicionIvaNavigation.Nombre : "",

                // si tu grilla sigue mostrando "Productora", armamos un texto:
                // - si hay 1: nombre
                // - si hay varias: "N asignadas"
                Productora = (c.ClientesProductorasAsignada != null && c.ClientesProductorasAsignada.Count > 0)
                    ? (c.ClientesProductorasAsignada.Count == 1
                        ? (c.ClientesProductorasAsignada.First().IdNavigation != null ? c.ClientesProductorasAsignada.First().IdNavigation.Nombre : "1 asignada")
                        : $"{c.ClientesProductorasAsignada.Count} asignadas")
                    : "",

                // auditoría
                IdUsuarioRegistra = c.IdUsuarioRegistra,
                FechaRegistra = c.FechaRegistra,
                UsuarioRegistra = c.IdUsuarioRegistraNavigation != null ? c.IdUsuarioRegistraNavigation.Usuario : "",

                IdUsuarioModifica = c.IdUsuarioModifica,
                FechaModifica = c.FechaModifica,
                UsuarioModifica = c.IdUsuarioModificaNavigation != null ? c.IdUsuarioModificaNavigation.Usuario : ""
            })
            .ToList();

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

        bool respuesta = await _service.Insertar(cliente, model.ProductorasIds);
        return Ok(new { valor = respuesta });
    }

    /* ===============================
       ACTUALIZAR
    =============================== */

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMCliente model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

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

        bool respuesta = await _service.Actualizar(cliente, model.ProductorasIds);
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
    =============================== */

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

            IdPais = c.IdPais,
            IdProvincia = c.IdProvincia,

            Localidad = c.Localidad,
            EntreCalles = c.EntreCalles,
            Direccion = c.Direccion,
            CodigoPostal = c.CodigoPostal,

            IdCondicionIva = c.IdCondicionIva,
            AsociacionAutomatica = c.AsociacionAutomatica,

            // ✅ NUEVO: productoras asignadas desde tabla puente
            ProductorasIds = (c.ClientesProductorasAsignada ?? new List<ClientesProductorasAsignada>())
                .Where(x => x.IdProductora.HasValue)
                .Select(x => x.IdProductora!.Value)
                .Distinct()
                .ToList(),

            // auditoría
            FechaRegistra = c.FechaRegistra,
            UsuarioRegistra = c.IdUsuarioRegistraNavigation?.Usuario,

            FechaModifica = c.FechaModifica,
            UsuarioModifica = c.IdUsuarioModificaNavigation?.Usuario
        };

        return Ok(vm);
    }
}