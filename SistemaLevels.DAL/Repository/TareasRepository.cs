using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class TareasRepository : ITareasRepository<Tarea>
    {
        private readonly SistemaLevelsContext _dbcontext;

        public TareasRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        public async Task<bool> Insertar(Tarea model)
        {
            try
            {
                _dbcontext.Tareas.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(Tarea model)
        {
            try
            {
                var entity = await _dbcontext.Tareas
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.Fecha = model.Fecha;
                entity.FechaLimite = model.FechaLimite;
                entity.IdPersonal = model.IdPersonal;
                entity.Descripcion = model.Descripcion;
                entity.IdEstado = model.IdEstado;

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
                var entity = await _dbcontext.Tareas
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) return false;

                _dbcontext.Tareas.Remove(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Tarea?> Obtener(int id)
        {
            return await _dbcontext.Tareas
                .Include(x => x.IdPersonalNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IQueryable<Tarea>> ObtenerTodos()
        {
            IQueryable<Tarea> query = _dbcontext.Tareas
                .Include(x => x.IdPersonalNavigation)
                .Include(x => x.IdEstadoNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation);

            return await Task.FromResult(query);
        }
    }
}
