using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Common;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

[Authorize]
public class VentasController : Controller
{
    private readonly IVentasService _service;
    private readonly IClientesService _clientesService;

    public VentasController(IVentasService service, IClientesService clientesService)
    {
        _service = service;
        _clientesService = clientesService;
    }

    [AllowAnonymous]
    public IActionResult Index() => View();


    [AllowAnonymous]
    public IActionResult NuevoModif(int id = 0, int idCliente = 0)
    {
        ViewBag.Id = id;
        ViewBag.IdCliente = idCliente;

        return View();
    }


    /* ===============================
       PANEL IZQUIERDA: CLIENTES
    =============================== */

    [HttpGet]
    public async Task<IActionResult> ListaClientes()
    {
        var clientes = await _clientesService.ObtenerTodos();

        var lista = clientes
            .Select(c => new { c.Id, c.Nombre })
            .OrderBy(x => x.Nombre)
            .ToList();

        return Ok(lista);
    }

    /* ===============================
   PANEL DERECHA: TODAS LAS VENTAS
=============================== */

    [HttpGet]
    public async Task<IActionResult> Lista()
    {
        var ventas = await _service.ObtenerTodos();

        var lista = ventas
            .Select(v => new VMVenta
            {
                Id = v.Id,
                Fecha = v.Fecha,
                NombreEvento = v.NombreEvento,

                IdCliente = v.IdCliente,
                Cliente = v.IdClienteNavigation != null
                    ? v.IdClienteNavigation.Nombre
                    : "",

                IdProductora = v.IdProductora,
                Productora = v.IdProductoraNavigation != null
                    ? v.IdProductoraNavigation.Nombre
                    : "",

                IdUbicacion = v.IdUbicacion,
                Ubicacion = v.IdUbicacionNavigation != null
                    ? v.IdUbicacionNavigation.Descripcion
                    : "",

                IdMoneda = v.IdMoneda,
                Moneda = v.IdMonedaNavigation != null
                    ? v.IdMonedaNavigation.Nombre
                    : "",

                IdEstado = v.IdEstado,
                Estado = v.IdEstadoNavigation != null
                    ? v.IdEstadoNavigation.Nombre
                    : "",

                ImporteTotal = v.ImporteTotal,
                ImporteAbonado = v.ImporteAbonado,
                Saldo = v.Saldo
            })
            .OrderByDescending(x => x.Fecha)
            .ToList();

        return Ok(lista);
    }

    /* ===============================
       PANEL DERECHA: VENTAS POR CLIENTE
    =============================== */

    [HttpGet]
    public async Task<IActionResult> ListaPorCliente(int idCliente)
    {
        var ventas = await _service.ObtenerPorCliente(idCliente);

        var lista = ventas.Select(v => new VMVenta
        {
            Id = v.Id,
            Fecha = v.Fecha,
            NombreEvento = v.NombreEvento,

            IdCliente = v.IdCliente,
            Cliente = v.IdClienteNavigation != null ? v.IdClienteNavigation.Nombre : "",

            IdProductora = v.IdProductora,
            Productora = v.IdProductoraNavigation != null ? v.IdProductoraNavigation.Nombre : "",

            IdUbicacion = v.IdUbicacion,
            Ubicacion = v.IdUbicacionNavigation != null ? v.IdUbicacionNavigation.Descripcion : "",

            IdMoneda = v.IdMoneda,
            Moneda = v.IdMonedaNavigation != null ? v.IdMonedaNavigation.Nombre : "",

            IdEstado = v.IdEstado,
            Estado = v.IdEstadoNavigation != null ? v.IdEstadoNavigation.Nombre : "",

            ImporteTotal = v.ImporteTotal,
            ImporteAbonado = v.ImporteAbonado,
            Saldo = v.Saldo
        }).OrderByDescending(x => x.Fecha).ToList();

        return Ok(lista);
    }

    /* ===============================
       EDITAR INFO (venta completa)
    =============================== */

    [HttpGet]
    public async Task<IActionResult> EditarInfo(int id)
    {
        var v = await _service.Obtener(id);
        if (v == null) return NotFound();

        var vm = new VMVenta
        {
            Id = v.Id,

            Fecha = v.Fecha,
            IdUbicacion = v.IdUbicacion,
            NombreEvento = v.NombreEvento,
            Duracion = v.Duracion,

            IdCliente = v.IdCliente,
            IdProductora = v.IdProductora,
            IdMoneda = v.IdMoneda,
            IdEstado = v.IdEstado,

            ImporteTotal = v.ImporteTotal,
            ImporteAbonado = v.ImporteAbonado,
            Saldo = v.Saldo,

            NotaInterna = v.NotaInterna,
            NotaCliente = v.NotaCliente,

            IdTipoContrato = v.IdTipoContrato,
            IdOpExclusividad = v.IdOpExclusividad,
            DiasPrevios = v.DiasPrevios,
            FechaHasta = v.FechaHasta,

            Artistas = v.VentasArtista.Select(a => new VMVentaArtista
            {
                Id = a.Id,
                IdArtista = a.IdArtista,
                Artista = a.IdArtistaNavigation != null ? a.IdArtistaNavigation.Nombre : "",
                IdRepresentante = a.IdRepresentante,
                Representante = a.IdRepresentanteNavigation != null ? a.IdRepresentanteNavigation.Nombre : "",
                PorcComision = a.PorcComision,
                TotalComision = a.TotalComision
            }).ToList(),

            Personal = v.VentasPersonals.Select(p => new VMVentaPersonal
            {
                Id = p.Id,
                IdPersonal = p.IdPersonal,
                Personal = p.IdPersonalNavigation != null ? p.IdPersonalNavigation.Nombre : "",
                IdCargo = p.IdCargo,
                Cargo = p.IdCargoNavigation != null ? p.IdCargoNavigation.Nombre : "",
                IdTipoComision = p.IdTipoComision,
                TipoComision = p.IdTipoComisionNavigation != null ? p.IdTipoComisionNavigation.Nombre : "",
                PorcComision = p.PorcComision,
                TotalComision = p.TotalComision
            }).ToList(),

            Cobros = v.VentasCobros.Select(c => new VMVentaCobro
            {
                Id = c.Id,
                Fecha = c.Fecha,
                IdMoneda = c.IdMoneda,
                Moneda = c.IdMonedaNavigation != null ? c.IdMonedaNavigation.Nombre : "",
                IdCuenta = c.IdCuenta,
                Cuenta = c.IdCuentaNavigation != null ? c.IdCuentaNavigation.Nombre : "",
                Importe = c.Importe,
                Cotizacion = c.Cotizacion,
                Conversion = c.Conversion,
                NotaCliente = c.NotaCliente,
                NotaInterna = c.NotaInterna
            }).OrderBy(x => x.Fecha).ToList(),

            FechaRegistra = v.FechaRegistra,
            UsuarioRegistra = v.IdUsuarioRegistraNavigation?.Usuario,
            FechaModifica = v.FechaModifica,
            UsuarioModifica = v.IdUsuarioModificaNavigation?.Usuario
        };

        return Ok(vm);
    }

    /* ===============================
       INSERTAR
    =============================== */

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] VMVenta model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var venta = MapVenta(model, idUsuario, esUpdate: false);

        var artistas = (model.Artistas ?? new()).Select(x => new VentasArtista
        {
            IdVenta = 0,
            IdArtista = x.IdArtista,
            IdRepresentante = x.IdRepresentante,
            PorcComision = x.PorcComision,
            TotalComision = x.TotalComision,
            IdArtistaCc = 0
        }).ToList();

        var personal = (model.Personal ?? new()).Select(x => new VentasPersonal
        {
            IdVenta = 0,
            IdPersonal = x.IdPersonal,
            IdCargo = x.IdCargo,
            IdTipoComision = x.IdTipoComision,
            PorcComision = x.PorcComision,
            TotalComision = x.TotalComision,
            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        }).ToList();

        var cobros = (model.Cobros ?? new()).Select(x => new VentasCobro
        {
            IdVenta = 0,

            IdClienteCc = null,
            IdArtistaCc = null,
            IdCaja = null,

            Fecha = x.Fecha == default ? DateTime.Now : x.Fecha,
            IdMoneda = x.IdMoneda,
            IdCuenta = x.IdCuenta,
            Importe = x.Importe,

            Cotizacion = x.Cotizacion <= 0 ? 1 : x.Cotizacion,
            Conversion = x.Conversion <= 0 ? x.Importe : x.Conversion,

            NotaCliente = x.NotaCliente,
            NotaInterna = x.NotaInterna,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        }).ToList();

        ServiceResult result = await _service.Insertar(venta, artistas, personal, cobros);

        return Ok(new { valor = result.Ok, mensaje = result.Mensaje, tipo = result.Tipo, idReferencia = result.IdReferencia });
    }

    /* ===============================
       ACTUALIZAR
    =============================== */

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMVenta model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var venta = MapVenta(model, idUsuario, esUpdate: true);

        var artistas = (model.Artistas ?? new()).Select(x => new VentasArtista
        {
            IdVenta = model.Id,
            IdArtista = x.IdArtista,
            IdRepresentante = x.IdRepresentante,
            PorcComision = x.PorcComision,
            TotalComision = x.TotalComision,
            IdArtistaCc = 0
        }).ToList();

        var personal = (model.Personal ?? new()).Select(x => new VentasPersonal
        {
            IdVenta = model.Id,
            IdPersonal = x.IdPersonal,
            IdCargo = x.IdCargo,
            IdTipoComision = x.IdTipoComision,
            PorcComision = x.PorcComision,
            TotalComision = x.TotalComision,
            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        }).ToList();

        var cobros = (model.Cobros ?? new()).Select(x => new VentasCobro
        {
            IdVenta = model.Id,

            IdClienteCc = null,
            IdArtistaCc = null,
            IdCaja = null,

            Fecha = x.Fecha == default ? DateTime.Now : x.Fecha,
            IdMoneda = x.IdMoneda,
            IdCuenta = x.IdCuenta,
            Importe = x.Importe,

            Cotizacion = x.Cotizacion <= 0 ? 1 : x.Cotizacion,
            Conversion = x.Conversion <= 0 ? x.Importe : x.Conversion,

            NotaCliente = x.NotaCliente,
            NotaInterna = x.NotaInterna,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        }).ToList();

        ServiceResult result = await _service.Actualizar(venta, artistas, personal, cobros);

        return Ok(new { valor = result.Ok, mensaje = result.Mensaje, tipo = result.Tipo, idReferencia = result.IdReferencia });
    }

    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        ServiceResult result = await _service.Eliminar(id);
        return Ok(new { valor = result.Ok, mensaje = result.Mensaje, tipo = result.Tipo, idReferencia = result.IdReferencia });
    }

    private static Venta MapVenta(VMVenta model, int idUsuario, bool esUpdate)
    {
        var venta = new Venta
        {
            Id = model.Id,

            Fecha = model.Fecha,
            IdUbicacion = model.IdUbicacion,
            NombreEvento = model.NombreEvento,
            Duracion = model.Duracion,

            IdCliente = model.IdCliente,
            IdProductora = model.IdProductora,
            IdMoneda = model.IdMoneda,
            IdEstado = model.IdEstado,

            ImporteTotal = model.ImporteTotal,

            NotaInterna = model.NotaInterna,
            NotaCliente = model.NotaCliente,

            IdTipoContrato = model.IdTipoContrato,
            IdOpExclusividad = model.IdOpExclusividad,
            DiasPrevios = model.DiasPrevios,
            FechaHasta = model.FechaHasta,

            // CC sin uso
            IdClienteCc = null,
            IdPresupuesto = null
        };

        if (!esUpdate)
        {
            venta.IdUsuarioRegistra = idUsuario;
            venta.FechaRegistra = DateTime.Now;
        }
        else
        {
            venta.IdUsuarioModifica = idUsuario;
            venta.FechaModifica = DateTime.Now;
        }

        return venta;
    }
}