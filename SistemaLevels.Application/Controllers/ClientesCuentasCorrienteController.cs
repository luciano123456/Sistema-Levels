using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;

namespace SistemaLevels.Controllers
{
    public class ClientesCuentaCorrienteController : Controller
    {
        private readonly IClientesCuentaCorrienteService _service;

        public ClientesCuentaCorrienteController(IClientesCuentaCorrienteService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ListaClientes(string? buscar)
        {
            var data = await _service.ListarClientes(buscar);

            var lista = data.Select(x => new VMClientesCuentaCorrienteCliente
            {
                Id = x.cliente.Id,
                Nombre = x.cliente.Nombre,
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
                mov.IdCliente,
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
        public async Task<IActionResult> Movimientos([FromBody] VMClientesCuentaCorrienteFiltro filtro)
        {
            var movimientos = await _service.Movimientos(
                filtro.IdCliente.Value,
                filtro.IdMoneda,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.TipoMov,
                filtro.Texto);

            decimal saldo = await _service.SaldoAnterior(
                filtro.IdCliente.Value,
                filtro.IdMoneda,
                filtro.FechaDesde);

            var lista = new List<VMClientesCuentaCorrienteMovimiento>();

            foreach (var m in movimientos)
            {
                saldo += m.Debe - m.Haber;

                lista.Add(new VMClientesCuentaCorrienteMovimiento
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    TipoMov = m.TipoMov,
                    Concepto = m.Concepto,
                    Debe = m.Debe,
                    Haber = m.Haber,
                    Saldo = saldo,
                    Moneda = m.IdMonedaNavigation?.Nombre,
                    PuedeEliminar = m.TipoMov == "COBRO CLIENTE" || m.TipoMov == "AJUSTE CLIENTE"
                });
            }

            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Resumen([FromBody] VMClientesCuentaCorrienteFiltro filtro)
        {
            var saldoAnterior = await _service.SaldoAnterior(
                filtro.IdCliente.Value,
                filtro.IdMoneda,
                filtro.FechaDesde);

            var (debe, haber, cantidad) = await _service.Resumen(
                filtro.IdCliente.Value,
                filtro.IdMoneda,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.TipoMov,
                filtro.Texto);

            var vm = new VMClientesCuentaCorrienteResumen
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
        public async Task<IActionResult> RegistrarCobro([FromBody] VMClientesCuentaCorrienteCobro model)
        {
            int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

            var resp = await _service.RegistrarCobro(
                model.IdCliente,
                model.IdMoneda,
                model.IdCuenta,
                model.Fecha,
                model.Concepto,
                model.Importe,
                idUsuario);

            return Ok(new { valor = resp });
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarAjuste([FromBody] VMClientesCuentaCorrienteAjuste model)
        {
            int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

            var resp = await _service.RegistrarAjuste(
                model.IdCliente,
                model.IdMoneda,
                model.Fecha,
                model.Concepto,
                model.Debe,
                model.Haber,
                idUsuario);

            return Ok(new { valor = resp });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resp = await _service.Eliminar(id);
            return Ok(new { valor = resp });
        }
    }
}