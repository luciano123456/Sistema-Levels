using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

[Authorize]
public class GastosController : Controller
{
    private readonly IGastosService _service;

    public GastosController(IGastosService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    // =========================
    // LISTA
    // =========================
    [HttpGet]
    public async Task<IActionResult> Lista()
    {
        try
        {
            var gastos = await _service.ObtenerTodos();

            var lista = gastos.Select(g => new VMGasto
            {
                Id = g.Id,
                Fecha = g.Fecha,
                Concepto = g.Concepto,
                Importe = g.Importe,

                IdCategoria = g.IdCategoria,
                Categoria = g.IdCategoriaNavigation.Nombre,

                IdCuenta = g.IdCuenta,
                Cuenta = g.IdCuentaNavigation.Nombre,

                IdMoneda = g.IdMoneda,
                Moneda = g.IdMonedaNavigation.Nombre,

                IdPersonal = g.IdPersonal,
                Personal = g.IdPersonalNavigation != null
                    ? g.IdPersonalNavigation.Nombre
                    : "",

                IdUsuarioRegistra = g.IdUsuarioRegistra,
                FechaRegistra = g.FechaRegistra,
                UsuarioRegistra = g.IdUsuarioRegistraNavigation.Usuario,

                IdUsuarioModifica = g.IdUsuarioModifica,
                FechaModifica = g.FechaModifica,
                UsuarioModifica = g.IdUsuarioModificaNavigation.Usuario
            }).ToList();

            return Ok(lista);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> ListaFiltrada([FromBody] VMGastosFiltro f)
    {
        var gastos = await _service.ListarFiltrado(
            f.FechaDesde,
            f.FechaHasta,
            f.IdCategoria,
            f.IdMoneda,
            f.IdCuenta,
            f.IdPersonal,
            f.Concepto,
            f.ImporteMin
        );

        var lista = gastos.Select(x => new VMGasto
        {
            Id = x.Id,
            Fecha = x.Fecha,

            IdCategoria = x.IdCategoria,
            Categoria = x.IdCategoriaNavigation.Nombre,

            IdCuenta = x.IdCuenta,
            Cuenta = x.IdCuentaNavigation.Nombre,

            IdMoneda = x.IdMoneda,
            Moneda = x.IdMonedaNavigation.Nombre,

            IdPersonal = x.IdPersonal,
            Personal = x.IdPersonalNavigation != null
                ? x.IdPersonalNavigation.Nombre
                : null,

            Concepto = x.Concepto,
            Importe = x.Importe,
            NotaInterna = x.NotaInterna,

            FechaRegistra = x.FechaRegistra,
            UsuarioRegistra = x.IdUsuarioRegistraNavigation.Usuario,

            FechaModifica = x.FechaModifica,
            UsuarioModifica = x.IdUsuarioModificaNavigation != null
                ? x.IdUsuarioModificaNavigation.Usuario
                : null
        }).ToList();

        return Ok(lista);
    }


    // =========================
    // INSERTAR
    // =========================
    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] VMGasto model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var gasto = new Gasto
        {
            Fecha = model.Fecha,
            IdCategoria = model.IdCategoria,
            IdMoneda = model.IdMoneda,
            IdCuenta = model.IdCuenta,
            IdPersonal = model.IdPersonal,
            Concepto = model.Concepto,
            Importe = model.Importe,
            NotaInterna = model.NotaInterna,

            IdUsuarioRegistra = idUsuario,
            FechaRegistra = DateTime.Now
        };

        bool respuesta = await _service.Insertar(gasto);
        return Ok(new { valor = respuesta });
    }

    // =========================
    // ACTUALIZAR
    // =========================
    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] VMGasto model)
    {
        int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

        var gasto = new Gasto
        {
            Id = model.Id,
            Fecha = model.Fecha,
            IdCategoria = model.IdCategoria,
            IdMoneda = model.IdMoneda,
            IdCuenta = model.IdCuenta,
            IdPersonal = model.IdPersonal,
            Concepto = model.Concepto,
            Importe = model.Importe,
            NotaInterna = model.NotaInterna,

            IdUsuarioModifica = idUsuario,
            FechaModifica = DateTime.Now
        };

        bool respuesta = await _service.Actualizar(gasto);
        return Ok(new { valor = respuesta });
    }

    // =========================
    // ELIMINAR
    // =========================
    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        bool respuesta = await _service.Eliminar(id);
        return Ok(new { valor = respuesta });
    }

    // =========================
    // EDITAR INFO
    // =========================
    [HttpGet]
    public async Task<IActionResult> EditarInfo(int id)
    {
        var g = await _service.Obtener(id);

        if (g == null)
            return NotFound();

        var vm = new VMGasto
        {
            Id = g.Id,
            Fecha = g.Fecha,
            IdCategoria = g.IdCategoria,
            IdCuenta = g.IdCuenta,
            IdMoneda = g.IdMoneda,
            IdPersonal = g.IdPersonal,
            Concepto = g.Concepto,
            Importe = g.Importe,
            NotaInterna = g.NotaInterna,

            FechaRegistra = g.FechaRegistra,
            UsuarioRegistra = g.IdUsuarioRegistraNavigation?.Usuario,

            FechaModifica = g.FechaModifica,
            UsuarioModifica = g.IdUsuarioModificaNavigation?.Usuario
        };

        return Ok(vm);
    }
}
