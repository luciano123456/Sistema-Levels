using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class VentasRepository : IVentasRepository<Venta>
    {
        private readonly SistemaLevelsContext _db;

        private const string TIPO_MOV_VENTA_CLIENTE = "VENTA";
        private const string TIPO_MOV_VENTA_ARTISTA = "VENTA";
        private const string TIPO_MOV_VENTA_PERSONAL = "VENTA";
        private const string TIPO_MOV_COBRO_CLIENTE = "COBRO";
        private const string TIPO_MOV_COBRO_CAJA = "COBRO";

        public VentasRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        public async Task<bool> Insertar(
            Venta venta,
            List<VentasArtista> artistas,
            List<VentasPersonal> personal,
            List<VentasCobro> cobros)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                artistas ??= new();
                personal ??= new();
                cobros ??= new();

                _db.Ventas.Add(venta);
                await _db.SaveChangesAsync();

                await UpsertMovimientoVentaClienteAsync(venta);
                await SincronizarArtistasAsync(venta, artistas);
                await SincronizarPersonalAsync(venta, personal);
                await SincronizarCobrosAsync(venta, cobros);
                await RecalcularImportesVentaAsync(venta.Id);

                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> Actualizar(
            Venta venta,
            List<VentasArtista> artistas,
            List<VentasPersonal> personal,
            List<VentasCobro> cobros)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                artistas ??= new();
                personal ??= new();
                cobros ??= new();

                var entity = await _db.Ventas.FirstOrDefaultAsync(x => x.Id == venta.Id);
                if (entity == null) return false;

                entity.IdPresupuesto = null;
                entity.IdClienteCc = entity.IdClienteCc;

                entity.Fecha = venta.Fecha;
                entity.IdUbicacion = venta.IdUbicacion;
                entity.NombreEvento = venta.NombreEvento;
                entity.Duracion = venta.Duracion;

                entity.IdCliente = venta.IdCliente;
                entity.IdProductora = venta.IdProductora;
                entity.IdMoneda = venta.IdMoneda;
                entity.IdEstado = venta.IdEstado;

                entity.ImporteTotal = venta.ImporteTotal;
                entity.NotaInterna = venta.NotaInterna;
                entity.NotaCliente = venta.NotaCliente;

                entity.IdTipoContrato = venta.IdTipoContrato;
                entity.IdOpExclusividad = venta.IdOpExclusividad;
                entity.DiasPrevios = venta.DiasPrevios;
                entity.FechaHasta = venta.FechaHasta;

                entity.IdUsuarioModifica = venta.IdUsuarioModifica;
                entity.FechaModifica = DateTime.Now;

                await _db.SaveChangesAsync();

                await UpsertMovimientoVentaClienteAsync(entity);
                await SincronizarArtistasAsync(entity, artistas);
                await SincronizarPersonalAsync(entity, personal);
                await SincronizarCobrosAsync(entity, cobros);
                await RecalcularImportesVentaAsync(entity.Id);

                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var venta = await _db.Ventas.FirstOrDefaultAsync(x => x.Id == id);
                if (venta == null) return false;

                var artistas = await _db.VentasArtistas
                    .Where(x => x.IdVenta == id)
                    .ToListAsync();

                var personal = await _db.VentasPersonals
                    .Where(x => x.IdVenta == id)
                    .ToListAsync();

                var cobros = await _db.VentasCobros
                    .Where(x => x.IdVenta == id)
                    .ToListAsync();

                var idsCobros = cobros.Select(x => x.Id).ToList();

                if (idsCobros.Count > 0)
                {
                    var cobrosComisiones = await _db.VentasCobrosComisiones
                        .Where(x => idsCobros.Contains(x.IdVentaCobro))
                        .ToListAsync();

                    foreach (var com in cobrosComisiones)
                    {
                        if (com.IdPersonalCc.HasValue)
                        {
                            var movPersonal = await _db.PersonalCuentaCorrientes
                                .FirstOrDefaultAsync(x => x.Id == com.IdPersonalCc.Value);

                            if (movPersonal != null)
                                _db.PersonalCuentaCorrientes.Remove(movPersonal);
                        }
                    }

                    _db.VentasCobrosComisiones.RemoveRange(cobrosComisiones);
                    await _db.SaveChangesAsync();
                }

                foreach (var cobro in cobros)
                {
                    await EliminarMovimientoCobroClienteAsync(cobro);
                    await EliminarMovimientoCajaCobroAsync(cobro);
                }

                foreach (var art in artistas)
                {
                    await EliminarMovimientoArtistaAsync(art);
                }

                foreach (var per in personal)
                {
                    await EliminarMovimientoPersonalAsync(per);
                }

                await EliminarMovimientoVentaClienteAsync(venta);

                _db.VentasCobros.RemoveRange(cobros);
                _db.VentasArtistas.RemoveRange(artistas);
                _db.VentasPersonals.RemoveRange(personal);
                _db.Ventas.Remove(venta);

                await _db.SaveChangesAsync();
                await trx.CommitAsync();

                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        public async Task<Venta?> Obtener(int id)
        {
            return await _db.Ventas
                .AsNoTracking()
                .Include(x => x.IdClienteNavigation)
                .Include(x => x.IdProductoraNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Include(x => x.IdUbicacionNavigation)
                .Include(x => x.IdTipoContratoNavigation)
                .Include(x => x.IdOpExclusividadNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)

                .Include(x => x.VentasArtista)
                    .ThenInclude(va => va.IdArtistaNavigation)
                .Include(x => x.VentasArtista)
                    .ThenInclude(va => va.IdRepresentanteNavigation)

                .Include(x => x.VentasPersonals)
                    .ThenInclude(vp => vp.IdPersonalNavigation)
                .Include(x => x.VentasPersonals)
                    .ThenInclude(vp => vp.IdCargoNavigation)
                .Include(x => x.VentasPersonals)
                    .ThenInclude(vp => vp.IdTipoComisionNavigation)

                .Include(x => x.VentasCobros)
                    .ThenInclude(c => c.IdMonedaNavigation)
                .Include(x => x.VentasCobros)
                    .ThenInclude(c => c.IdCuentaNavigation)

                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IQueryable<Venta>> ObtenerTodos()
        {
            var q = _db.Ventas
                .AsNoTracking()
                .Include(x => x.IdClienteNavigation)
                .Include(x => x.IdProductoraNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Include(x => x.IdUbicacionNavigation)
                .Include(x => x.IdTipoContratoNavigation)
                .Include(x => x.IdOpExclusividadNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation);

            return await Task.FromResult(q);
        }

        public async Task<IQueryable<Venta>> ObtenerPorCliente(int idCliente)
        {
            var q = _db.Ventas
                .AsNoTracking()
                .Where(x => x.IdCliente == idCliente)
                .Include(x => x.IdClienteNavigation)
                .Include(x => x.IdProductoraNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Include(x => x.IdUbicacionNavigation)
                .Include(x => x.IdTipoContratoNavigation)
                .Include(x => x.IdOpExclusividadNavigation);

            return await Task.FromResult(q);
        }

        public async Task<IQueryable<Venta>> ListarFiltrado(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idEstado,
            int? idArtista,
            int? idCliente)
        {
            IQueryable<Venta> query = _db.Ventas
                .AsNoTracking()
                .Include(x => x.IdClienteNavigation)
                .Include(x => x.IdProductoraNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Include(x => x.IdUbicacionNavigation);

            if (fechaDesde.HasValue)
                query = query.Where(x => x.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(x => x.Fecha <= fechaHasta.Value);

            if (idEstado.HasValue)
                query = query.Where(x => x.IdEstado == idEstado.Value);

            if (idCliente.HasValue)
                query = query.Where(x => x.IdCliente == idCliente.Value);

            if (idArtista.HasValue)
                query = query.Where(x => x.VentasArtista.Any(a => a.IdArtista == idArtista.Value));

            return await Task.FromResult(query);
        }

        private async Task SincronizarArtistasAsync(Venta venta, List<VentasArtista> artistas)
        {
            artistas ??= new();

            artistas = artistas
                .Where(x => x.IdArtista > 0 && x.IdRepresentante > 0)
                .ToList();

            var actuales = await _db.VentasArtistas
                .Where(x => x.IdVenta == venta.Id)
                .ToListAsync();

            var idsNuevos = artistas.Where(x => x.Id > 0).Select(x => x.Id).ToHashSet();

            var eliminar = actuales
                .Where(x => !idsNuevos.Contains(x.Id))
                .ToList();

            foreach (var item in eliminar)
            {
                await EliminarMovimientoArtistaAsync(item);
                _db.VentasArtistas.Remove(item);
            }

            await _db.SaveChangesAsync();

            foreach (var item in artistas)
            {
                if (item.Id <= 0)
                {
                    var nuevo = new VentasArtista
                    {
                        IdVenta = venta.Id,
                        IdArtista = item.IdArtista,
                        IdRepresentante = item.IdRepresentante,
                        PorcComision = item.PorcComision,
                        TotalComision = item.TotalComision,
                        IdArtistaCc = 0
                    };

                    _db.VentasArtistas.Add(nuevo);
                    await _db.SaveChangesAsync();

                    await UpsertMovimientoArtistaAsync(venta, nuevo);
                }
                else
                {
                    var actual = actuales.FirstOrDefault(x => x.Id == item.Id);
                    if (actual == null) continue;

                    actual.IdArtista = item.IdArtista;
                    actual.IdRepresentante = item.IdRepresentante;
                    actual.PorcComision = item.PorcComision;
                    actual.TotalComision = item.TotalComision;

                    await _db.SaveChangesAsync();
                    await UpsertMovimientoArtistaAsync(venta, actual);
                }
            }
        }

        private async Task SincronizarPersonalAsync(Venta venta, List<VentasPersonal> personal)
        {
            personal ??= new();

            personal = personal
                .Where(x => x.IdPersonal > 0 && x.IdCargo > 0 && x.IdTipoComision > 0)
                .ToList();

            var actuales = await _db.VentasPersonals
                .Where(x => x.IdVenta == venta.Id)
                .ToListAsync();

            var idsNuevos = personal.Where(x => x.Id > 0).Select(x => x.Id).ToHashSet();

            var eliminar = actuales
                .Where(x => !idsNuevos.Contains(x.Id))
                .ToList();

            foreach (var item in eliminar)
            {
                await EliminarMovimientoPersonalAsync(item);
                _db.VentasPersonals.Remove(item);
            }

            await _db.SaveChangesAsync();

            foreach (var item in personal)
            {
                if (item.Id <= 0)
                {
                    var nuevo = new VentasPersonal
                    {
                        IdVenta = venta.Id,
                        IdPersonal = item.IdPersonal,
                        IdCargo = item.IdCargo,
                        IdTipoComision = item.IdTipoComision,
                        PorcComision = item.PorcComision,
                        TotalComision = item.TotalComision,
                        IdUsuarioRegistra = item.IdUsuarioRegistra > 0
                            ? item.IdUsuarioRegistra
                            : ObtenerUsuarioActual(venta),
                        FechaRegistra = item.FechaRegistra == default
                            ? DateTime.Now
                            : item.FechaRegistra
                    };

                    _db.VentasPersonals.Add(nuevo);
                    await _db.SaveChangesAsync();

                    await UpsertMovimientoPersonalAsync(venta, nuevo);
                }
                else
                {
                    var actual = actuales.FirstOrDefault(x => x.Id == item.Id);
                    if (actual == null) continue;

                    actual.IdPersonal = item.IdPersonal;
                    actual.IdCargo = item.IdCargo;
                    actual.IdTipoComision = item.IdTipoComision;
                    actual.PorcComision = item.PorcComision;
                    actual.TotalComision = item.TotalComision;
                    actual.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                    actual.FechaModifica = DateTime.Now;

                    await _db.SaveChangesAsync();
                    await UpsertMovimientoPersonalAsync(venta, actual);
                }
            }
        }

        private async Task SincronizarCobrosAsync(Venta venta, List<VentasCobro> cobros)
        {
            cobros ??= new();

            var actuales = await _db.VentasCobros
                .Where(x => x.IdVenta == venta.Id)
                .ToListAsync();

            var idsNuevos = cobros
                .Where(x => x.Id > 0)
                .Select(x => x.Id)
                .ToHashSet();

            var eliminar = actuales
                .Where(x => !idsNuevos.Contains(x.Id))
                .ToList();

            /* =========================================
               ELIMINAR COBROS QUE YA NO EXISTEN
            ========================================= */
            foreach (var item in eliminar)
            {
                var comisionesCobro = await _db.VentasCobrosComisiones
                    .Where(x => x.IdVentaCobro == item.Id)
                    .ToListAsync();

                foreach (var com in comisionesCobro)
                {
                    if (com.IdPersonalCc.HasValue)
                    {
                        var movPersonal = await _db.PersonalCuentaCorrientes
                            .FirstOrDefaultAsync(x => x.Id == com.IdPersonalCc.Value);

                        if (movPersonal != null)
                            _db.PersonalCuentaCorrientes.Remove(movPersonal);
                    }
                }

                if (comisionesCobro.Any())
                    _db.VentasCobrosComisiones.RemoveRange(comisionesCobro);

                await EliminarMovimientoCobroClienteAsync(item);
                await EliminarMovimientoCajaCobroAsync(item);

                _db.VentasCobros.Remove(item);
            }

            await _db.SaveChangesAsync();

            /* =========================================
               INSERTAR / ACTUALIZAR COBROS
            ========================================= */
            foreach (var item in cobros)
            {
                /* ===============================
                   NUEVO COBRO
                =============================== */
                if (item.Id <= 0)
                {
                    var nuevo = new VentasCobro
                    {
                        IdVenta = venta.Id,
                        IdClienteCc = null,
                        IdArtistaCc = null,
                        IdCaja = null,

                        Fecha = item.Fecha == default ? DateTime.Now : item.Fecha,
                        IdMoneda = item.IdMoneda,
                        IdCuenta = item.IdCuenta,

                        Importe = item.Importe,
                        Cotizacion = item.Cotizacion <= 0 ? 1 : item.Cotizacion,

                        Conversion = item.Conversion > 0
                            ? item.Conversion
                            : item.Importe * (item.Cotizacion <= 0 ? 1 : item.Cotizacion),

                        NotaCliente = item.NotaCliente,
                        NotaInterna = item.NotaInterna,

                        IdUsuarioRegistra = item.IdUsuarioRegistra > 0
                            ? item.IdUsuarioRegistra
                            : ObtenerUsuarioActual(venta),

                        FechaRegistra = item.FechaRegistra == default
                            ? DateTime.Now
                            : item.FechaRegistra
                    };

                    _db.VentasCobros.Add(nuevo);
                    await _db.SaveChangesAsync();

                    /* Generar comisiones del personal */
                    await GenerarComisionesCobroAsync(venta, nuevo);

                    /* Movimiento cliente */
                    await UpsertMovimientoCobroClienteAsync(venta, nuevo);

                    /* Movimiento caja */
                    await UpsertMovimientoCajaCobroAsync(venta, nuevo);
                }

                /* ===============================
                   ACTUALIZAR COBRO EXISTENTE
                =============================== */
                else
                {
                    var actual = actuales.FirstOrDefault(x => x.Id == item.Id);
                    if (actual == null)
                        continue;

                    actual.Fecha = item.Fecha == default ? actual.Fecha : item.Fecha;
                    actual.IdMoneda = item.IdMoneda;
                    actual.IdCuenta = item.IdCuenta;
                    actual.Importe = item.Importe;
                    actual.Cotizacion = item.Cotizacion <= 0 ? 1 : item.Cotizacion;

                    actual.Conversion = item.Conversion > 0
                        ? item.Conversion
                        : item.Importe * (item.Cotizacion <= 0 ? 1 : item.Cotizacion);

                    actual.NotaCliente = item.NotaCliente;
                    actual.NotaInterna = item.NotaInterna;

                    actual.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                    actual.FechaModifica = DateTime.Now;

                    await _db.SaveChangesAsync();

                    /* ===============================
                       BORRAR COMISIONES ANTERIORES
                    =============================== */

                    var viejas = await _db.VentasCobrosComisiones
                        .Where(x => x.IdVentaCobro == actual.Id)
                        .ToListAsync();

                    foreach (var com in viejas)
                    {
                        if (com.IdPersonalCc.HasValue)
                        {
                            var mov = await _db.PersonalCuentaCorrientes
                                .FirstOrDefaultAsync(x => x.Id == com.IdPersonalCc.Value);

                            if (mov != null)
                                _db.PersonalCuentaCorrientes.Remove(mov);
                        }
                    }

                    if (viejas.Any())
                        _db.VentasCobrosComisiones.RemoveRange(viejas);

                    await _db.SaveChangesAsync();

                    /* ===============================
                       REGENERAR COMISIONES
                    =============================== */
                    await GenerarComisionesCobroAsync(venta, actual);

                    /* ===============================
                       ACTUALIZAR MOVIMIENTOS
                    =============================== */
                    await UpsertMovimientoCobroClienteAsync(venta, actual);
                    await UpsertMovimientoCajaCobroAsync(venta, actual);
                }
            }
        }
        private async Task UpsertMovimientoVentaClienteAsync(Venta venta)
        {
            ClientesCuentaCorriente? mov = null;

            if (venta.IdClienteCc.HasValue && venta.IdClienteCc.Value > 0)
            {
                mov = await _db.ClientesCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == venta.IdClienteCc.Value);
            }

            mov ??= await _db.ClientesCuentaCorrientes
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_VENTA_CLIENTE &&
                    x.IdMov == venta.Id);

            if (mov == null)
            {
                mov = new ClientesCuentaCorriente
                {
                    IdCliente = venta.IdCliente,
                    IdMoneda = venta.IdMoneda,
                    TipoMov = TIPO_MOV_VENTA_CLIENTE,
                    IdMov = venta.Id,
                    Fecha = venta.Fecha,
                    Concepto = $"Venta {venta.NombreEvento}",
                    Debe = venta.ImporteTotal,
                    Haber = 0,
                    IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                    FechaRegistra = DateTime.Now
                };

                _db.ClientesCuentaCorrientes.Add(mov);
                await _db.SaveChangesAsync();

                venta.IdClienteCc = mov.Id;
                await _db.SaveChangesAsync();
            }
            else
            {
                mov.IdCliente = venta.IdCliente;
                mov.IdMoneda = venta.IdMoneda;
                mov.Fecha = venta.Fecha;
                mov.Concepto = $"Venta {venta.NombreEvento}";
                mov.Debe = venta.ImporteTotal;
                mov.Haber = 0;
                mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                mov.FechaModifica = DateTime.Now;

                if (venta.IdClienteCc != mov.Id)
                {
                    venta.IdClienteCc = mov.Id;
                    await _db.SaveChangesAsync();
                }
            }
        }

        private async Task EliminarMovimientoVentaClienteAsync(Venta venta)
        {
            ClientesCuentaCorriente? mov = null;

            if (venta.IdClienteCc.HasValue && venta.IdClienteCc.Value > 0)
            {
                mov = await _db.ClientesCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == venta.IdClienteCc.Value);
            }

            mov ??= await _db.ClientesCuentaCorrientes
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_VENTA_CLIENTE &&
                    x.IdMov == venta.Id);

            if (mov != null)
            {
                _db.ClientesCuentaCorrientes.Remove(mov);
                await _db.SaveChangesAsync();
            }
        }

        private async Task UpsertMovimientoArtistaAsync(Venta venta, VentasArtista artista)
        {
            ArtistasCuentaCorriente? mov = null;

            if (artista.IdArtistaCc > 0)
            {
                mov = await _db.ArtistasCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == artista.IdArtistaCc);
            }

            mov ??= await _db.ArtistasCuentaCorrientes
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_VENTA_ARTISTA &&
                    x.IdMov == artista.Id);

            if (mov == null)
            {
                mov = new ArtistasCuentaCorriente
                {
                    IdArtista = artista.IdArtista,
                    IdMoneda = venta.IdMoneda,
                    TipoMov = TIPO_MOV_VENTA_ARTISTA,
                    IdMov = artista.Id,
                    Fecha = venta.Fecha,
                    Concepto = $"Comisión artista venta {venta.NombreEvento}",
                    Debe = artista.TotalComision,
                    Haber = 0,
                    IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                    FechaRegistra = DateTime.Now
                };

                _db.ArtistasCuentaCorrientes.Add(mov);
                await _db.SaveChangesAsync();

                artista.IdArtistaCc = mov.Id;
                await _db.SaveChangesAsync();
            }
            else
            {
                mov.IdArtista = artista.IdArtista;
                mov.IdMoneda = venta.IdMoneda;
                mov.Fecha = venta.Fecha;
                mov.Concepto = $"Comisión artista venta {venta.NombreEvento}";
                mov.Debe = artista.TotalComision;
                mov.Haber = 0;
                mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                mov.FechaModifica = DateTime.Now;

                if (artista.IdArtistaCc != mov.Id)
                {
                    artista.IdArtistaCc = mov.Id;
                    await _db.SaveChangesAsync();
                }
            }
        }

        private async Task EliminarMovimientoArtistaAsync(VentasArtista artista)
        {
            ArtistasCuentaCorriente? mov = null;

            if (artista.IdArtistaCc > 0)
            {
                mov = await _db.ArtistasCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == artista.IdArtistaCc);
            }

            mov ??= await _db.ArtistasCuentaCorrientes
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_VENTA_ARTISTA &&
                    x.IdMov == artista.Id);

            if (mov != null)
            {
                _db.ArtistasCuentaCorrientes.Remove(mov);
                await _db.SaveChangesAsync();
            }
        }

        private async Task UpsertMovimientoPersonalAsync(Venta venta, VentasPersonal personal)
        {
            var mov = await _db.PersonalCuentaCorrientes
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_VENTA_PERSONAL &&
                    x.IdMov == personal.Id);

            if (mov == null)
            {
                mov = new PersonalCuentaCorriente
                {
                    IdPersonal = personal.IdPersonal,
                    IdMoneda = venta.IdMoneda,
                    TipoMov = TIPO_MOV_VENTA_PERSONAL,
                    IdMov = personal.Id,
                    Fecha = venta.Fecha,
                    Concepto = $"Comisión personal venta {venta.NombreEvento}",
                    Debe = personal.TotalComision,
                    Haber = 0,
                    IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                    FechaRegistra = DateTime.Now
                };

                _db.PersonalCuentaCorrientes.Add(mov);
                await _db.SaveChangesAsync();
            }
            else
            {
                mov.IdPersonal = personal.IdPersonal;
                mov.IdMoneda = venta.IdMoneda;
                mov.Fecha = venta.Fecha;
                mov.Concepto = $"Comisión personal venta {venta.NombreEvento}";
                mov.Debe = personal.TotalComision;
                mov.Haber = 0;
                mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                mov.FechaModifica = DateTime.Now;

                await _db.SaveChangesAsync();
            }
        }

        private async Task EliminarMovimientoPersonalAsync(VentasPersonal personal)
        {
            var mov = await _db.PersonalCuentaCorrientes
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_VENTA_PERSONAL &&
                    x.IdMov == personal.Id);

            if (mov != null)
            {
                _db.PersonalCuentaCorrientes.Remove(mov);
                await _db.SaveChangesAsync();
            }
        }

        private async Task UpsertMovimientoCobroClienteAsync(Venta venta, VentasCobro cobro)
        {
            ClientesCuentaCorriente? mov = null;

            if (cobro.IdClienteCc.HasValue && cobro.IdClienteCc.Value > 0)
            {
                mov = await _db.ClientesCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == cobro.IdClienteCc.Value);
            }

            mov ??= await _db.ClientesCuentaCorrientes
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_COBRO_CLIENTE &&
                    x.IdMov == cobro.Id);

            if (mov == null)
            {
                mov = new ClientesCuentaCorriente
                {
                    IdCliente = venta.IdCliente,
                    IdMoneda = cobro.IdMoneda,
                    TipoMov = TIPO_MOV_COBRO_CLIENTE,
                    IdMov = cobro.Id,
                    Fecha = cobro.Fecha,
                    Concepto = $"Cobro venta {venta.NombreEvento}",
                    Debe = 0,
                    Haber = cobro.Conversion,
                    IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                    FechaRegistra = DateTime.Now
                };

                _db.ClientesCuentaCorrientes.Add(mov);
                await _db.SaveChangesAsync();

                cobro.IdClienteCc = mov.Id;
                await _db.SaveChangesAsync();
            }
            else
            {
                mov.IdCliente = venta.IdCliente;
                mov.IdMoneda = cobro.IdMoneda;
                mov.Fecha = cobro.Fecha;
                mov.Concepto = $"Cobro venta {venta.NombreEvento}";
                mov.Debe = 0;
                mov.Haber = cobro.Conversion;
                mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                mov.FechaModifica = DateTime.Now;

                if (cobro.IdClienteCc != mov.Id)
                {
                    cobro.IdClienteCc = mov.Id;
                    await _db.SaveChangesAsync();
                }
            }
        }

        private async Task EliminarMovimientoCobroClienteAsync(VentasCobro cobro)
        {
            ClientesCuentaCorriente? mov = null;

            if (cobro.IdClienteCc.HasValue && cobro.IdClienteCc.Value > 0)
            {
                mov = await _db.ClientesCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == cobro.IdClienteCc.Value);
            }

            mov ??= await _db.ClientesCuentaCorrientes
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_COBRO_CLIENTE &&
                    x.IdMov == cobro.Id);

            if (mov != null)
            {
                _db.ClientesCuentaCorrientes.Remove(mov);
                await _db.SaveChangesAsync();
            }
        }

        private async Task UpsertMovimientoCajaCobroAsync(Venta venta, VentasCobro cobro)
        {
            Caja? mov = null;

            if (cobro.IdCaja.HasValue && cobro.IdCaja.Value > 0)
            {
                mov = await _db.Cajas
                    .FirstOrDefaultAsync(x => x.Id == cobro.IdCaja.Value);
            }

            mov ??= await _db.Cajas
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_COBRO_CAJA &&
                    x.IdMov == cobro.Id);

            if (mov == null)
            {
                mov = new Caja
                {
                    TipoMov = TIPO_MOV_COBRO_CAJA,
                    IdMov = cobro.Id,
                    Fecha = cobro.Fecha,
                    Concepto = $"Cobro venta {venta.NombreEvento}",
                    IdMoneda = cobro.IdMoneda,
                    IdCuenta = cobro.IdCuenta,
                    Ingreso = cobro.Conversion,
                    Egrreso = 0,
                    Saldo = 0,
                    IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                    FechaRegistra = DateTime.Now
                };

                _db.Cajas.Add(mov);
                await _db.SaveChangesAsync();

                cobro.IdCaja = mov.Id;
                await _db.SaveChangesAsync();
            }
            else
            {
                mov.Fecha = cobro.Fecha;
                mov.Concepto = $"Cobro venta {venta.NombreEvento}";
                mov.IdMoneda = cobro.IdMoneda;
                mov.IdCuenta = cobro.IdCuenta;
                mov.Ingreso = cobro.Conversion;
                mov.Egrreso = 0;
                mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                mov.FechaModifica = DateTime.Now;

                if (cobro.IdCaja != mov.Id)
                {
                    cobro.IdCaja = mov.Id;
                    await _db.SaveChangesAsync();
                }
            }
        }

        private async Task EliminarMovimientoCajaCobroAsync(VentasCobro cobro)
        {
            Caja? mov = null;

            if (cobro.IdCaja.HasValue && cobro.IdCaja.Value > 0)
            {
                mov = await _db.Cajas
                    .FirstOrDefaultAsync(x => x.Id == cobro.IdCaja.Value);
            }

            mov ??= await _db.Cajas
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == TIPO_MOV_COBRO_CAJA &&
                    x.IdMov == cobro.Id);

            if (mov != null)
            {
                _db.Cajas.Remove(mov);
                await _db.SaveChangesAsync();
            }
        }

        private async Task RecalcularImportesVentaAsync(int idVenta)
        {
            var venta = await _db.Ventas.FirstOrDefaultAsync(x => x.Id == idVenta);
            if (venta == null) return;

            var totalCobrado = await _db.VentasCobros
                .Where(x => x.IdVenta == idVenta)
                .SumAsync(x => (decimal?)x.Conversion) ?? 0m;

            venta.ImporteAbonado = totalCobrado;
            venta.Saldo = venta.ImporteTotal - totalCobrado;

            await _db.SaveChangesAsync();

            await UpsertMovimientoVentaClienteAsync(venta);
        }

        private int ObtenerUsuarioActual(Venta venta)
        {
            if (venta.IdUsuarioModifica.HasValue && venta.IdUsuarioModifica.Value > 0)
                return venta.IdUsuarioModifica.Value;

            return venta.IdUsuarioRegistra;
        }


        private async Task EliminarMovimientosComisionCobroAsync(int idVentaCobro)
        {
            var comisiones = await _db.VentasCobrosComisiones
                .Where(x => x.IdVentaCobro == idVentaCobro)
                .ToListAsync();

            foreach (var com in comisiones)
            {
                if (com.IdPersonalCc.HasValue)
                {
                    var mov = await _db.PersonalCuentaCorrientes
                        .FirstOrDefaultAsync(x => x.Id == com.IdPersonalCc.Value);

                    if (mov != null)
                        _db.PersonalCuentaCorrientes.Remove(mov);
                }
            }

            if (comisiones.Any())
            {
                _db.VentasCobrosComisiones.RemoveRange(comisiones);
                await _db.SaveChangesAsync();
            }
        }

        private async Task GenerarComisionesCobroAsync(Venta venta, VentasCobro cobro)
        {
            var personal = await _db.VentasPersonals
                .Where(x => x.IdVenta == venta.Id)
                .ToListAsync();

            if (!personal.Any())
                return;

            var lista = new List<VentasCobrosComision>();

            foreach (var p in personal)
            {
                if (p.PorcComision <= 0)
                    continue;

                var total = Math.Round(
                    cobro.Conversion * (p.PorcComision / 100m),
                    2
                );

                if (total <= 0)
                    continue;

                var comision = new VentasCobrosComision
                {
                    IdVentaCobro = cobro.Id,
                    IdPersonal = p.IdPersonal,
                    TotalComision = total,
                    IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                    FechaRegistra = DateTime.Now
                };

                _db.VentasCobrosComisiones.Add(comision);
                await _db.SaveChangesAsync();

                var mov = new PersonalCuentaCorriente
                {
                    IdPersonal = p.IdPersonal,
                    IdMoneda = venta.IdMoneda,
                    TipoMov = "COMISION_COBRO",
                    IdMov = cobro.Id,
                    Fecha = cobro.Fecha,
                    Concepto = $"Comisión cobro venta {venta.NombreEvento}",
                    Debe = total,
                    Haber = 0,
                    IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                    FechaRegistra = DateTime.Now
                };

                _db.PersonalCuentaCorrientes.Add(mov);
                await _db.SaveChangesAsync();

                comision.IdPersonalCc = mov.Id;

                await _db.SaveChangesAsync();
            }
        }
    }
}