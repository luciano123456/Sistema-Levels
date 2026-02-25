using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class ProductorasRepository : IProductorasRepository<Productora>
    {
        private readonly SistemaLevelsContext _dbcontext;

        public ProductorasRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        /* =====================================================
           SYNC CLIENTES ↔ PRODUCTORAS
        ===================================================== */

        private async Task SyncClientesProductorasAsignadas(
            int idProductora,
            List<int> clientesIds)
        {
            clientesIds ??= new List<int>();
            clientesIds = clientesIds.Distinct().ToList();

            var actuales = await _dbcontext.ClientesProductorasAsignadas
                .Where(x => x.IdProductora == idProductora)
                .ToListAsync();

            _dbcontext.ClientesProductorasAsignadas.RemoveRange(actuales);
            await _dbcontext.SaveChangesAsync();

            foreach (var idCliente in clientesIds)
            {
                _dbcontext.ClientesProductorasAsignadas.Add(
                    new ClientesProductorasAsignada
                    {
                        IdProductora = idProductora,
                        IdCliente = idCliente
                    });
            }

            await _dbcontext.SaveChangesAsync();
        }

        /* =====================================================
           INSERTAR
        ===================================================== */

        public async Task<bool> Insertar(Productora model, List<int> clientesIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                _dbcontext.Productoras.Add(model);
                await _dbcontext.SaveChangesAsync();

                await SyncClientesProductorasAsignadas(
                    model.Id,
                    clientesIds
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

        public async Task<bool> Actualizar(Productora model, List<int> clientesIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbcontext.Productoras
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.Nombre = model.Nombre;
                entity.Telefono = model.Telefono;
                entity.TelefonoAlternativo = model.TelefonoAlternativo;
                entity.Dni = model.Dni;
                entity.Idpais = model.Idpais;
                entity.IdTipoDocumento = model.IdTipoDocumento;
                entity.NumeroDocumento = model.NumeroDocumento;
                entity.IdCondicionIva = model.IdCondicionIva;
                entity.Email = model.Email;
                entity.IdProvincia = model.IdProvincia;
                entity.Localidad = model.Localidad;
                entity.EntreCalles = model.EntreCalles;
                entity.Direccion = model.Direccion;
                entity.CodigoPostal = model.CodigoPostal;
                entity.AsociacionAutomatica = model.AsociacionAutomatica;

                entity.IdUsuarioModifica = model.IdUsuarioModifica;
                entity.FechaModifica = DateTime.Now;

                await _dbcontext.SaveChangesAsync();

                await SyncClientesProductorasAsignadas(
                    entity.Id,
                    clientesIds
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
           ELIMINAR
        ===================================================== */

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbcontext.Productoras
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    return false;

                // 🔥 SOLO BORRAMOS RELACIONES MANY-TO-MANY
                var relaciones = await _dbcontext.ClientesProductorasAsignadas
                    .Where(x => x.IdProductora == id)
                    .ToListAsync();

                _dbcontext.ClientesProductorasAsignadas.RemoveRange(relaciones);

                await _dbcontext.SaveChangesAsync();

                _dbcontext.Productoras.Remove(entity);

                await _dbcontext.SaveChangesAsync();

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

        public async Task<Productora?> Obtener(int id)
        {
            return await _dbcontext.Productoras
                .Include(x => x.IdpaisNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdProvinciaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .Include(x => x.ProductorasClientesAsignados)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /* =====================================================
           OBTENER TODOS
        ===================================================== */

        public async Task<IQueryable<Productora>> ObtenerTodos()
        {
            IQueryable<Productora> query = _dbcontext.Productoras
                .Include(x => x.IdpaisNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdProvinciaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation);

            return await Task.FromResult(query);
        }
    }
}