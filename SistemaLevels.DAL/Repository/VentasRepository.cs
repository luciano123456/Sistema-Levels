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
            await using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                artistas ??= new();
                personal ??= new();
                cobros ??= new();

                NormalizarVenta(venta);

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
            await using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                artistas ??= new();
                personal ??= new();

                var entity = await _db.Ventas.FirstOrDefaultAsync(x => x.Id == venta.Id);
                if (entity == null)
                    return false;

                entity.IdPresupuesto = venta.IdPresupuesto;
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

                NormalizarVenta(entity);

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
            await using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var venta = await _db.Ventas.FirstOrDefaultAsync(x => x.Id == id);
                if (venta == null)
                    return false;

                var artistas = await _db.VentasArtistas
                    .Where(x => x.IdVenta == id)
                    .ToListAsync();

                var personal = await _db.VentasPersonals
                    .Where(x => x.IdVenta == id)
                    .ToListAsync();

                var cobros = await _db.Set<VentasCobro>()
                    .Where(x => x.IdVenta == id)
                    .ToListAsync();

                var idsCobros = cobros.Select(x => x.Id).ToList();

                /*
                =====================================
                ELIMINAR COMISIONES COBROS PERSONAL
                =====================================
                */

                if (idsCobros.Count > 0)
                {
                    await EliminarMovimientosComisionCobroPorListaAsync(idsCobros);
                }

                /*
                =====================================
                COBROS → CAJA + CC CLIENTE + CC ARTISTA
                =====================================
                */

                foreach (var cobro in cobros)
                {
                    await EliminarMovimientoCobroClienteAsync(cobro);
                    await EliminarMovimientoCajaCobroAsync(cobro);
                    await EliminarMovimientoArtistaCobroAsync(cobro);
                }

                /*
                =====================================
                ARTISTAS CC
                =====================================
                */

                foreach (var art in artistas)
                {
                    await EliminarMovimientoArtistaAsync(art);
                }

                /*
                =====================================
                PERSONAL CC
                =====================================
                */

                foreach (var per in personal)
                {
                    await EliminarMovimientoPersonalAsync(per);
                }

                /*
                =====================================
                CLIENTE CC (VENTA)
                =====================================
                */

                await EliminarMovimientoVentaClienteAsync(venta);

                /*
                =====================================
                BORRAR TABLAS
                =====================================
                */

                if (cobros.Count > 0)
                    _db.Set<VentasCobro>().RemoveRange(cobros);

                if (artistas.Count > 0)
                    _db.VentasArtistas.RemoveRange(artistas);

                if (personal.Count > 0)
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
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la venta {id}.", ex);
            }
        }

        public async Task<IQueryable<Venta>> ObtenerTodos()
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception("Error al obtener todas las ventas.", ex);
            }
        }

        public async Task<IQueryable<Venta>> ObtenerPorCliente(int idCliente)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ventas del cliente {idCliente}.", ex);
            }
        }

        public async Task<IQueryable<Venta>> ListarFiltrado(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idEstado,
            int? idArtista,
            int? idCliente)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception("Error al listar ventas filtradas.", ex);
            }
        }

        private async Task SincronizarArtistasAsync(Venta venta, List<VentasArtista>? artistas)
        {
            try
            {
                artistas ??= new();

                var incoming = artistas
                    .Where(x => x.IdArtista > 0 && x.IdRepresentante > 0)
                    .ToList();

                var actuales = await _db.VentasArtistas
                    .Where(x => x.IdVenta == venta.Id)
                    .ToListAsync();

                var actualesDict = actuales.ToDictionary(x => x.Id, x => x);
                var idsIncomingExistentes = incoming
                    .Where(x => x.Id > 0)
                    .Select(x => x.Id)
                    .ToHashSet();

                var eliminar = actuales
                    .Where(x => !idsIncomingExistentes.Contains(x.Id))
                    .ToList();

                foreach (var item in eliminar)
                {
                    await EliminarMovimientoArtistaAsync(item);
                }

                if (eliminar.Count > 0)
                {
                    _db.VentasArtistas.RemoveRange(eliminar);
                    await _db.SaveChangesAsync();
                }

                var nuevos = new List<VentasArtista>();
                var modificados = new List<VentasArtista>();

                foreach (var item in incoming)
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
                            IdArtistaCc = null
                        };

                        nuevos.Add(nuevo);
                    }
                    else if (actualesDict.TryGetValue(item.Id, out var entity))
                    {
                        var cambio =
                            entity.IdArtista != item.IdArtista ||
                            entity.IdRepresentante != item.IdRepresentante ||
                            entity.PorcComision != item.PorcComision ||
                            entity.TotalComision != item.TotalComision;

                        if (cambio)
                        {
                            entity.IdArtista = item.IdArtista;
                            entity.IdRepresentante = item.IdRepresentante;
                            entity.PorcComision = item.PorcComision;
                            entity.TotalComision = item.TotalComision;

                            modificados.Add(entity);
                        }
                    }
                }

                if (nuevos.Count > 0)
                {
                    _db.VentasArtistas.AddRange(nuevos);
                    await _db.SaveChangesAsync();
                }

                var afectados = nuevos.Concat(modificados).ToList();

                foreach (var entity in afectados)
                {
                    if (entity.TotalComision > 0)
                    {
                        await UpsertMovimientoVentaArtistaAsync(venta, entity);
                    }
                    else
                    {
                        await EliminarMovimientoArtistaAsync(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al sincronizar artistas de la venta {venta.Id}.", ex);
            }
        }

        private async Task SincronizarPersonalAsync(Venta venta, List<VentasPersonal>? personal)
        {
            try
            {
                personal ??= new();

                var incoming = personal
                    .Where(x => x.IdPersonal > 0 && x.IdCargo > 0 && x.IdTipoComision > 0)
                    .ToList();

                var actuales = await _db.VentasPersonals
                    .Where(x => x.IdVenta == venta.Id)
                    .ToListAsync();

                var actualesDict = actuales.ToDictionary(x => x.Id, x => x);
                var idsIncomingExistentes = incoming
                    .Where(x => x.Id > 0)
                    .Select(x => x.Id)
                    .ToHashSet();

                var eliminar = actuales
                    .Where(x => !idsIncomingExistentes.Contains(x.Id))
                    .ToList();

                foreach (var item in eliminar)
                {
                    await EliminarMovimientoPersonalAsync(item);
                }

                if (eliminar.Count > 0)
                {
                    _db.VentasPersonals.RemoveRange(eliminar);
                    await _db.SaveChangesAsync();
                }

                var nuevos = new List<VentasPersonal>();
                var modificados = new List<VentasPersonal>();

                foreach (var item in incoming)
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
                            IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                            FechaRegistra = DateTime.Now
                        };

                        nuevos.Add(nuevo);
                    }
                    else if (actualesDict.TryGetValue(item.Id, out var entity))
                    {
                        var cambio =
                            entity.IdPersonal != item.IdPersonal ||
                            entity.IdCargo != item.IdCargo ||
                            entity.IdTipoComision != item.IdTipoComision ||
                            entity.PorcComision != item.PorcComision ||
                            entity.TotalComision != item.TotalComision;

                        if (cambio)
                        {
                            entity.IdPersonal = item.IdPersonal;
                            entity.IdCargo = item.IdCargo;
                            entity.IdTipoComision = item.IdTipoComision;
                            entity.PorcComision = item.PorcComision;
                            entity.TotalComision = item.TotalComision;
                            entity.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                            entity.FechaModifica = DateTime.Now;

                            modificados.Add(entity);
                        }
                    }
                }

                if (nuevos.Count > 0)
                {
                    _db.VentasPersonals.AddRange(nuevos);
                    await _db.SaveChangesAsync();
                }

                var afectados = nuevos.Concat(modificados).ToList();

                foreach (var entity in afectados)
                {
                    if (entity.TotalComision > 0)
                    {
                        await UpsertMovimientoVentaPersonalAsync(venta, entity);
                    }
                    else
                    {
                        await EliminarMovimientoPersonalAsync(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al sincronizar personal de la venta {venta.Id}.", ex);
            }
        }

        private async Task SincronizarCobrosAsync(Venta venta, List<VentasCobro>? cobros)
        {
            try
            {
                cobros ??= new();

                var actuales = await _db.Set<VentasCobro>()
                    .Where(x => x.IdVenta == venta.Id)
                    .ToListAsync();

                var actualesDict = actuales.ToDictionary(x => x.Id, x => x);

                if (!cobros.Any())
                {
                    var idsActuales = actuales.Select(x => x.Id).ToList();

                    if (idsActuales.Count > 0)
                        await EliminarMovimientosComisionCobroPorListaAsync(idsActuales);

                    foreach (var item in actuales)
                    {
                        await EliminarMovimientoArtistaCobroAsync(item);
                        await EliminarMovimientoCobroClienteAsync(item);
                        await EliminarMovimientoCajaCobroAsync(item);
                    }

                    if (actuales.Count > 0)
                    {
                        _db.Set<VentasCobro>().RemoveRange(actuales);
                        await _db.SaveChangesAsync();
                    }

                    return;
                }

                var idsIncomingExistentes = cobros
                    .Where(x => x.Id > 0)
                    .Select(x => x.Id)
                    .ToHashSet();

                var eliminar = actuales
                    .Where(x => !idsIncomingExistentes.Contains(x.Id))
                    .ToList();

                if (eliminar.Count > 0)
                {
                    var idsEliminar = eliminar.Select(x => x.Id).ToList();

                    await EliminarMovimientosComisionCobroPorListaAsync(idsEliminar);

                    foreach (var item in eliminar)
                    {
                        await EliminarMovimientoArtistaCobroAsync(item);
                        await EliminarMovimientoCobroClienteAsync(item);
                        await EliminarMovimientoCajaCobroAsync(item);
                    }

                    _db.Set<VentasCobro>().RemoveRange(eliminar);
                    await _db.SaveChangesAsync();
                }

                var nuevos = new List<VentasCobro>();
                var modificados = new List<VentasCobro>();

                foreach (var item in cobros)
                {
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
                            Conversion = CalcularConversion(item),
                            NotaCliente = item.NotaCliente,
                            NotaInterna = item.NotaInterna,
                            IdUsuarioRegistra = item.IdUsuarioRegistra > 0
                                ? item.IdUsuarioRegistra
                                : ObtenerUsuarioActual(venta),
                            FechaRegistra = item.FechaRegistra == default
                                ? DateTime.Now
                                : item.FechaRegistra
                        };

                        nuevos.Add(nuevo);
                    }
                    else if (actualesDict.TryGetValue(item.Id, out var actual))
                    {
                        var nuevaFecha = item.Fecha == default ? actual.Fecha : item.Fecha;
                        var nuevaCotizacion = item.Cotizacion <= 0 ? 1 : item.Cotizacion;
                        var nuevaConversion = CalcularConversion(item);

                        var cambio =
                            actual.Fecha != nuevaFecha ||
                            actual.IdMoneda != item.IdMoneda ||
                            actual.IdCuenta != item.IdCuenta ||
                            actual.Importe != item.Importe ||
                            actual.Cotizacion != nuevaCotizacion ||
                            actual.Conversion != nuevaConversion ||
                            actual.NotaCliente != item.NotaCliente ||
                            actual.NotaInterna != item.NotaInterna;

                        if (cambio)
                        {
                            actual.Fecha = nuevaFecha;
                            actual.IdMoneda = item.IdMoneda;
                            actual.IdCuenta = item.IdCuenta;
                            actual.Importe = item.Importe;
                            actual.Cotizacion = nuevaCotizacion;
                            actual.Conversion = nuevaConversion;
                            actual.NotaCliente = item.NotaCliente;
                            actual.NotaInterna = item.NotaInterna;
                            actual.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                            actual.FechaModifica = DateTime.Now;

                            modificados.Add(actual);
                        }
                    }
                }

                if (nuevos.Count > 0)
                {
                    _db.Set<VentasCobro>().AddRange(nuevos);
                    await _db.SaveChangesAsync();
                }

                var afectados = nuevos.Concat(modificados).ToList();

                foreach (var cobro in afectados)
                {
                    await UpsertMovimientoCobroClienteAsync(venta, cobro);
                    await UpsertMovimientoCajaCobroAsync(venta, cobro);

                    await UpsertMovimientoArtistaCobroAsync(venta, cobro);
                    await SincronizarComisionesPersonalCobroAsync(venta, cobro);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al sincronizar cobros de la venta {venta.Id}.", ex);
            }
        }
        private async Task SincronizarComisionesPersonalCobroAsync(Venta venta, VentasCobro cobro)
        {
            try
            {
                var personalVenta = await _db.VentasPersonals
                    .Where(x => x.IdVenta == venta.Id && x.PorcComision > 0)
                    .ToListAsync();

                var actuales = await _db.VentasCobrosComisiones
                    .Where(x => x.IdVentaCobro == cobro.Id)
                    .ToListAsync();

                var actualesDict = actuales.ToDictionary(x => x.IdPersonal, x => x);
                var idsPersonalVenta = personalVenta.Select(x => x.IdPersonal).ToHashSet();

                var eliminar = actuales
                    .Where(x => !idsPersonalVenta.Contains(x.IdPersonal))
                    .ToList();

                foreach (var item in eliminar)
                {
                    if (item.IdPersonalCc.HasValue && item.IdPersonalCc.Value > 0)
                    {
                        var mov = await _db.PersonalCuentaCorrientes
                            .FirstOrDefaultAsync(x => x.Id == item.IdPersonalCc.Value);

                        if (mov != null)
                            _db.PersonalCuentaCorrientes.Remove(mov);
                    }
                }

                if (eliminar.Count > 0)
                {
                    _db.VentasCobrosComisiones.RemoveRange(eliminar);
                    await _db.SaveChangesAsync();
                }

                foreach (var per in personalVenta)
                {
                    var total = Math.Round(cobro.Conversion * (per.PorcComision / 100m), 2);

                    if (total <= 0)
                        continue;

                    if (!actualesDict.TryGetValue(per.IdPersonal, out var comision))
                    {
                        comision = new VentasCobrosComision
                        {
                            IdVentaCobro = cobro.Id,
                            IdPersonal = per.IdPersonal,
                            TotalComision = total,
                            IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                            FechaRegistra = DateTime.Now,
                            IdPersonalCc = null
                        };

                        _db.VentasCobrosComisiones.Add(comision);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        comision.TotalComision = total;
                        comision.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                        comision.FechaModifica = DateTime.Now;
                        await _db.SaveChangesAsync();
                    }

                    await UpsertMovimientoPersonalCobroAsync(venta, cobro, comision);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al sincronizar comisiones de personal del cobro {cobro.Id}.", ex);
            }
        }

        private async Task UpsertMovimientoPersonalCobroAsync(Venta venta, VentasCobro cobro, VentasCobrosComision comision)
        {
            try
            {
                if (comision.TotalComision <= 0)
                {
                    if (comision.IdPersonalCc.HasValue && comision.IdPersonalCc.Value > 0)
                    {
                        var movEliminar = await _db.PersonalCuentaCorrientes
                            .FirstOrDefaultAsync(x => x.Id == comision.IdPersonalCc.Value);

                        if (movEliminar != null)
                        {
                            _db.PersonalCuentaCorrientes.Remove(movEliminar);
                            await _db.SaveChangesAsync();
                        }

                        comision.IdPersonalCc = null;
                        await _db.SaveChangesAsync();
                    }

                    return;
                }

                PersonalCuentaCorriente? mov = null;

                if (comision.IdPersonalCc.HasValue && comision.IdPersonalCc.Value > 0)
                {
                    mov = await _db.PersonalCuentaCorrientes
                        .FirstOrDefaultAsync(x => x.Id == comision.IdPersonalCc.Value);
                }

                mov ??= await _db.PersonalCuentaCorrientes
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == "COMISION COBRO" &&
                        x.IdMov == cobro.Id &&
                        x.IdPersonal == comision.IdPersonal);

                if (mov == null)
                {
                    mov = new PersonalCuentaCorriente
                    {
                        IdPersonal = comision.IdPersonal,
                        IdMoneda = venta.IdMoneda,
                        TipoMov = "COMISION COBRO",
                        IdMov = cobro.Id,
                        Fecha = cobro.Fecha,
                        Concepto = $"Comisión cobro venta {venta.NombreEvento}",
                        Debe = 0,
                        Haber = comision.TotalComision,
                        IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                        FechaRegistra = DateTime.Now
                    };

                    _db.PersonalCuentaCorrientes.Add(mov);
                    await _db.SaveChangesAsync();

                    comision.IdPersonalCc = mov.Id;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    mov.IdPersonal = comision.IdPersonal;
                    mov.IdMoneda = venta.IdMoneda;
                    mov.Fecha = cobro.Fecha;
                    mov.Concepto = $"Comisión cobro venta {venta.NombreEvento}";
                    mov.Debe = comision.TotalComision;
                    mov.Haber = 0;
                    mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                    mov.FechaModifica = DateTime.Now;

                    if (comision.IdPersonalCc != mov.Id)
                        comision.IdPersonalCc = mov.Id;

                    await _db.SaveChangesAsync();
                }

                await UpsertMovimientoCajaComisionAsync(
    venta,
    cobro.Id,
    comision.TotalComision,
    venta.IdMoneda,
    cobro.IdCuenta,
    $"Comisión personal cobro venta {venta.NombreEvento}"
);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al upsertar movimiento de personal del cobro {cobro.Id}.", ex);
            }
        }


        private async Task EliminarMovimientoArtistaCobroAsync(VentasCobro cobro)
        {
            try
            {
                var pagos = await _db.ArtistasPagos
                    .Where(x => x.IdCobro == cobro.Id)
                    .ToListAsync();

                foreach (var pago in pagos)
                {
                    if (pago.IdCaja.HasValue)
                    {
                        var caja = await _db.Cajas
                            .FirstOrDefaultAsync(x => x.Id == pago.IdCaja.Value);

                        if (caja != null)
                            _db.Cajas.Remove(caja);
                    }

                    if (pago.IdArtistaCc.HasValue)
                    {
                        var cc = await _db.ArtistasCuentaCorrientes
                            .FirstOrDefaultAsync(x => x.Id == pago.IdArtistaCc.Value);

                        if (cc != null)
                            _db.ArtistasCuentaCorrientes.Remove(cc);
                    }

                    _db.ArtistasPagos.Remove(pago);
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar pago artista del cobro {cobro.Id}.", ex);
            }
        }

        private async Task UpsertMovimientoVentaPersonalAsync(Venta venta, VentasPersonal personal)
        {
            try
            {
                var mov = await _db.PersonalCuentaCorrientes
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_VENTA_PERSONAL &&
                        x.IdMov == personal.Id);

                if (personal.TotalComision <= 0)
                {
                    if (mov != null)
                    {
                        _db.PersonalCuentaCorrientes.Remove(mov);
                        await _db.SaveChangesAsync();
                    }
                    return;
                }

                if (mov == null)
                {
                    mov = new PersonalCuentaCorriente
                    {
                        IdPersonal = personal.IdPersonal,
                        IdMoneda = venta.IdMoneda,
                        TipoMov = TIPO_MOV_VENTA_PERSONAL,
                        IdMov = personal.Id,
                        Fecha = venta.Fecha,
                        Concepto = $"Comisión venta {venta.NombreEvento}",
                        Debe = personal.TotalComision,
                        Haber = 0,
                        IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                        FechaRegistra = DateTime.Now
                    };

                    _db.PersonalCuentaCorrientes.Add(mov);
                }
                else
                {
                    mov.IdPersonal = personal.IdPersonal;
                    mov.IdMoneda = venta.IdMoneda;
                    mov.Fecha = venta.Fecha;
                    mov.Concepto = $"Comisión venta {venta.NombreEvento}";
                    mov.Debe = personal.TotalComision;
                    mov.Haber = 0;
                    mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                    mov.FechaModifica = DateTime.Now;
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error al upsertar movimiento de personal para la venta {venta.Id} y detalle {personal.Id}.",
                    ex);
            }
        }

        private async Task EliminarMovimientoPersonalAsync(VentasPersonal personal)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error al eliminar movimiento de personal del detalle {personal.Id}.",
                    ex);
            }
        }

        private async Task UpsertMovimientoVentaClienteAsync(Venta venta)
        {
            try
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
                    }

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al upsertar movimiento de venta del cliente para la venta {venta.Id}.", ex);
            }
        }

        private async Task EliminarMovimientoVentaClienteAsync(Venta venta)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar movimiento de venta del cliente para la venta {venta.Id}.", ex);
            }
        }

        private async Task EliminarMovimientoArtistaAsync(VentasArtista artista)
        {
            try
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

                if (artista.IdArtistaCc.HasValue && artista.IdArtistaCc.Value > 0)
                {
                    artista.IdArtistaCc = null;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error al eliminar movimiento del artista del detalle {artista.Id}.",
                    ex);
            }
        }

        private async Task UpsertMovimientoCobroClienteAsync(Venta venta, VentasCobro cobro)
        {
            try
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
                    }

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al upsertar movimiento de cobro del cliente para la venta {venta.Id}.", ex);
            }
        }

        private async Task EliminarMovimientoCobroClienteAsync(VentasCobro cobro)
        {
            try
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

                if (cobro.IdClienteCc.HasValue)
                {
                    cobro.IdClienteCc = null;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar movimiento de cobro del cliente del cobro {cobro.Id}.", ex);
            }
        }

        private async Task UpsertMovimientoCajaCobroAsync(Venta venta, VentasCobro cobro)
        {
            try
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
                        Egreso = 0,
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
                    mov.Egreso = 0;
                    mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                    mov.FechaModifica = DateTime.Now;

                    if (cobro.IdCaja != mov.Id)
                    {
                        cobro.IdCaja = mov.Id;
                    }

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al upsertar movimiento de caja del cobro {cobro.Id}.", ex);
            }
        }

        private async Task EliminarMovimientoCajaCobroAsync(VentasCobro cobro)
        {
            try
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

                if (cobro.IdCaja.HasValue)
                {
                    cobro.IdCaja = null;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar movimiento de caja del cobro {cobro.Id}.", ex);
            }
        }

        private async Task RecalcularImportesVentaAsync(int idVenta)
        {
            try
            {
                var venta = await _db.Ventas.FirstOrDefaultAsync(x => x.Id == idVenta);
                if (venta == null)
                    return;

                var totalCobrado = await _db.Set<VentasCobro>()
                    .Where(x => x.IdVenta == idVenta)
                    .SumAsync(x => (decimal?)x.Conversion) ?? 0m;

                venta.ImporteAbonado = totalCobrado;
                venta.Saldo = venta.ImporteTotal - totalCobrado;

                await _db.SaveChangesAsync();

                await UpsertMovimientoVentaClienteAsync(venta);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al recalcular importes de la venta {idVenta}.", ex);
            }
        }

        private int ObtenerUsuarioActual(Venta venta)
        {
            if (venta.IdUsuarioModifica.HasValue && venta.IdUsuarioModifica.Value > 0)
                return venta.IdUsuarioModifica.Value;

            return venta.IdUsuarioRegistra;
        }

        

        private async Task EliminarMovimientosComisionCobroPorListaAsync(List<int> idsVentaCobro)
        {
            try
            {
                if (idsVentaCobro == null || idsVentaCobro.Count == 0)
                    return;

                var comisiones = await _db.VentasCobrosComisiones
                    .Where(x => idsVentaCobro.Contains(x.IdVentaCobro))
                    .ToListAsync();

                if (!comisiones.Any())
                    return;

                var idsPersonalCc = comisiones
                    .Where(x => x.IdPersonalCc.HasValue && x.IdPersonalCc.Value > 0)
                    .Select(x => x.IdPersonalCc!.Value)
                    .Distinct()
                    .ToList();

                if (idsPersonalCc.Count > 0)
                {
                    var movs = await _db.PersonalCuentaCorrientes
                        .Where(x => idsPersonalCc.Contains(x.Id))
                        .ToListAsync();

                    if (movs.Count > 0)
                        _db.PersonalCuentaCorrientes.RemoveRange(movs);
                }

                _db.VentasCobrosComisiones.RemoveRange(comisiones);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar comisiones de múltiples cobros.", ex);
            }
        }

        private async Task UpsertMovimientoArtistaCobroAsync(Venta venta, VentasCobro cobro)
        {
            try
            {
                var artistasVenta = await _db.VentasArtistas
                    .Where(x => x.IdVenta == venta.Id && x.PorcComision > 0)
                    .ToListAsync();

                if (!artistasVenta.Any())
                {
                    await EliminarMovimientoArtistaCobroAsync(cobro);
                    return;
                }

                foreach (var artistaVenta in artistasVenta)
                {
                    var totalComision = Math.Round(
                        cobro.Conversion * (artistaVenta.PorcComision / 100m),
                        2);

                    if (totalComision <= 0)
                        continue;

                    var concepto = $"Comisión cobro venta {venta.NombreEvento}";

                    var pago = await _db.ArtistasPagos
                        .FirstOrDefaultAsync(x =>
                            x.IdCobro == cobro.Id &&
                            x.IdArtista == artistaVenta.IdArtista);

                    if (pago == null)
                    {
                        pago = new ArtistasPago
                        {
                            IdCobro = cobro.Id,
                            IdArtista = artistaVenta.IdArtista,
                            Fecha = cobro.Fecha,
                            Concepto = concepto,

                            IdMoneda = venta.IdMoneda,
                            IdCuenta = cobro.IdCuenta,

                            Importe = totalComision,
                            Cotizacion = 1,
                            Conversion = totalComision,

                            IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                            FechaRegistra = DateTime.Now
                        };

                        _db.ArtistasPagos.Add(pago);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        pago.Importe = totalComision;
                        pago.Conversion = totalComision;
                        pago.IdCuenta = cobro.IdCuenta;
                        pago.Fecha = cobro.Fecha;

                        pago.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                        pago.FechaModifica = DateTime.Now;

                        await _db.SaveChangesAsync();
                    }

                    await UpsertMovimientoCajaPagoArtistaAsync(venta, pago);
                    await UpsertMovimientoCuentaCorrientePagoArtistaAsync(venta, pago);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar pago de artista del cobro {cobro.Id}.", ex);
            }
        }

        private async Task UpsertMovimientoCajaPagoArtistaAsync(Venta venta, ArtistasPago pago)
        {
            var mov = await _db.Cajas
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == "PAGO_ARTISTA" &&
                    x.IdMov == pago.Id);

            if (mov == null)
            {
                mov = new Caja
                {
                    TipoMov = "PAGO_ARTISTA",
                    IdMov = pago.Id,
                    Fecha = pago.Fecha,
                    Concepto = pago.Concepto,
                    IdMoneda = pago.IdMoneda,
                    IdCuenta = pago.IdCuenta,
                    Ingreso = 0,
                    Egreso = pago.Conversion,
                    Saldo = 0,
                    IdUsuarioRegistra = pago.IdUsuarioRegistra,
                    FechaRegistra = DateTime.Now
                };

                _db.Cajas.Add(mov);
                await _db.SaveChangesAsync();

                pago.IdCaja = mov.Id;
                await _db.SaveChangesAsync();
            }
            else
            {
                mov.Egreso = pago.Conversion;
                mov.IdCuenta = pago.IdCuenta;

                await _db.SaveChangesAsync();
            }
        }

        private async Task UpsertMovimientoCuentaCorrientePagoArtistaAsync(Venta venta, ArtistasPago pago)
        {
            ArtistasCuentaCorriente? mov = null;

            if (pago.IdArtistaCc.HasValue && pago.IdArtistaCc.Value > 0)
            {
                mov = await _db.ArtistasCuentaCorrientes
                    .FirstOrDefaultAsync(x => x.Id == pago.IdArtistaCc.Value);
            }

            mov ??= await _db.ArtistasCuentaCorrientes
                .FirstOrDefaultAsync(x =>
                    x.TipoMov == "PAGO ARTISTA" &&
                    x.IdMov == pago.Id);

            if (mov == null)
            {
                mov = new ArtistasCuentaCorriente
                {
                    IdArtista = pago.IdArtista,
                    IdMoneda = pago.IdMoneda,
                    TipoMov = "PAGO ARTISTA",
                    IdMov = pago.Id,
                    Fecha = pago.Fecha,
                    Concepto = pago.Concepto,
                    Debe = pago.Conversion,
                    Haber = 0,
                    IdUsuarioRegistra = pago.IdUsuarioRegistra,
                    FechaRegistra = DateTime.Now
                };

                _db.ArtistasCuentaCorrientes.Add(mov);
                await _db.SaveChangesAsync();

                pago.IdArtistaCc = mov.Id;
                await _db.SaveChangesAsync();
            }
            else
            {
                mov.Debe = pago.Conversion;
                mov.Fecha = pago.Fecha;

                await _db.SaveChangesAsync();
            }
        }

        private async Task UpsertMovimientoVentaArtistaAsync(Venta venta, VentasArtista artista)
        {
            try
            {
                ArtistasCuentaCorriente? mov = null;

                if (artista.IdArtistaCc.HasValue && artista.IdArtistaCc.Value > 0)
                {
                    mov = await _db.ArtistasCuentaCorrientes
                        .FirstOrDefaultAsync(x => x.Id == artista.IdArtistaCc.Value);
                }

                mov ??= await _db.ArtistasCuentaCorrientes
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_VENTA_ARTISTA &&
                        x.IdMov == artista.Id);

                if (artista.TotalComision <= 0)
                {
                    if (mov != null)
                    {
                        _db.ArtistasCuentaCorrientes.Remove(mov);
                        await _db.SaveChangesAsync();
                    }

                    if (artista.IdArtistaCc.HasValue)
                    {
                        artista.IdArtistaCc = null;
                        await _db.SaveChangesAsync();
                    }

                    return;
                }

                if (mov == null)
                {
                    mov = new ArtistasCuentaCorriente
                    {
                        IdArtista = artista.IdArtista,
                        IdMoneda = venta.IdMoneda,
                        TipoMov = TIPO_MOV_VENTA_ARTISTA,
                        IdMov = artista.Id,
                        Fecha = venta.Fecha,
                        Concepto = $"Comisión venta {venta.NombreEvento}",
                        Debe = 0,
                        Haber = artista.TotalComision,
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
                    mov.Concepto = $"Comisión venta {venta.NombreEvento}";
                    mov.Debe = artista.TotalComision;
                    mov.Haber = 0;
                    mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                    mov.FechaModifica = DateTime.Now;

                    if (artista.IdArtistaCc != mov.Id)
                    {
                        artista.IdArtistaCc = mov.Id;
                    }

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error al upsertar movimiento del artista para la venta {venta.Id} y detalle {artista.Id}.",
                    ex);
            }
        }

        private void NormalizarVenta(Venta venta)
        {
            if (venta.ImporteTotal < 0)
                venta.ImporteTotal = 0;

            if (venta.ImporteAbonado < 0)
                venta.ImporteAbonado = 0;

            if (venta.Saldo < 0 && venta.ImporteTotal == 0)
                venta.Saldo = 0;
        }

        private decimal CalcularConversion(VentasCobro cobro)
        {
            if (cobro.Conversion > 0)
                return cobro.Conversion;

            var cotizacion = cobro.Cotizacion <= 0 ? 1 : cobro.Cotizacion;
            return cobro.Importe * cotizacion;
        }

        private async Task UpsertMovimientoCajaComisionAsync(
    Venta venta,
    int idMov,
    decimal importe,
    int idMoneda,
    int idCuenta,
    string concepto)
        {
            try
            {
                var mov = await _db.Cajas
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == "COMISION" &&
                        x.IdMov == idMov &&
                        x.Concepto == concepto);

                if (importe <= 0)
                {
                    if (mov != null)
                    {
                        _db.Cajas.Remove(mov);
                        await _db.SaveChangesAsync();
                    }
                    return;
                }

                if (mov == null)
                {
                    mov = new Caja
                    {
                        TipoMov = "COMISION",
                        IdMov = idMov,
                        Fecha = DateTime.Now,
                        Concepto = concepto,
                        IdMoneda = idMoneda,
                        IdCuenta = idCuenta,
                        Ingreso = 0,
                        Egreso = importe,
                        Saldo = 0,
                        IdUsuarioRegistra = ObtenerUsuarioActual(venta),
                        FechaRegistra = DateTime.Now
                    };

                    _db.Cajas.Add(mov);
                }
                else
                {
                    mov.Egreso = importe;
                    mov.IdUsuarioModifica = ObtenerUsuarioActual(venta);
                    mov.FechaModifica = DateTime.Now;
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar egreso de caja por comisión.", ex);
            }
        }
    }

}