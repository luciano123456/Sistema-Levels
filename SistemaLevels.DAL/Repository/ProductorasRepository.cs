using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class ProductorasRepository : IProductorasRepository
    {
        private readonly SistemaLevelsContext _dbcontext;

        public ProductorasRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        /* =====================================================
           SYNC CLIENTES (MISMA LÓGICA QUE CLIENTES)
        ===================================================== */

        private async Task SincronizarClientes(int idProductora, List<int> clientesIds)
        {
            clientesIds ??= new();

            clientesIds = clientesIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            // SOLO MANUALES
            var actuales = await _dbcontext.ClientesProductoras
                .Where(x =>
                    x.IdProductora == idProductora &&
                    x.OrigenAsignacion == 1)
                .ToListAsync();

            _dbcontext.ClientesProductoras.RemoveRange(actuales);

            foreach (var idCliente in clientesIds)
            {
                _dbcontext.ClientesProductoras.Add(new ClientesProductora
                {
                    IdCliente = idCliente,
                    IdProductora = idProductora,
                    OrigenAsignacion = 1,
                    FechaRegistro = DateTime.Now
                });
            }
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

                await SincronizarClientes(model.Id, clientesIds);

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
           ACTUALIZAR
        ===================================================== */

        public async Task<bool> Actualizar(Productora model, List<int> clientesIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbcontext.Productoras
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null)
                    return false;

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

                await SincronizarClientes(entity.Id, clientesIds);

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
           ELIMINAR
        ===================================================== */

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbcontext.Productoras
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) return false;

                var relaciones = await _dbcontext.ClientesProductoras
                    .Where(x => x.IdProductora == id)
                    .ToListAsync();

                _dbcontext.ClientesProductoras.RemoveRange(relaciones);

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
                .AsNoTracking()
                .Include(x => x.ClientesProductoras)
                    .ThenInclude(cp => cp.IdClienteNavigation)
                .Include(x => x.IdpaisNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdProvinciaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /* =====================================================
           LISTA
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

        /* =====================================================
           CLIENTES AUTOMÁTICOS
        ===================================================== */

        public async Task<List<int>> ObtenerClientesAsociadosAutomaticos(int idProductora)
        {
            return await _dbcontext.ClientesProductoras
                .Where(x =>
                    x.IdProductora == idProductora &&
                    x.OrigenAsignacion == 2)
                .Select(x => x.IdCliente)
                .ToListAsync();
        }

        public async Task<Productora?> BuscarDuplicado(
    int? id,
    string? nombre,
    string? dni,
    string? cuit)
        {
            return await _dbcontext.Productoras
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    (id == null || x.Id != id) &&
                    (
                        (!string.IsNullOrEmpty(nombre) && x.Nombre == nombre) ||
                        (!string.IsNullOrEmpty(dni) && x.Dni == dni) ||
                        (!string.IsNullOrEmpty(cuit) && x.NumeroDocumento == cuit)
                    ));
        }
    }
}