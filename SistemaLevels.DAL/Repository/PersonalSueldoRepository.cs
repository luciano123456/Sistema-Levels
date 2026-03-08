using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class PersonalSueldosRepository : IPersonalSueldosRepository<PersonalSueldo>
    {
        private readonly SistemaLevelsContext _db;

        private const string TIPO_MOV_SUELDO_PERSONAL = "SUELDO";
        private const string TIPO_MOV_PAGO_SUELDO_PERSONAL = "PAGO SUELDO";
        private const string TIPO_MOV_PAGO_SUELDO_CAJA = "SUELDO";

        public PersonalSueldosRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        public async Task<bool> Insertar(PersonalSueldo sueldo, List<PersonalSueldosPago> pagos)
        {
            await using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                pagos ??= new();

                NormalizarSueldo(sueldo);

                _db.PersonalSueldos.Add(sueldo);
                await _db.SaveChangesAsync();

                await UpsertMovimientoSueldoPersonalAsync(sueldo);
                await SincronizarPagosAsync(sueldo, pagos);

                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> Actualizar(PersonalSueldo sueldo, List<PersonalSueldosPago> pagos)
        {
            await using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                pagos ??= new();

                var entity = await _db.PersonalSueldos
                    .FirstOrDefaultAsync(x => x.Id == sueldo.Id);

                if (entity == null)
                    return false;

                entity.Fecha = sueldo.Fecha;
                entity.IdPersonal = sueldo.IdPersonal;
                entity.IdMoneda = sueldo.IdMoneda;
                entity.Concepto = sueldo.Concepto;
                entity.ImporteTotal = sueldo.ImporteTotal;
                entity.NotaPersonal = sueldo.NotaPersonal;
                entity.NotaInterna = sueldo.NotaInterna;
                entity.IdUsuarioModifica = sueldo.IdUsuarioModifica;
                entity.FechaModifica = DateTime.Now;

                NormalizarSueldo(entity);

                await _db.SaveChangesAsync();

                await UpsertMovimientoSueldoPersonalAsync(entity);
                await SincronizarPagosAsync(entity, pagos);

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
                var sueldo = await _db.PersonalSueldos
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (sueldo == null)
                    return false;

                var pagos = await _db.Set<PersonalSueldosPago>()
                    .Where(x => x.IdSueldo == id)
                    .ToListAsync();

                foreach (var pago in pagos)
                {
                    await EliminarMovimientoPagoPersonalAsync(pago);
                    await EliminarMovimientoCajaPagoAsync(pago);
                    await EliminarGastoPagoAsync(sueldo, pago);
                }

                if (pagos.Count > 0)
                {
                    _db.Set<PersonalSueldosPago>().RemoveRange(pagos);
                    await _db.SaveChangesAsync();
                }

                await EliminarMovimientoSueldoPersonalAsync(sueldo);

                _db.PersonalSueldos.Remove(sueldo);
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

        public async Task<PersonalSueldo?> Obtener(int id)
        {
            try
            {
                return await _db.PersonalSueldos
                    .AsNoTracking()
                    .Include(x => x.IdPersonalNavigation)
                    .Include(x => x.IdMonedaNavigation)
                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation)
                    .Include(x => x.PersonalSueldosPagos)
                        .ThenInclude(p => p.IdMonedaNavigation)
                    .Include(x => x.PersonalSueldosPagos)
                        .ThenInclude(p => p.IdCuentaNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el sueldo {id}.", ex);
            }
        }

        public async Task<IQueryable<PersonalSueldo>> ObtenerTodos()
        {
            try
            {
                var q = _db.PersonalSueldos
                    .AsNoTracking()
                    .Include(x => x.IdPersonalNavigation)
                    .Include(x => x.IdMonedaNavigation)
                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation)
                    .Include(x => x.PersonalSueldosPagos);

                return await Task.FromResult(q);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener todos los sueldos.", ex);
            }
        }

        public async Task<IQueryable<PersonalSueldo>> ObtenerPorPersonal(int idPersonal)
        {
            try
            {
                var q = _db.PersonalSueldos
                    .AsNoTracking()
                    .Where(x => x.IdPersonal == idPersonal)
                    .Include(x => x.IdPersonalNavigation)
                    .Include(x => x.IdMonedaNavigation)
                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation)
                    .Include(x => x.PersonalSueldosPagos);

                return await Task.FromResult(q);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener sueldos del personal {idPersonal}.", ex);
            }
        }

        public async Task<IQueryable<PersonalSueldo>> ListarFiltrado(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idPersonal,
            int? idMoneda,
            string? estado)
        {
            try
            {
                IQueryable<PersonalSueldo> query = _db.PersonalSueldos
                    .AsNoTracking()
                    .Include(x => x.IdPersonalNavigation)
                    .Include(x => x.IdMonedaNavigation)
                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation)
                    .Include(x => x.PersonalSueldosPagos);

                if (fechaDesde.HasValue)
                    query = query.Where(x => x.Fecha >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                    query = query.Where(x => x.Fecha <= fechaHasta.Value);

                if (idPersonal.HasValue)
                    query = query.Where(x => x.IdPersonal == idPersonal.Value);

                if (idMoneda.HasValue)
                    query = query.Where(x => x.IdMoneda == idMoneda.Value);

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    var est = estado.Trim().ToUpper();

                    if (est == "PENDIENTE")
                    {
                        query = query.Where(x =>
                            (x.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) <= 0m);
                    }
                    else if (est == "PARCIAL")
                    {
                        query = query.Where(x =>
                            (x.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) > 0m &&
                            (x.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) < x.ImporteTotal);
                    }
                    else if (est == "PAGADO")
                    {
                        query = query.Where(x =>
                            (x.PersonalSueldosPagos.Sum(p => (decimal?)p.Conversion) ?? 0m) >= x.ImporteTotal &&
                            x.ImporteTotal > 0m);
                    }
                }

                return await Task.FromResult(query);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar sueldos filtrados.", ex);
            }
        }

        /*
        =========================================================
        SUELDO -> CUENTA CORRIENTE PERSONAL
        =========================================================
        */

        private async Task UpsertMovimientoSueldoPersonalAsync(PersonalSueldo sueldo)
        {
            try
            {
                PersonalCuentaCorriente? mov = null;

                if (sueldo.IdPersonalCc.HasValue && sueldo.IdPersonalCc.Value > 0)
                {
                    mov = await _db.PersonalCuentaCorrientes
                        .FirstOrDefaultAsync(x => x.Id == sueldo.IdPersonalCc.Value);
                }

                mov ??= await _db.PersonalCuentaCorrientes
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_SUELDO_PERSONAL &&
                        x.IdMov == sueldo.Id);

                if (sueldo.ImporteTotal <= 0)
                {
                    if (mov != null)
                    {
                        _db.PersonalCuentaCorrientes.Remove(mov);
                        await _db.SaveChangesAsync();
                    }

                    sueldo.IdPersonalCc = null;
                    await _db.SaveChangesAsync();
                    return;
                }

                if (mov == null)
                {
                    mov = new PersonalCuentaCorriente
                    {
                        IdPersonal = sueldo.IdPersonal,
                        IdMoneda = sueldo.IdMoneda,
                        TipoMov = TIPO_MOV_SUELDO_PERSONAL,
                        IdMov = sueldo.Id,
                        Fecha = sueldo.Fecha,
                        Concepto = $"Sueldo {sueldo.Concepto}",
                        Debe = sueldo.ImporteTotal,
                        Haber = 0,
                        IdUsuarioRegistra = ObtenerUsuarioActual(sueldo),
                        FechaRegistra = DateTime.Now
                    };

                    _db.PersonalCuentaCorrientes.Add(mov);
                    await _db.SaveChangesAsync();

                    sueldo.IdPersonalCc = mov.Id;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    mov.IdPersonal = sueldo.IdPersonal;
                    mov.IdMoneda = sueldo.IdMoneda;
                    mov.Fecha = sueldo.Fecha;
                    mov.Concepto = $"Sueldo {sueldo.Concepto}";
                    mov.Debe = sueldo.ImporteTotal;
                    mov.Haber = 0;
                    mov.IdUsuarioModifica = ObtenerUsuarioActual(sueldo);
                    mov.FechaModifica = DateTime.Now;

                    if (sueldo.IdPersonalCc != mov.Id)
                        sueldo.IdPersonalCc = mov.Id;

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al upsertar movimiento de sueldo {sueldo.Id}.", ex);
            }
        }

        private async Task EliminarMovimientoSueldoPersonalAsync(PersonalSueldo sueldo)
        {
            try
            {
                PersonalCuentaCorriente? mov = null;

                if (sueldo.IdPersonalCc.HasValue && sueldo.IdPersonalCc.Value > 0)
                {
                    mov = await _db.PersonalCuentaCorrientes
                        .FirstOrDefaultAsync(x => x.Id == sueldo.IdPersonalCc.Value);
                }

                mov ??= await _db.PersonalCuentaCorrientes
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_SUELDO_PERSONAL &&
                        x.IdMov == sueldo.Id);

                if (mov != null)
                {
                    _db.PersonalCuentaCorrientes.Remove(mov);
                    await _db.SaveChangesAsync();
                }

                if (sueldo.IdPersonalCc.HasValue)
                {
                    sueldo.IdPersonalCc = null;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar movimiento de sueldo {sueldo.Id}.", ex);
            }
        }

        /*
        =========================================================
        PAGOS
        =========================================================
        */

        private async Task SincronizarPagosAsync(PersonalSueldo sueldo, List<PersonalSueldosPago>? pagos)
        {
            try
            {
                pagos ??= new();

                var actuales = await _db.Set<PersonalSueldosPago>()
                    .Where(x => x.IdSueldo == sueldo.Id)
                    .ToListAsync();

                if (!pagos.Any())
                {
                    foreach (var item in actuales)
                    {
                        await EliminarMovimientoPagoPersonalAsync(item);
                        await EliminarMovimientoCajaPagoAsync(item);
                        await EliminarGastoPagoAsync(sueldo, item);
                    }

                    if (actuales.Count > 0)
                    {
                        _db.Set<PersonalSueldosPago>().RemoveRange(actuales);
                        await _db.SaveChangesAsync();
                    }

                    return;
                }

                var actualesDict = actuales.ToDictionary(x => x.Id, x => x);

                var idsIncomingExistentes = pagos
                    .Where(x => x.Id > 0)
                    .Select(x => x.Id)
                    .ToHashSet();

                var eliminar = actuales
                    .Where(x => !idsIncomingExistentes.Contains(x.Id))
                    .ToList();

                foreach (var item in eliminar)
                {
                    await EliminarMovimientoPagoPersonalAsync(item);
                    await EliminarMovimientoCajaPagoAsync(item);
                    await EliminarGastoPagoAsync(sueldo, item);
                }

                if (eliminar.Count > 0)
                {
                    _db.Set<PersonalSueldosPago>().RemoveRange(eliminar);
                    await _db.SaveChangesAsync();
                }

                var nuevos = new List<PersonalSueldosPago>();
                var modificados = new List<PersonalSueldosPago>();

                foreach (var item in pagos)
                {
                    if (item.Id <= 0)
                    {
                        var nuevo = new PersonalSueldosPago
                        {
                            IdSueldo = sueldo.Id,
                            IdPersonalCc = null,
                            IdCaja = null,
                            Fecha = item.Fecha == default ? DateTime.Now : item.Fecha,
                            IdMoneda = item.IdMoneda,
                            IdCuenta = item.IdCuenta,
                            Importe = item.Importe,
                            Cotizacion = item.Cotizacion <= 0 ? 1 : item.Cotizacion,
                            Conversion = CalcularConversion(item),
                            IdUsuarioRegistra = item.IdUsuarioRegistra > 0
                                ? item.IdUsuarioRegistra
                                : ObtenerUsuarioActual(sueldo),
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
                            actual.Conversion != nuevaConversion;

                        if (cambio)
                        {
                            actual.Fecha = nuevaFecha;
                            actual.IdMoneda = item.IdMoneda;
                            actual.IdCuenta = item.IdCuenta;
                            actual.Importe = item.Importe;
                            actual.Cotizacion = nuevaCotizacion;
                            actual.Conversion = nuevaConversion;
                            actual.IdUsuarioModifica = ObtenerUsuarioActual(sueldo);
                            actual.FechaModifica = DateTime.Now;

                            modificados.Add(actual);
                        }
                    }
                }

                if (nuevos.Count > 0)
                {
                    _db.Set<PersonalSueldosPago>().AddRange(nuevos);
                    await _db.SaveChangesAsync();
                }

                var afectados = nuevos.Concat(modificados).ToList();

                foreach (var pago in afectados)
                {
                    await UpsertMovimientoPagoPersonalAsync(sueldo, pago);
                    await UpsertMovimientoCajaPagoAsync(sueldo, pago);
                    await UpsertGastoPagoAsync(sueldo, pago);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al sincronizar pagos del sueldo {sueldo.Id}.", ex);
            }
        }

        /*
        =========================================================
        PAGO -> CUENTA CORRIENTE PERSONAL
        =========================================================
        */

        private async Task UpsertMovimientoPagoPersonalAsync(PersonalSueldo sueldo, PersonalSueldosPago pago)
        {
            try
            {
                PersonalCuentaCorriente? mov = null;

                if (pago.IdPersonalCc.HasValue && pago.IdPersonalCc.Value > 0)
                {
                    mov = await _db.PersonalCuentaCorrientes
                        .FirstOrDefaultAsync(x => x.Id == pago.IdPersonalCc.Value);
                }

                mov ??= await _db.PersonalCuentaCorrientes
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_PAGO_SUELDO_PERSONAL &&
                        x.IdMov == pago.Id);

                if (pago.Conversion <= 0)
                {
                    if (mov != null)
                    {
                        _db.PersonalCuentaCorrientes.Remove(mov);
                        await _db.SaveChangesAsync();
                    }

                    pago.IdPersonalCc = null;
                    await _db.SaveChangesAsync();
                    return;
                }

                if (mov == null)
                {
                    mov = new PersonalCuentaCorriente
                    {
                        IdPersonal = sueldo.IdPersonal,
                        IdMoneda = pago.IdMoneda,
                        TipoMov = TIPO_MOV_PAGO_SUELDO_PERSONAL,
                        IdMov = pago.Id,
                        Fecha = pago.Fecha,
                        Concepto = $"Pago sueldo {sueldo.Concepto}",
                        Debe = 0,
                        Haber = pago.Conversion,
                        IdUsuarioRegistra = ObtenerUsuarioActual(sueldo),
                        FechaRegistra = DateTime.Now
                    };

                    _db.PersonalCuentaCorrientes.Add(mov);
                    await _db.SaveChangesAsync();

                    pago.IdPersonalCc = mov.Id;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    mov.IdPersonal = sueldo.IdPersonal;
                    mov.IdMoneda = pago.IdMoneda;
                    mov.Fecha = pago.Fecha;
                    mov.Concepto = $"Pago sueldo {sueldo.Concepto}";
                    mov.Debe = 0;
                    mov.Haber = pago.Conversion;
                    mov.IdUsuarioModifica = ObtenerUsuarioActual(sueldo);
                    mov.FechaModifica = DateTime.Now;

                    if (pago.IdPersonalCc != mov.Id)
                        pago.IdPersonalCc = mov.Id;

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al upsertar movimiento de pago {pago.Id}.", ex);
            }
        }

        private async Task EliminarMovimientoPagoPersonalAsync(PersonalSueldosPago pago)
        {
            try
            {
                PersonalCuentaCorriente? mov = null;

                if (pago.IdPersonalCc.HasValue && pago.IdPersonalCc.Value > 0)
                {
                    mov = await _db.PersonalCuentaCorrientes
                        .FirstOrDefaultAsync(x => x.Id == pago.IdPersonalCc.Value);
                }

                mov ??= await _db.PersonalCuentaCorrientes
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_PAGO_SUELDO_PERSONAL &&
                        x.IdMov == pago.Id);

                if (mov != null)
                {
                    _db.PersonalCuentaCorrientes.Remove(mov);
                    await _db.SaveChangesAsync();
                }

                if (pago.IdPersonalCc.HasValue)
                {
                    pago.IdPersonalCc = null;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar movimiento de pago {pago.Id}.", ex);
            }
        }

        /*
        =========================================================
        PAGO -> CAJA
        =========================================================
        */

        private async Task UpsertMovimientoCajaPagoAsync(PersonalSueldo sueldo, PersonalSueldosPago pago)
        {
            try
            {
                Caja? mov = null;

                if (pago.IdCaja.HasValue && pago.IdCaja.Value > 0)
                {
                    mov = await _db.Cajas
                        .FirstOrDefaultAsync(x => x.Id == pago.IdCaja.Value);
                }

                mov ??= await _db.Cajas
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_PAGO_SUELDO_CAJA &&
                        x.IdMov == pago.Id);

                if (pago.Conversion <= 0)
                {
                    if (mov != null)
                    {
                        _db.Cajas.Remove(mov);
                        await _db.SaveChangesAsync();
                    }

                    pago.IdCaja = null;
                    await _db.SaveChangesAsync();
                    return;
                }

                if (mov == null)
                {
                    mov = new Caja
                    {
                        TipoMov = TIPO_MOV_PAGO_SUELDO_CAJA,
                        IdMov = pago.Id,
                        Fecha = pago.Fecha,
                        Concepto = $"Pago sueldo {sueldo.Concepto}",
                        IdMoneda = pago.IdMoneda,
                        IdCuenta = pago.IdCuenta,
                        Ingreso = 0,
                        Egreso = pago.Conversion,
                        Saldo = 0,
                        IdUsuarioRegistra = ObtenerUsuarioActual(sueldo),
                        FechaRegistra = DateTime.Now
                    };

                    _db.Cajas.Add(mov);
                    await _db.SaveChangesAsync();

                    pago.IdCaja = mov.Id;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    mov.Fecha = pago.Fecha;
                    mov.Concepto = $"Pago sueldo {sueldo.Concepto}";
                    mov.IdMoneda = pago.IdMoneda;
                    mov.IdCuenta = pago.IdCuenta;
                    mov.Ingreso = 0;
                    mov.Egreso = pago.Conversion;
                    mov.IdUsuarioModifica = ObtenerUsuarioActual(sueldo);
                    mov.FechaModifica = DateTime.Now;

                    if (pago.IdCaja != mov.Id)
                        pago.IdCaja = mov.Id;

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al upsertar caja del pago {pago.Id}.", ex);
            }
        }

        private async Task EliminarMovimientoCajaPagoAsync(PersonalSueldosPago pago)
        {
            try
            {
                Caja? mov = null;

                if (pago.IdCaja.HasValue && pago.IdCaja.Value > 0)
                {
                    mov = await _db.Cajas
                        .FirstOrDefaultAsync(x => x.Id == pago.IdCaja.Value);
                }

                mov ??= await _db.Cajas
                    .FirstOrDefaultAsync(x =>
                        x.TipoMov == TIPO_MOV_PAGO_SUELDO_CAJA &&
                        x.IdMov == pago.Id);

                if (mov != null)
                {
                    _db.Cajas.Remove(mov);
                    await _db.SaveChangesAsync();
                }

                if (pago.IdCaja.HasValue)
                {
                    pago.IdCaja = null;
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar caja del pago {pago.Id}.", ex);
            }
        }

        /*
        =========================================================
        PAGO -> GASTO
        =========================================================
        */

        private async Task UpsertGastoPagoAsync(PersonalSueldo sueldo, PersonalSueldosPago pago)
        {
            try
            {
                int idCategoriaSueldos = await ObtenerCategoriaSueldosAsync();

                string conceptoGasto = ObtenerConceptoGasto(sueldo, pago);

                var gasto = await _db.Set<Gasto>()
                    .FirstOrDefaultAsync(x =>
                        x.Concepto == conceptoGasto &&
                        x.IdPersonal == sueldo.IdPersonal);

                if (pago.Conversion <= 0)
                {
                    if (gasto != null)
                    {
                        _db.Set<Gasto>().Remove(gasto);
                        await _db.SaveChangesAsync();
                    }

                    return;
                }

                if (gasto == null)
                {
                    gasto = new Gasto
                    {
                        Fecha = pago.Fecha,
                        IdCategoria = idCategoriaSueldos,
                        IdMoneda = pago.IdMoneda,
                        IdCuenta = pago.IdCuenta,
                        IdPersonal = sueldo.IdPersonal,
                        Concepto = conceptoGasto,
                        Importe = pago.Conversion,
                        NotaInterna = sueldo.NotaInterna,
                        IdUsuarioRegistra = ObtenerUsuarioActual(sueldo),
                        FechaRegistra = DateTime.Now
                    };

                    _db.Set<Gasto>().Add(gasto);
                }
                else
                {
                    gasto.Fecha = pago.Fecha;
                    gasto.IdCategoria = idCategoriaSueldos;
                    gasto.IdMoneda = pago.IdMoneda;
                    gasto.IdCuenta = pago.IdCuenta;
                    gasto.IdPersonal = sueldo.IdPersonal;
                    gasto.Concepto = conceptoGasto;
                    gasto.Importe = pago.Conversion;
                    gasto.NotaInterna = sueldo.NotaInterna;
                    gasto.IdUsuarioModifica = ObtenerUsuarioActual(sueldo);
                    gasto.FechaModifica = DateTime.Now;
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al upsertar gasto del pago {pago.Id}.", ex);
            }
        }

        private async Task EliminarGastoPagoAsync(PersonalSueldo sueldo, PersonalSueldosPago pago)
        {
            try
            {
                string conceptoGasto = ObtenerConceptoGasto(sueldo, pago);

                var gasto = await _db.Set<Gasto>()
                    .FirstOrDefaultAsync(x =>
                        x.Concepto == conceptoGasto &&
                        x.IdPersonal == sueldo.IdPersonal);

                if (gasto != null)
                {
                    _db.Set<Gasto>().Remove(gasto);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar gasto del pago {pago.Id}.", ex);
            }
        }

        /*
        =========================================================
        HELPERS
        =========================================================
        */

        private async Task<int> ObtenerCategoriaSueldosAsync()
        {
            var categoria = await _db.Set<GastosCategoria>()
                .FirstOrDefaultAsync(x =>
                    x.Nombre == "SUELDOS" ||
                    x.Nombre == "SUELDO");

            if (categoria == null)
                throw new Exception("No existe la categoría de gasto 'SUELDOS'.");

            return categoria.Id;
        }

        private string ObtenerConceptoGasto(PersonalSueldo sueldo, PersonalSueldosPago pago)
        {
            return $"Pago sueldo #{sueldo.Id} / pago #{pago.Id} - {sueldo.Concepto}";
        }

        private int ObtenerUsuarioActual(PersonalSueldo sueldo)
        {
            if (sueldo.IdUsuarioModifica.HasValue && sueldo.IdUsuarioModifica.Value > 0)
                return sueldo.IdUsuarioModifica.Value;

            return sueldo.IdUsuarioRegistra;
        }

        private void NormalizarSueldo(PersonalSueldo sueldo)
        {
            if (sueldo.ImporteTotal < 0)
                sueldo.ImporteTotal = 0;
        }

        private decimal CalcularConversion(PersonalSueldosPago pago)
        {
            if (pago.Conversion > 0)
                return pago.Conversion;

            var cotizacion = pago.Cotizacion <= 0 ? 1 : pago.Cotizacion;
            return pago.Importe * cotizacion;
        }
    }
}