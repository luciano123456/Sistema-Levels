using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;
using System.Linq.Expressions;

namespace SistemaLevels.DAL.Repository
{
    public class ClientesRepository : IClientesRepository<Cliente>
    {
        private readonly SistemaLevelsContext _dbcontext;

        public ClientesRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        /* =====================================================
           INSERTAR
        ===================================================== */

        public async Task<bool> Insertar(Cliente model, List<int> productorasIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                _dbcontext.Clientes.Add(model);
                await _dbcontext.SaveChangesAsync();

                await SincronizarProductoras(model.Id, productorasIds);

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

        public async Task<bool> Actualizar(Cliente model, List<int> productorasIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbcontext.Clientes
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.Nombre = model.Nombre;
                entity.Telefono = model.Telefono;
                entity.TelefonoAlternativo = model.TelefonoAlternativo;

                entity.Dni = model.Dni;
                entity.IdTipoDocumento = model.IdTipoDocumento;
                entity.NumeroDocumento = model.NumeroDocumento;

                entity.Email = model.Email;

                entity.IdPais = model.IdPais;
                entity.IdProvincia = model.IdProvincia;

                entity.Localidad = model.Localidad;
                entity.EntreCalles = model.EntreCalles;
                entity.Direccion = model.Direccion;
                entity.CodigoPostal = model.CodigoPostal;

                entity.IdCondicionIva = model.IdCondicionIva;

                entity.AsociacionAutomatica = model.AsociacionAutomatica;

                entity.IdUsuarioModifica = model.IdUsuarioModifica;

                // ✅ fecha segura (si te llega rara)
                if (model.FechaModifica.HasValue && model.FechaModifica.Value >= new DateTime(1753, 1, 1))
                    entity.FechaModifica = model.FechaModifica;
                else
                    entity.FechaModifica = DateTime.Now;

                await _dbcontext.SaveChangesAsync();

                await SincronizarProductoras(entity.Id, productorasIds);

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
                var entity = await _dbcontext.Clientes
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) return false;

                // ✅ borrar relaciones (tabla puente)
                var relaciones = await _dbcontext.ClientesProductorasAsignadas
                    .Where(x => x.IdCliente == id)
                    .ToListAsync();

                _dbcontext.ClientesProductorasAsignadas.RemoveRange(relaciones);

                _dbcontext.Clientes.Remove(entity);

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
           OBTENER (para EditarInfo)
        ===================================================== */

        public async Task<Cliente?> Obtener(int id)
        {
            try
            {
                return await _dbcontext.Clientes
                    // ✅ tabla puente + navegación a Productora (para armar nombres si querés)
                    .Include(x => x.ClientesProductorasAsignada)
                        .ThenInclude(x => x.IdNavigation)

                    // ✅ combos / descriptivos
                    .Include(x => x.IdPaisNavigation)
                    .Include(x => x.IdProvinciaNavigation)
                    .Include(x => x.IdTipoDocumentoNavigation)
                    .Include(x => x.IdCondicionIvaNavigation)

                    // ✅ auditoría
                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation)

                    .FirstOrDefaultAsync(x => x.Id == id);
            } catch (Exception ex)
            {
                return null;
            }
        }

        /* =====================================================
           OBTENER TODOS (grilla)
        ===================================================== */

        public async Task<IQueryable<Cliente>> ObtenerTodos()
        {
            IQueryable<Cliente> query = _dbcontext.Clientes
                 // ✅ tabla puente + navegación a Productora (para grilla: "N asignadas" o nombre)
                 .Include(x => x.ClientesProductorasAsignada)
        .ThenInclude(x => x.IdNavigation)
    .Include(x => x.IdPaisNavigation)
    .Include(x => x.IdProvinciaNavigation)
    .Include(x => x.IdTipoDocumentoNavigation)
    .Include(x => x.IdCondicionIvaNavigation)
    .Include(x => x.IdUsuarioRegistraNavigation)
    .Include(x => x.IdUsuarioModificaNavigation);

            return await Task.FromResult(query);
        }

        /* =====================================================
           HELPER: SINCRONIZAR PRODUCTORAS (REEMPLAZO TOTAL)
        ===================================================== */

        private async Task SincronizarProductoras(int idCliente, List<int> productorasIds)
        {
            try
            {
                productorasIds ??= new List<int>();

                // ✅ limpiar ids inválidos
                productorasIds = productorasIds
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                // ✅ traer SOLO productoras existentes (FK SAFE)
                var productorasValidas = await _dbcontext.Productoras
                    .Where(p => productorasIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                // ===== borrar actuales =====
                var actuales = await _dbcontext.ClientesProductorasAsignadas
                    .Where(x => x.IdCliente == idCliente)
                    .ToListAsync();

                _dbcontext.ClientesProductorasAsignadas.RemoveRange(actuales);

                // ===== insertar nuevas =====
                foreach (var idProd in productorasValidas)
                {
                    _dbcontext.ClientesProductorasAsignadas.Add(
                        new ClientesProductorasAsignada
                        {
                            IdCliente = idCliente,
                            IdProductora = idProd
                        });
                }

                await _dbcontext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Manejo de errores opcional, pero no se debe retornar null en un método async Task
                // Puedes registrar el error o manejarlo según sea necesario
            }
        }
    }
}