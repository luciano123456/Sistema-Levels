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

    /* ===============================
       LISTA (GRILLA)
    =============================== */

    [HttpGet]
    public async Task<IActionResult> Lista()
    {
        var productoras = await _service.ObtenerTodos();

        var lista = productoras.Select(p => new VMProductora
        {
            Id = p.Id,
            Nombre = p.Nombre,

            Telefono = p.Telefono,
            Email = p.Email,
            Direccion = p.Direccion,

            Pais = p.IdpaisNavigation != null ? p.IdpaisNavigation.Nombre : "",
            TipoDocumento = p.IdTipoDocumentoNavigation != null ? p.IdTipoDocumentoNavigation.Nombre : "",
            CondicionIva = p.IdCondicionIvaNavigation != null ? p.IdCondicionIvaNavigation.Nombre : "",
            Provincia = p.IdProvinciaNavigation != null ? p.IdProvinciaNavigation.Nombre : "",

            IdUsuarioRegistra = p.IdUsuarioRegistra,
            FechaRegistra = p.FechaRegistra,
            UsuarioRegistra = p.IdUsuarioRegistraNavigation != null ? p.IdUsuarioRegistraNavigation.Usuario : "",

            IdUsuarioModifica = p.IdUsuarioModifica,
            FechaModifica = p.FechaModifica,
            UsuarioModifica = p.IdUsuarioModificaNavigation != null ? p.IdUsuarioModificaNavigation.Usuario : "",

            AsociacionAutomatica = (p.AsociacionAutomatica ?? 0) == 1
        }).ToList();

        return Ok(lista);
    }

    /* ===============================
       INSERTAR
    =============================== */

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

    /* ===============================
       ACTUALIZAR
    =============================== */

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
       - manuales: OrigenAsignacion=1
       - automáticos: OrigenAsignacion=2
    =============================== */

    [HttpGet]
    public async Task<IActionResult> EditarInfo(int id)
    {
        var p = await _service.Obtener(id);

        if (p == null)
            return NotFound();

        var relaciones = p.ClientesProductoras ?? new List<ClientesProductora>();

        // manuales creados DESDE PRODUCTORA
        // ✅ manuales creados DESDE PRODUCTORA
        var clientesManuales = relaciones
            .Where(x => x.OrigenAsignacion == 1)
            .Select(x => x.IdCliente)
            .Distinct()
            .ToList();

        // ✅ creados manualmente DESDE CLIENTE
        var clientesDesdeCliente = relaciones
            .Where(x => x.OrigenAsignacion == 2)
            .Select(x => x.IdCliente)
            .Distinct()
            .ToList();

        // ✅ automáticos reales
        var clientesAutomaticos = relaciones
            .Where(x => x.OrigenAsignacion == 3)
            .Select(x => x.IdCliente)
            .Distinct()
            .ToList();

        // VM base
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

            // 🔵 solo manuales se guardan desde Productoras
            ClientesIds = clientesManuales,

            FechaRegistra = p.FechaRegistra,
            UsuarioRegistra = p.IdUsuarioRegistraNavigation?.Usuario,

            FechaModifica = p.FechaModifica,
            UsuarioModifica = p.IdUsuarioModificaNavigation?.Usuario
        };

        // Respuesta extendida (como venías haciendo)
        return Ok(new
        {
            vm.Id,
            vm.Nombre,
            vm.Telefono,
            vm.TelefonoAlternativo,
            vm.Dni,
            vm.Idpais,
            vm.IdTipoDocumento,
            vm.NumeroDocumento,
            vm.IdCondicionIva,
            vm.Email,
            vm.IdProvincia,
            vm.Localidad,
            vm.EntreCalles,
            vm.Direccion,
            vm.CodigoPostal,
            vm.AsociacionAutomatica,

            clientesManualesIds = clientesManuales,
            clientesAutomaticosIds = clientesAutomaticos,
            clientesDesdeClienteIds = clientesDesdeCliente,

            vm.FechaRegistra,
            vm.UsuarioRegistra,
            vm.FechaModifica,
            vm.UsuarioModifica
        });
    }
}