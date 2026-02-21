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

        public async Task<bool> Insertar(Productora model, List<int> clientesIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                _dbcontext.Productoras.Add(model);
                await _dbcontext.SaveChangesAsync();

                clientesIds ??= new List<int>();

                foreach (var idCliente in clientesIds.Distinct())
                {
                    _dbcontext.ProductorasClientesAsignados.Add(new ProductorasClientesAsignado
                    {
                        IdProductora = model.Id,
                        IdCliente = idCliente
                    });
                }

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

        public async Task<bool> Actualizar(Productora model, List<int> clientesIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbcontext.Productoras
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                // ===== CAMPOS PRINCIPALES =====
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

                // ===== FECHA SEGURA =====
                entity.IdUsuarioModifica = model.IdUsuarioModifica;

                if (model.FechaModifica.HasValue && model.FechaModifica.Value >= new DateTime(1753, 1, 1))
                    entity.FechaModifica = model.FechaModifica;
                else
                    entity.FechaModifica = DateTime.Now;

                await _dbcontext.SaveChangesAsync();

                // ===== CLIENTES: REEMPLAZO TOTAL =====
                clientesIds ??= new List<int>();
                clientesIds = clientesIds.Distinct().ToList();

                var actuales = await _dbcontext.ProductorasClientesAsignados
                    .Where(x => x.IdProductora == model.Id)
                    .ToListAsync();

                _dbcontext.ProductorasClientesAsignados.RemoveRange(actuales);
                await _dbcontext.SaveChangesAsync();

                foreach (var idCliente in clientesIds)
                {
                    _dbcontext.ProductorasClientesAsignados.Add(new ProductorasClientesAsignado
                    {
                        IdProductora = model.Id,
                        IdCliente = idCliente
                    });
                }

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


        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var entity = await _dbcontext.Productoras
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) return false;

                _dbcontext.Productoras.Remove(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

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
