using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Common;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

[Authorize]
public class PersonalSueldosController : Controller
{
    private readonly IPersonalSueldosService _service;
    private readonly IPersonalService _personalService;

    public PersonalSueldosController(
        IPersonalSueldosService service,
        IPersonalService personalService)
    {
        _service = service;
        _personalService = personalService;
    }

    [AllowAnonymous]
    public IActionResult Index() => View();

    [AllowAnonymous]
    public IActionResult NuevoModif(int id = 0, int idPersonal = 0)
    {
        ViewBag.Id = id;
        ViewBag.IdPersonal = idPersonal;
        return View();
    }

    /* ===============================
       PERSONAL
    =============================== */

    [HttpGet]
    public async Task<IActionResult> ListaPersonal()
    {
        var personal = await _personalService.ObtenerTodos();

        var lista = personal
            .Select(p => new { p.Id, p.Nombre })
            .OrderBy(x => x.Nombre)
            .ToList();

        return Ok(lista);
    }

    /* ===============================
       LISTA FILTRADA
    =============================== */

    [HttpPost]
    public async Task<IActionResult> ListaFiltrada([FromBody] VMPersonalSueldosFiltro f)
    {
        var sueldos = await _service.ListarFiltrado(
            f.FechaDesde,
            f.FechaHasta,
            f.IdPersonal,
            f.IdMoneda,
            f.Estado
        );

        var lista = sueldos
            .Select(s => new VMPersonalSueldo
            {
                Id = s.Id,

                IdUsuarioRegistra = s.IdUsuarioRegistra,
                FechaRegistra = s.FechaRegistra,
                IdUsuarioModifica = s.IdUsuarioModifica,
                FechaModifica = s.FechaModifica,

                IdPersonalCc = s.IdPersonalCc,
                Fecha = s.Fecha,

                IdPersonal = s.IdPersonal,
                Personal = s.IdPersonalNavigation != null ? s.IdPersonalNavigation.Nombre : "",

                IdMoneda = s.IdMoneda,
                Moneda = s.IdMonedaNavigation != null ? s.IdMonedaNavigation.Nombre : "",

                Concepto = s.Concepto,
                ImporteTotal = s.ImporteTotal,

                ImportePagado = s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m,
                Saldo = s.ImporteTotal - (s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m),

                Estado =
                    (s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) <= 0m
                        ? "PENDIENTE"
                        : ((s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) >= s.ImporteTotal
                            ? "PAGADO"
                            : "PARCIAL"),

                NotaPersonal = s.NotaPersonal,
                NotaInterna = s.NotaInterna,

                UsuarioRegistra = s.IdUsuarioRegistraNavigation != null ? s.IdUsuarioRegistraNavigation.Usuario : "",
                UsuarioModifica = s.IdUsuarioModificaNavigation != null ? s.IdUsuarioModificaNavigation.Usuario : ""
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
        var sueldos = await _service.ObtenerTodos();

        var lista = sueldos
            .Select(s => new VMPersonalSueldo
            {
                Id = s.Id,

                IdUsuarioRegistra = s.IdUsuarioRegistra,
                FechaRegistra = s.FechaRegistra,
                IdUsuarioModifica = s.IdUsuarioModifica,
                FechaModifica = s.FechaModifica,

                IdPersonalCc = s.IdPersonalCc,
                Fecha = s.Fecha,

                IdPersonal = s.IdPersonal,
                Personal = s.IdPersonalNavigation != null ? s.IdPersonalNavigation.Nombre : "",

                IdMoneda = s.IdMoneda,
                Moneda = s.IdMonedaNavigation != null ? s.IdMonedaNavigation.Nombre : "",

                Concepto = s.Concepto,
                ImporteTotal = s.ImporteTotal,

                ImportePagado = s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m,
                Saldo = s.ImporteTotal - (s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m),

                Estado =
                    (s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) <= 0m
                        ? "PENDIENTE"
                        : ((s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) >= s.ImporteTotal
                            ? "PAGADO"
                            : "PARCIAL"),

                NotaPersonal = s.NotaPersonal,
                NotaInterna = s.NotaInterna,

                UsuarioRegistra = s.IdUsuarioRegistraNavigation != null ? s.IdUsuarioRegistraNavigation.Usuario : "",
                UsuarioModifica = s.IdUsuarioModificaNavigation != null ? s.IdUsuarioModificaNavigation.Usuario : ""
            })
            .OrderByDescending(x => x.Fecha)
            .ToList();

        return Ok(lista);
    }

    /* ===============================
       SUELDOS POR PERSONAL
    =============================== */

    [HttpGet]
    public async Task<IActionResult> ListaPorPersonal(int idPersonal)
    {
        var sueldos = await _service.ObtenerPorPersonal(idPersonal);

        var lista = sueldos
            .Select(s => new VMPersonalSueldo
            {
                Id = s.Id,

                IdUsuarioRegistra = s.IdUsuarioRegistra,
                FechaRegistra = s.FechaRegistra,
                IdUsuarioModifica = s.IdUsuarioModifica,
                FechaModifica = s.FechaModifica,

                IdPersonalCc = s.IdPersonalCc,
                Fecha = s.Fecha,

                IdPersonal = s.IdPersonal,
                Personal = s.IdPersonalNavigation != null ? s.IdPersonalNavigation.Nombre : "",

                IdMoneda = s.IdMoneda,
                Moneda = s.IdMonedaNavigation != null ? s.IdMonedaNavigation.Nombre : "",

                Concepto = s.Concepto,
                ImporteTotal = s.ImporteTotal,

                ImportePagado = s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m,
                Saldo = s.ImporteTotal - (s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m),

                Estado =
                    (s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) <= 0m
                        ? "PENDIENTE"
                        : ((s.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) >= s.ImporteTotal
                            ? "PAGADO"
                            : "PARCIAL"),

                NotaPersonal = s.NotaPersonal,
                NotaInterna = s.NotaInterna,

                UsuarioRegistra = s.IdUsuarioRegistraNavigation != null ? s.IdUsuarioRegistraNavigation.Usuario : "",
                UsuarioModifica = s.IdUsuarioModificaNavigation != null ? s.IdUsuarioModificaNavigation.Usuario : ""
            })
            .OrderByDescending(x => x.Fecha)
            .ToList();

        return Ok(lista);
    }

    /* ===============================
       OBTENER SUELDO COMPLETO
    =============================== */

    [HttpGet]
    public async Task<IActionResult> EditarInfo(int id)
    {
        var s = await _service.Obtener(id);

        if (s == null)
            return NotFound();

        var vm = new VMPersonalSueldo
        {
            Id = s.Id,

            IdUsuarioRegistra = s.IdUsuarioRegistra,
            FechaRegistra = s.FechaRegistra,
            IdUsuarioModifica = s.IdUsuarioModifica,
            FechaModifica = s.FechaModifica,

            IdPersonalCc = s.IdPersonalCc,

            Fecha = s.Fecha,
            IdPersonal = s.IdPersonal,
            Personal = s.IdPersonalNavigation != null ? s.IdPersonalNavigation.Nombre : "",

            IdMoneda = s.IdMoneda,
            Moneda = s.IdMonedaNavigation != null ? s.IdMonedaNavigation.Nombre : "",

            Concepto = s.Concepto,
            ImporteTotal = s.ImporteTotal,

            ImportePagado = s.PersonalSueldosPagos.Sum(p => p.Conversion),
            Saldo = s.ImporteTotal - s.PersonalSueldosPagos.Sum(p => p.Conversion),

            Estado =
                s.PersonalSueldosPagos.Sum(p => p.Conversion) <= 0m
                    ? "PENDIENTE"
                    : (s.PersonalSueldosPagos.Sum(p => p.Conversion) >= s.ImporteTotal
                        ? "PAGADO"
                        : "PARCIAL"),

            NotaPersonal = s.NotaPersonal,
            NotaInterna = s.NotaInterna,

            UsuarioRegistra = s.IdUsuarioRegistraNavigation != null ? s.IdUsuarioRegistraNavigation.Usuario : "",
            UsuarioModifica = s.IdUsuarioModificaNavigation != null ? s.IdUsuarioModificaNavigation.Usuario : "",

            Pagos = s.PersonalSueldosPagos
                .Select(p => new VMPersonalSueldoPago
                {
                    Id = p.Id,

                    IdUsuarioRegistra = p.IdUsuarioRegistra,
                    FechaRegistra = p.FechaRegistra,
                    IdUsuarioModifica = p.IdUsuarioModifica,
                    FechaModifica = p.FechaModifica,

                    IdSueldo = p.IdSueldo,
                    IdPersonalCc = p.IdPersonalCc,
                    IdCaja = p.IdCaja,

                    Fecha = p.Fecha,

                    IdMoneda = p.IdMoneda,
                    Moneda = p.IdMonedaNavigation != null ? p.IdMonedaNavigation.Nombre : "",

                    IdCuenta = p.IdCuenta,
                    Cuenta = p.IdCuentaNavigation != null ? p.IdCuentaNavigation.Nombre : "",

                    Importe = p.Importe,
                    Cotizacion = p.Cotizacion,
                    Conversion = p.Conversion
                })
                .OrderBy(x => x.Fecha)
                .ToList()
        };

        return Ok(vm);
    }

    /* ===============================
       INSERTAR
    =============================== */

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] VMPersonalSueldo model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var sueldo = MapSueldo(model, idUsuario, false);

        var pagos = (model.Pagos ?? new()).Select(x => new PersonalSueldosPago
        {
            Id = x.Id,
            IdSueldo = 0,
            IdPersonalCc = x.IdPersonalCc,
            IdCaja = x.IdCaja,
            Fecha = x.Fecha == default ? DateTime.Now : x.Fecha,
            IdMoneda = x.IdMoneda,
            IdCuenta = x.IdCuenta,
            Importe = x.Importe,
            Cotizacion = x.Cotizacion <= 0 ? 1 : x.Cotizacion,
            Conversion = x.Conversion <= 0 ? x.Importe : x.Conversion,
            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        }).ToList();

        ServiceResult result = await _service.Insertar(sueldo, pagos);

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
    public async Task<IActionResult> Actualizar([FromBody] VMPersonalSueldo model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var sueldo = MapSueldo(model, idUsuario, true);

        var pagos = (model.Pagos ?? new()).Select(x => new PersonalSueldosPago
        {
            Id = x.Id,
            IdSueldo = model.Id,
            IdPersonalCc = x.IdPersonalCc,
            IdCaja = x.IdCaja,
            Fecha = x.Fecha == default ? DateTime.Now : x.Fecha,
            IdMoneda = x.IdMoneda,
            IdCuenta = x.IdCuenta,
            Importe = x.Importe,
            Cotizacion = x.Cotizacion <= 0 ? 1 : x.Cotizacion,
            Conversion = x.Conversion <= 0 ? x.Importe : x.Conversion,
            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        }).ToList();

        ServiceResult result = await _service.Actualizar(sueldo, pagos);

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

    private static PersonalSueldo MapSueldo(VMPersonalSueldo model, int idUsuario, bool esUpdate)
    {
        var sueldo = new PersonalSueldo
        {
            Id = model.Id,
            IdPersonalCc = model.IdPersonalCc,
            Fecha = model.Fecha,
            IdPersonal = model.IdPersonal,
            IdMoneda = model.IdMoneda,
            Concepto = model.Concepto,
            ImporteTotal = model.ImporteTotal,
            NotaPersonal = model.NotaPersonal,
            NotaInterna = model.NotaInterna
        };

        if (!esUpdate)
        {
            sueldo.IdUsuarioRegistra = idUsuario;
            sueldo.FechaRegistra = DateTime.Now;
        }
        else
        {
            sueldo.IdUsuarioModifica = idUsuario;
            sueldo.FechaModifica = DateTime.Now;
        }

        return sueldo;
    }
}