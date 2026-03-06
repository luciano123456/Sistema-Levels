using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;

namespace SistemaLevels.Controllers
{
    public class ArtistasCuentaCorrienteController : Controller
    {
        private readonly IArtistasCuentaCorrienteService _service;

        public ArtistasCuentaCorrienteController(IArtistasCuentaCorrienteService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ListaArtistas(string? buscar)
        {
            var data = await _service.ListarArtistas(buscar);

            var lista = data.Select(x => new VMArtistasCuentaCorrienteArtista
            {
                Id = x.artista.Id,
                Nombre = x.artista.NombreArtistico,

                Saldo = x.saldo
            });

            return Ok(lista);
        }


        [HttpGet]
        public async Task<IActionResult> Movimiento(int id)
        {
            var (mov, cuenta) = await _service.ObtenerMovimiento(id);

            if (mov == null)
                return NotFound();

            var vm = new
            {
                mov.Id,
                mov.Fecha,
                mov.TipoMov,
                mov.Concepto,
                mov.Debe,
                mov.Haber,
                Moneda = mov.IdMonedaNavigation?.Nombre,
                Cuenta = cuenta
            };

            return Ok(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Movimientos([FromBody] VMArtistasCuentaCorrienteFiltro filtro)
        {
            var movimientos = await _service.Movimientos(
                filtro.IdArtista.Value,
                filtro.IdMoneda,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.TipoMov,
                filtro.Texto);

            decimal saldo = await _service.SaldoAnterior(
                filtro.IdArtista.Value,
                filtro.IdMoneda,
                filtro.FechaDesde);

            var lista = new List<VMArtistasCuentaCorrienteMovimiento>();

            foreach (var m in movimientos)
            {
                saldo += m.Debe - m.Haber;

                lista.Add(new VMArtistasCuentaCorrienteMovimiento
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    TipoMov = m.TipoMov,
                    Concepto = m.Concepto,
                    Debe = m.Debe,
                    Haber = m.Haber,
                    Saldo = saldo,
                    Moneda = m.IdMonedaNavigation?.Nombre,
                    PuedeEliminar = m.TipoMov == "PAGO ARTISTA" || m.TipoMov == "AJUSTE ARTISTA"
                });
            }

            return Ok(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Resumen([FromBody] VMArtistasCuentaCorrienteFiltro filtro)
        {
            var saldoAnterior = await _service.SaldoAnterior(
                filtro.IdArtista.Value,
                filtro.IdMoneda,
                filtro.FechaDesde);

            var (debe, haber, cantidad) = await _service.Resumen(
                filtro.IdArtista.Value,
                filtro.IdMoneda,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.TipoMov,
                filtro.Texto);

            var vm = new VMArtistasCuentaCorrienteResumen
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
        public async Task<IActionResult> RegistrarPago([FromBody] VMArtistasCuentaCorrientePago model)
        {
            try { 
            int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

            var resp = await _service.RegistrarPago(
                model.IdArtista,
                model.IdMoneda,
                model.IdCuenta,
                model.Fecha,
                model.Concepto,
                model.Importe,
                idUsuario);

            return Ok(new { valor = resp });
            }
            catch (Exception ex)
            {
                return Ok(new { valor = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarAjuste([FromBody] VMArtistasCuentaCorrienteAjuste model)
        {
            try
            {
                int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

                var resp = await _service.RegistrarAjuste(
                    model.IdArtista,
                    model.IdMoneda,
                    model.Fecha,
                    model.Concepto,
                    model.Debe,
                    model.Haber,
                    idUsuario);

                return Ok(new { valor = resp });
            } catch  (Exception ex)
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
            } catch (Exception ex)
            {
                return Ok(new { valor = false });
            }
        }
    }
}