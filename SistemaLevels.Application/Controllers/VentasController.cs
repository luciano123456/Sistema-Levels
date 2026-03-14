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
       CLIENTES
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
       LISTA FILTRADA
    =============================== */

    [HttpPost]
    public async Task<IActionResult> ListaFiltrada([FromBody] VMVentasFiltro f)
    {
        var ventas = await _service.ListarFiltrado(
            f.FechaDesde,
            f.FechaHasta,
            f.IdEstado,
            f.IdArtista,
            f.IdCliente
        );

        var lista = ventas
        .Select(v => new VMVenta
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
        })
        .OrderByDescending(x => x.Fecha)
        .ToList();

        return Ok(lista);
    }

    /* ===============================
       LISTA GENERAL
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
        })
        .OrderByDescending(x => x.Fecha)
        .ToList();

        return Ok(lista);
    }

    /* ===============================
       VENTAS POR CLIENTE
    =============================== */

    [HttpGet]
    public async Task<IActionResult> ListaPorCliente(int idCliente)
    {
        var ventas = await _service.ObtenerPorCliente(idCliente);

        var lista = ventas
        .Select(v => new VMVenta
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
        })
        .OrderByDescending(x => x.Fecha)
        .ToList();

        return Ok(lista);
    }

    /* ===============================
       OBTENER VENTA COMPLETA
    =============================== */

    [HttpGet]
    public async Task<IActionResult> EditarInfo(int id)
    {
        var v = await _service.Obtener(id);

        if (v == null)
            return NotFound();

        var vm = new VMVenta
        {
            Id = v.Id,

            // =============================
            // DATOS PRINCIPALES
            // =============================

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

            IdClienteCc = v.IdClienteCc,

            // =============================
            // CLIENTE
            // =============================

            Cliente = v.IdClienteNavigation?.Nombre ?? "",
            DniCliente = v.IdClienteNavigation?.Dni ?? "",
            CuitCliente = v.IdClienteNavigation?.NumeroDocumento ?? "",
            DomicilioCliente = v.IdClienteNavigation?.Direccion ?? "",
            TelefonoCliente = v.IdClienteNavigation?.Telefono ?? "",
            EmailCliente = v.IdClienteNavigation?.Email ?? "",

            // =============================
            // PRODUCTORA
            // =============================

            Productora = v.IdProductoraNavigation?.Nombre ?? "",

            // =============================
            // UBICACION
            // =============================

            Ubicacion = v.IdUbicacionNavigation?.Descripcion ?? "",

            // =============================
            // MONEDA
            // =============================

            Moneda = v.IdMonedaNavigation?.Nombre ?? "",

            // =============================
            // ESTADO
            // =============================

            Estado = v.IdEstadoNavigation?.Nombre ?? "",

            // =============================
            // ARTISTAS
            // =============================

            Artistas = v.VentasArtista.Select(a => new VMVentaArtista
            {
                Id = a.Id,
                IdArtista = a.IdArtista,
                IdRepresentante = a.IdRepresentante,
                PorcComision = a.PorcComision,
                TotalComision = a.TotalComision,
                IdArtistaCc = a.IdArtistaCc,

                Artista = a.IdArtistaNavigation?.NombreArtistico
                          ?? a.IdArtistaNavigation?.Nombre
                          ?? "",

                Representante = a.IdRepresentanteNavigation?.Nombre ?? "",

                DniArtista = a.IdArtistaNavigation?.Dni ?? "",
                CuitArtista = a.IdArtistaNavigation?.NumeroDocumento ?? "",
                DomicilioArtista = a.IdArtistaNavigation?.Direccion ?? "",

                DniRepresentante = a.IdRepresentanteNavigation?.Dni ?? "",
                CuitRepresentante = a.IdRepresentanteNavigation?.NumeroDocumento ?? "",
                DomicilioRepresentante = a.IdRepresentanteNavigation?.Direccion ?? ""

            }).ToList(),

            // =============================
            // PERSONAL
            // =============================

            Personal = v.VentasPersonals.Select(p => new VMVentaPersonal
            {
                Id = p.Id,
                IdPersonal = p.IdPersonal,
                IdCargo = p.IdCargo,
                IdTipoComision = p.IdTipoComision,
                PorcComision = p.PorcComision,
                TotalComision = p.TotalComision,

                Personal = p.IdPersonalNavigation?.Nombre ?? "",
                Cargo = p.IdCargoNavigation?.Nombre ?? "",
                TipoComision = p.IdTipoComisionNavigation?.Nombre ?? ""

            }).ToList(),

            // =============================
            // COBROS
            // =============================

            Cobros = v.VentasCobros
                .Select(c => new VMVentaCobro
                {
                    Id = c.Id,
                    Fecha = c.Fecha,
                    IdMoneda = c.IdMoneda,
                    IdCuenta = c.IdCuenta,

                    Importe = c.Importe,
                    Cotizacion = c.Cotizacion,
                    Conversion = c.Conversion,

                    NotaCliente = c.NotaCliente,
                    NotaInterna = c.NotaInterna,

                    IdClienteCc = c.IdClienteCc,
                    IdArtistaCc = c.IdArtistaCc,
                    IdCaja = c.IdCaja,

                    Moneda = c.IdMonedaNavigation?.Nombre ?? "",
                    Cuenta = c.IdCuentaNavigation?.Nombre ?? ""

                })
                .OrderBy(x => x.Fecha)
                .ToList(),


            IdUsuarioRegistra = v.IdUsuarioRegistra,
            FechaRegistra = v.FechaRegistra,
            UsuarioRegistra = v.IdUsuarioRegistraNavigation?.Usuario ?? "",

            IdUsuarioModifica = v.IdUsuarioModifica,
            FechaModifica = v.FechaModifica,
            UsuarioModifica = v.IdUsuarioModificaNavigation?.Usuario ?? ""
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

        var venta = MapVenta(model, idUsuario, false);

        var artistas = (model.Artistas ?? new()).Select(x => new VentasArtista
        {
            Id = x.Id,
            IdVenta = 0,
            IdArtista = x.IdArtista,
            IdRepresentante = x.IdRepresentante,
            PorcComision = x.PorcComision,
            TotalComision = x.TotalComision,
            IdArtistaCc = x.IdArtistaCc ?? 0
        }).ToList();

        var personal = (model.Personal ?? new()).Select(x => new VentasPersonal
        {
            Id = x.Id,
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
            Id = x.Id,
            IdVenta = 0,
            IdClienteCc = x.IdClienteCc,
            IdArtistaCc = x.IdArtistaCc,
            IdCaja = x.IdCaja,
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

        return Ok(new
        {
            valor = result.Ok,
            mensaje = result.Mensaje,
            tipo = result.Tipo,
            idReferencia = result.IdReferencia
        });
    }

    /* ===============================
       ACTUALIZAR
    =============================== */

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMVenta model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var venta = MapVenta(model, idUsuario, true);

        var artistas = (model.Artistas ?? new()).Select(x => new VentasArtista
        {
            Id = x.Id,
            IdVenta = model.Id,
            IdArtista = x.IdArtista,
            IdRepresentante = x.IdRepresentante,
            PorcComision = x.PorcComision,
            TotalComision = x.TotalComision,
            IdArtistaCc = x.IdArtistaCc ?? 0
        }).ToList();

        var personal = (model.Personal ?? new()).Select(x => new VentasPersonal
        {
            Id = x.Id,
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
            Id = x.Id,
            IdVenta = model.Id,
            IdClienteCc = x.IdClienteCc,
            IdArtistaCc = x.IdArtistaCc,
            IdCaja = x.IdCaja,
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

        return Ok(new
        {
            valor = result.Ok,
            mensaje = result.Mensaje,
            tipo = result.Tipo,
            idReferencia = result.IdReferencia
        });
    }

    /* ===============================
       ELIMINAR
    =============================== */

    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        ServiceResult result = await _service.Eliminar(id);

        return Ok(new
        {
            valor = result.Ok,
            mensaje = result.Mensaje,
            tipo = result.Tipo,
            idReferencia = result.IdReferencia
        });
    }

    /* ===============================
       MAPEO
    =============================== */

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
            IdClienteCc = model.IdClienteCc,
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