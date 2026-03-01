using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class ClientesRepository : IClientesRepository<Cliente>
    {
        private readonly SistemaLevelsContext _db;

        public ClientesRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        /* =====================================================
           INSERTAR
        ===================================================== */

        public async Task<bool> Insertar(
            Cliente model,
            List<int> productorasIds)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                _db.Clientes.Add(model);
                await _db.SaveChangesAsync();

                await SincronizarProductoras(
                    model.Id,
                    productorasIds,
                    2 // origen cliente
                );

                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        /* =====================================================
           ACTUALIZAR
        ===================================================== */

        public async Task<bool> Actualizar(
            Cliente model,
            List<int> productorasIds)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var entity = await _db.Clientes
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null)
                    return false;

                entity.Nombre = model.Nombre;
                entity.Telefono = model.Telefono;
                entity.TelefonoAlternativo = model.TelefonoAlternativo;
                entity.Email = model.Email;
                entity.Dni = model.Dni;

                entity.IdPais = model.IdPais;
                entity.IdProvincia = model.IdProvincia;

                entity.IdTipoDocumento = model.IdTipoDocumento;
                entity.NumeroDocumento = model.NumeroDocumento;
                entity.IdCondicionIva = model.IdCondicionIva;

                entity.Direccion = model.Direccion;
                entity.Localidad = model.Localidad;
                entity.EntreCalles = model.EntreCalles;
                entity.CodigoPostal = model.CodigoPostal;

                entity.AsociacionAutomatica = model.AsociacionAutomatica;

                entity.IdUsuarioModifica = model.IdUsuarioModifica;
                entity.FechaModifica = DateTime.Now;

                await _db.SaveChangesAsync();

                await SincronizarProductoras(
                    entity.Id,
                    productorasIds,
                    2
                );

                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        /* =====================================================
           BUSCAR DUPLICADO ⭐
        ===================================================== */

        public async Task<Cliente?> BuscarDuplicado(
            int? idExcluir,
            string? nombre,
            string? numeroDocumento,
            string? dni)
        {
            var query = _db.Clientes.AsQueryable();

            if (idExcluir.HasValue)
                query = query.Where(x => x.Id != idExcluir.Value);

            if (!string.IsNullOrWhiteSpace(dni))
            {
                var dup = await query
                    .FirstOrDefaultAsync(x => x.Dni == dni);

                if (dup != null)
                    return dup;
            }

            if (!string.IsNullOrWhiteSpace(numeroDocumento))
            {
                var dup = await query
                    .FirstOrDefaultAsync(x => x.NumeroDocumento == numeroDocumento);

                if (dup != null)
                    return dup;
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var dup = await query
                    .FirstOrDefaultAsync(x => x.Nombre == nombre);

                if (dup != null)
                    return dup;
            }

            return null;
        }

        /* =====================================================
           SINCRONIZADOR PRODUCTORAS
        ===================================================== */

        private async Task SincronizarProductoras(
            int idCliente,
            List<int> nuevasIds,
            byte origenAsignacion)
        {
            nuevasIds ??= new List<int>();

            nuevasIds = nuevasIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var actuales = await _db.ClientesProductoras
                .Where(x =>
                    x.IdCliente == idCliente &&
                    x.OrigenAsignacion == origenAsignacion)
                .ToListAsync();

            _db.ClientesProductoras.RemoveRange(actuales);

            foreach (var idProd in nuevasIds)
            {
                _db.ClientesProductoras.Add(new ClientesProductora
                {
                    IdCliente = idCliente,
                    IdProductora = idProd,
                    OrigenAsignacion = origenAsignacion,
                    FechaRegistro = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();
        }

        /* =====================================================
           ELIMINAR
        ===================================================== */

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var cliente = await _db.Clientes
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (cliente == null)
                    return false;

                var relaciones = await _db.ClientesProductoras
                    .Where(x => x.IdCliente == id)
                    .ToListAsync();

                _db.ClientesProductoras.RemoveRange(relaciones);
                _db.Clientes.Remove(cliente);

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

        /* =====================================================
           OBTENER
        ===================================================== */

        public async Task<Cliente?> Obtener(int id)
        {
            return await _db.Clientes
                .AsNoTracking()
                .Include(x => x.ClientesProductoras)
                    .ThenInclude(cp => cp.IdProductoraNavigation)
                .Include(x => x.IdPaisNavigation)
                .Include(x => x.IdProvinciaNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /* =====================================================
           LISTA
        ===================================================== */

        public async Task<IQueryable<Cliente>> ObtenerTodos()
        {
            var query = _db.Clientes
                .AsNoTracking()
                .Include(x => x.ClientesProductoras)
                    .ThenInclude(cp => cp.IdProductoraNavigation)
                .Include(x => x.IdPaisNavigation)
                .Include(x => x.IdProvinciaNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation);

            return await Task.FromResult(query);
        }
    }
}