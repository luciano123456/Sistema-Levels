using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public class RepresentantesRepository : IRepresentantesRepository<Representante>
    {
        private readonly SistemaLevelsContext _dbcontext;

        public RepresentantesRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        public async Task<bool> Insertar(Representante model)
        {
            try
            {
                _dbcontext.Representantes.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(Representante model)
        {
            try
            {
                // Traemos el existente para NO pisar auditoría de registro
                var entity = await _dbcontext.Representantes
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                // Campos editables
                entity.Nombre = model.Nombre;
                entity.Dni = model.Dni;
                entity.IdPais = model.IdPais;
                entity.IdTipoDocumento = model.IdTipoDocumento;
                entity.NumeroDocumento = model.NumeroDocumento;
                entity.Direccion = model.Direccion;
                entity.Telefono = model.Telefono;
                entity.Email = model.Email;

                // Auditoría modificación (solo estos)
                entity.IdUsuarioModifica = model.IdUsuarioModifica;
                entity.FechaModifica = model.FechaModifica;

                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var entity = await _dbcontext.Representantes
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) return false;

                _dbcontext.Representantes.Remove(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Representante?> Obtener(int id)
        {
            try
            {
                // Incluimos todo lo que se necesita para auditoría y combos
                var entity = await _dbcontext.Representantes
                    .Include(x => x.IdPaisNavigation)
                    .Include(x => x.IdTipoDocumentoNavigation)
                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);

                return entity;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IQueryable<Representante>> ObtenerTodos()
        {
            try
            {
                // IQueryable + Includes (igual estilo que tu UsuariosRepository)
                IQueryable<Representante> query = _dbcontext.Representantes
                    .Include(x => x.IdPaisNavigation)
                    .Include(x => x.IdTipoDocumentoNavigation)
                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation);

                return await Task.FromResult(query);
            }
            catch (Exception)
            {
                return null!;
            }
        }
    }
}
