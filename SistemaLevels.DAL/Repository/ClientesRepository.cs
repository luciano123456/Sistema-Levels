using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class ClientesRepository : IClientesRepository<Cliente>
    {
        private readonly SistemaLevelsContext _dbcontext;

        public ClientesRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        public async Task<bool> Insertar(Cliente model)
        {
            try
            {
                _dbcontext.Clientes.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(Cliente model)
        {
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

                entity.IdProductora = model.IdProductora;
                entity.IdPais = model.IdPais;
                entity.IdProvincia = model.IdProvincia;

                entity.Localidad = model.Localidad;
                entity.EntreCalles = model.EntreCalles;
                entity.Direccion = model.Direccion;
                entity.CodigoPostal = model.CodigoPostal;

                entity.IdCondicionIva = model.IdCondicionIva;

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
                var entity = await _dbcontext.Clientes
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) return false;

                _dbcontext.Clientes.Remove(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Cliente?> Obtener(int id)
        {
            return await _dbcontext.Clientes
                .Include(x => x.IdProductoraNavigation)
                .Include(x => x.IdPaisNavigation)
                .Include(x => x.IdProvinciaNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IQueryable<Cliente>> ObtenerTodos()
        {
            IQueryable<Cliente> query = _dbcontext.Clientes
                .Include(x => x.IdProductoraNavigation)
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
