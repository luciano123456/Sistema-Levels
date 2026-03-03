using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class UbicacionesRepository : IUbicacionesRepository<Ubicacion>
    {
        private readonly SistemaLevelsContext _db;

        public UbicacionesRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        public async Task<bool> Insertar(Ubicacion model)
        {
            _db.Ubicaciones.Add(model);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Actualizar(Ubicacion model)
        {
            var entity = await _db.Ubicaciones
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (entity == null)
                return false;

            entity.Descripcion = model.Descripcion;
            entity.Espacio = model.Espacio;
            entity.Direccion = model.Direccion;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            var entity = await _db.Ubicaciones
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return false;

            _db.Ubicaciones.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Ubicacion?> Obtener(int id)
        {
            return await _db.Ubicaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IQueryable<Ubicacion>> ObtenerTodos()
        {
            return await Task.FromResult(
                _db.Ubicaciones.AsNoTracking()
            );
        }

        public async Task<Ubicacion?> BuscarDuplicado(int? idExcluir, string descripcion)
        {
            var query = _db.Ubicaciones.AsQueryable();

            if (idExcluir.HasValue)
                query = query.Where(x => x.Id != idExcluir.Value);

            return await query
                .FirstOrDefaultAsync(x => x.Descripcion == descripcion);
        }
    }
}