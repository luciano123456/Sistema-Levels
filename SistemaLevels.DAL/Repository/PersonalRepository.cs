using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class PersonalRepository : IPersonalRepository<Personal>
    {
        private readonly SistemaLevelsContext _dbcontext;

        public PersonalRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        public async Task<bool> Insertar(Personal model)
        {
            try
            {
                _dbcontext.Personals.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(Personal model)
        {
            try
            {
                var entity = await _dbcontext.Personals
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.Nombre = model.Nombre;
                entity.Dni = model.Dni;
                entity.IdPais = model.IdPais;
                entity.IdTipoDocumento = model.IdTipoDocumento;
                entity.NumeroDocumento = model.NumeroDocumento;
                entity.Direccion = model.Direccion;
                entity.Telefono = model.Telefono;
                entity.Email = model.Email;
                entity.IdCondicionIva = model.IdCondicionIva;

                // ✅ IMPORTANTE: evitar SqlDateTime overflow (0001-01-01)
                // Si tu columna en DB es datetime, no acepta < 1753-01-01
                if (model.FechaNacimiento.HasValue && model.FechaNacimiento.Value >= new DateTime(1753, 1, 1))
                    entity.FechaNacimiento = model.FechaNacimiento;
                else
                    entity.FechaNacimiento = null;

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
                var personal = await _dbcontext.Personals.FindAsync(id);
                if (personal == null) return false;

                _dbcontext.Personals.Remove(personal);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Personal?> Obtener(int id)
        {
            try
            {
                return await _dbcontext.Personals
                    .Include(x => x.IdPaisNavigation)
                    .Include(x => x.IdTipoDocumentoNavigation)
                    .Include(x => x.IdCondicionIvaNavigation)
                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IQueryable<Personal>> ObtenerTodos()
        {
            var query = _dbcontext.Personals
                .Include(x => x.IdPaisNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .AsQueryable();

            return await Task.FromResult(query);
        }
    }
}
