using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;

namespace SistemaLevels.Controllers
{
    public class CajaController : Controller
    {
        private readonly ICajaService _service;

        public CajaController(ICajaService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Movimientos([FromBody] VMCajaFiltro filtro)
        {
            var movimientos = await _service.Movimientos(
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.IdMoneda,
                filtro.IdCuenta,
                filtro.TipoMov,
                filtro.Texto);

            decimal saldo = await _service.SaldoAnterior(
                filtro.FechaDesde,
                filtro.IdMoneda,
                filtro.IdCuenta);

            var lista = new List<VMCajaMovimiento>();

            // 👇 FILA SALDO ANTERIOR
            lista.Add(new VMCajaMovimiento
            {
                Id = 0,
                Fecha = filtro.FechaDesde ?? DateTime.Today,
                TipoMov = "SALDO_ANTERIOR",
                Concepto = "Saldo anterior",
                Ingreso = 0,
                Egreso = 0,
                Saldo = saldo,
                PuedeEditar = false,
                PuedeEliminar = false,
                Origen = "",
                Moneda = "",
                Cuenta = ""
            });

            foreach (var m in movimientos)
            {
                saldo += m.Ingreso - m.Egreso;

                bool puedeEditar =
                    m.TipoMov == "INGRESO MANUAL" ||
                    m.TipoMov == "EGRESO MANUAL" ||
                    m.TipoMov == "TRANSFERENCIA";

                bool puedeEliminar = puedeEditar;

                string origen = m.TipoMov switch
                {
                    "INGRESO MANUAL" => "MANUAL",
                    "EGRESO MANUAL" => "MANUAL",
                    "TRANSFERENCIA" => "TRANSFERENCIA",
                    "COBRO" => "VENTAS",
                    "PAGO ARTISTA" => "CUENTA CORRIENTE ARTISTAS",
                    "PAGO PERSONAL" => "CUENTA CORRIENTE PERSONAL",
                    _ => "SISTEMA"
                };

                lista.Add(new VMCajaMovimiento
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    TipoMov = m.TipoMov,
                    IdMov = m.IdMov,
                    Concepto = m.Concepto,
                    IdMoneda = m.IdMoneda,
                    Moneda = m.IdMonedaNavigation?.Nombre,
                    IdCuenta = m.IdCuenta,
                    Cuenta = m.IdCuentaNavigation?.Nombre,
                    Ingreso = m.Ingreso,
                    Egreso = m.Egreso,
                    Saldo = saldo,
                    PuedeEditar = puedeEditar,
                    PuedeEliminar = puedeEliminar,
                    Origen = origen
                });
            }

            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Resumen([FromBody] VMCajaFiltro filtro)
        {
            var saldoAnterior = await _service.SaldoAnterior(
                filtro.FechaDesde,
                filtro.IdMoneda,
                filtro.IdCuenta);

            var (ingresos, egresos, cantidad) = await _service.Resumen(
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.IdMoneda,
                filtro.IdCuenta,
                filtro.TipoMov,
                filtro.Texto);

            return Ok(new VMCajaResumen
            {
                SaldoAnterior = saldoAnterior,
                Ingresos = ingresos,
                Egresos = egresos,
                SaldoActual = saldoAnterior + ingresos - egresos,
                CantidadMovimientos = cantidad
            });
        }

        [HttpGet]
        public async Task<IActionResult> Movimiento(int id)
        {
            var (mov, saldo, origen, puedeEditar, puedeEliminar, tipoTransferencia) = await _service.ObtenerMovimiento(id);

            if (mov == null)
                return NotFound();

            return Ok(new VMCajaDetalleMovimiento
            {
                Id = mov.Id,
                Fecha = mov.Fecha,
                TipoMov = mov.TipoMov,
                IdMov = mov.IdMov,
                Concepto = mov.Concepto,
                IdMoneda = mov.IdMoneda,
                Moneda = mov.IdMonedaNavigation?.Nombre,
                IdCuenta = mov.IdCuenta,
                Cuenta = mov.IdCuentaNavigation?.Nombre,
                Ingreso = mov.Ingreso,
                Egreso = mov.Egreso,
                Saldo = saldo,
                PuedeEditar = puedeEditar,
                PuedeEliminar = puedeEliminar,
                Origen = origen,
                TipoTransferencia = tipoTransferencia
            });
        }

        [HttpGet]
        public async Task<IActionResult> Transferencia(int id)
        {
            var tr = await _service.ObtenerTransferencia(id);
            if (tr == null)
                return NotFound();

            return Ok(new VMCajaDetalleTransferencia
            {
                Id = tr.Id,
                Fecha = DateTime.Now,
                IdMonedaOrigen = tr.IdMonedaOrigen,
                MonedaOrigen = tr.IdMonedaOrigenNavigation?.Nombre,
                IdCuentaOrigen = tr.IdCuentaOrigen,
                CuentaOrigen = tr.IdCuentaOrigenNavigation?.Nombre,
                ImporteOrigen = tr.ImporteOrigen,
                IdMonedaDestino = tr.IdMonedaDestino,
                MonedaDestino = tr.IdMonedaDestinoNavigation?.Nombre,
                IdCuentaDestino = tr.IdCuentaDestino,
                CuentaDestino = tr.IdCuentaDestinoNavigation?.Nombre,
                ImporteDestino = tr.ImporteDestino,
                Cotizacion = tr.Cotizacion,
                NotaInterna = tr.NotaInterna,
                PuedeEditar = true,
                PuedeEliminar = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarIngreso([FromBody] VMCajaMovimientoManual model)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

                var resp = await _service.RegistrarIngresoManual(
                    DateTime.Now,
                    model.IdMoneda,
                    model.IdCuenta,
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
        public async Task<IActionResult> RegistrarEgreso([FromBody] VMCajaMovimientoManual model)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

                var resp = await _service.RegistrarEgresoManual(
                     DateTime.Now,
                    model.IdMoneda,
                    model.IdCuenta,
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
        public async Task<IActionResult> ActualizarMovimientoManual([FromBody] VMCajaMovimientoManual model)
        {
            try
            {
                if (!model.Id.HasValue)
                    return Ok(new { valor = false });

                int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

                var resp = await _service.ActualizarMovimientoManual(
                    model.Id.Value,
                     DateTime.Now,
                    model.IdMoneda,
                    model.IdCuenta,
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
        public async Task<IActionResult> RegistrarTransferencia([FromBody] VMCajaTransferencia model)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

                var resp = await _service.RegistrarTransferencia(
                     DateTime.Now,
                    model.IdMonedaOrigen,
                    model.IdCuentaOrigen,
                    model.ImporteOrigen,
                    model.IdMonedaDestino,
                    model.IdCuentaDestino,
                    model.ImporteDestino,
                    model.Cotizacion,
                    model.NotaInterna,
                    idUsuario);

                return Ok(new { valor = resp });
            }
            catch
            {
                return Ok(new { valor = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarTransferencia([FromBody] VMCajaTransferencia model)
        {
            try
            {
                if (!model.Id.HasValue)
                    return Ok(new { valor = false });

                int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

                var resp = await _service.ActualizarTransferencia(
                    model.Id.Value,
                    model.Fecha,
                    model.IdMonedaOrigen,
                    model.IdCuentaOrigen,
                    model.ImporteOrigen,
                    model.IdMonedaDestino,
                    model.IdCuentaDestino,
                    model.ImporteDestino,
                    model.Cotizacion,
                    model.NotaInterna,
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
                int idUsuario = int.Parse(User.FindFirst("Id")!.Value);
                var resp = await _service.Eliminar(id, idUsuario);
                return Ok(new { valor = resp });
            }
            catch
            {
                return Ok(new { valor = false });
            }
        }
    }
}