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

        public async Task<bool> Insertar(Productora model)
        {
            try
            {
                _dbcontext.Productoras.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(Productora model)
        {
            try
            {
                var entity = await _dbcontext.Productoras
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.Nombre = model.Nombre;
                entity.NombreRepresentante = model.NombreRepresentante;
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

                entity.IdUsuarioModifica = model.IdUsuarioModifica;
                entity.FechaModifica = model.FechaModifica;

                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
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
