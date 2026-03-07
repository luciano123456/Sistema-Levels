using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;

namespace SistemaLevels.Controllers
{
    public class PersonalCuentaCorrienteController : Controller
    {
        private readonly IPersonalCuentaCorrienteService _service;

        public PersonalCuentaCorrienteController(IPersonalCuentaCorrienteService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ListaPersonal(string? buscar)
        {
            var data = await _service.ListarPersonal(buscar);

            var lista = data.Select(x => new VMPersonalCuentaCorrientePersonal
            {
                Id = x.personal.Id,
                Nombre = x.personal.Nombre,
                Saldo = x.saldo
            });

            return Ok(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Movimiento(int id)
        {
            var (mov, cuenta, saldo) = await _service.ObtenerMovimiento(id);

            if (mov == null)
                return NotFound();

            return Ok(new
            {
                mov.Id,
                mov.IdPersonal,
                mov.IdMoneda,
                mov.TipoMov,
                mov.Fecha,
                mov.Concepto,
                mov.Debe,
                mov.Haber,
                mov.IdMov,
                Moneda = mov.IdMonedaNavigation?.Nombre,
                cuenta,
                saldo
            });
        }

        [HttpPost]
        public async Task<IActionResult> Movimientos([FromBody] VMPersonalCuentaCorrienteFiltro filtro)
        {
            var movimientos = await _service.Movimientos(
                filtro.IdPersonal.Value,
                filtro.IdMoneda,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.TipoMov,
                filtro.Texto);

            decimal saldo = await _service.SaldoAnterior(
                filtro.IdPersonal.Value,
                filtro.IdMoneda,
                filtro.FechaDesde);

            var lista = new List<VMPersonalCuentaCorrienteMovimiento>();

            foreach (var m in movimientos)
            {
                saldo += m.Debe - m.Haber;

                lista.Add(new VMPersonalCuentaCorrienteMovimiento
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    TipoMov = m.TipoMov,
                    Concepto = m.Concepto,
                    Debe = m.Debe,
                    Haber = m.Haber,
                    Saldo = saldo,
                    Moneda = m.IdMonedaNavigation?.Nombre,
                    PuedeEliminar = m.TipoMov == "PAGO PERSONAL" || m.TipoMov == "AJUSTE PERSONAL"
                });
            }

            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Resumen([FromBody] VMPersonalCuentaCorrienteFiltro filtro)
        {
            var saldoAnterior = await _service.SaldoAnterior(
                filtro.IdPersonal.Value,
                filtro.IdMoneda,
                filtro.FechaDesde);

            var (debe, haber, cantidad) = await _service.Resumen(
                filtro.IdPersonal.Value,
                filtro.IdMoneda,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.TipoMov,
                filtro.Texto);

            var vm = new VMPersonalCuentaCorrienteResumen
            {
                SaldoAnterior = saldoAnterior,
                Debe = debe,
                Haber = haber,
                SaldoActual = saldoAnterior + debe - haber,
                CantidadMovimientos = cantidad
            };

            return Ok(vm);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPago([FromBody] VMPersonalCuentaCorrientePago model)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

                var resp = await _service.RegistrarPago(
                    model.IdPersonal,
                    model.IdMoneda,
                    model.IdCuenta,
                    model.Fecha,
                    model.Concepto,
                    model.Importe,
                    idUsuario);

                return Ok(new { valor = resp });
            }
            catch
            {
                return Ok(new { valor = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarAjuste([FromBody] VMPersonalCuentaCorrienteAjuste model)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

                var resp = await _service.RegistrarAjuste(
                    model.IdPersonal,
                    model.IdMoneda,
                    model.Fecha,
                    model.Concepto,
                    model.Debe,
                    model.Haber,
                    idUsuario);

                return Ok(new { valor = resp });
            }
            catch
            {
                return Ok(new { valor = false });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var resp = await _service.Eliminar(id);
                return Ok(new { valor = resp });
            }
            catch
            {
                return Ok(new { valor = false });
            }
        }
    }
}