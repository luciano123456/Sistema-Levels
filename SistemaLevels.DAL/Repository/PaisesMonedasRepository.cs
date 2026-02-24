using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class PaisesMonedaRepository : IPaisesMonedaRepository<PaisesMoneda>
    {
        private readonly SistemaLevelsContext _dbcontext;

        public PaisesMonedaRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        public async Task<bool> Insertar(PaisesMoneda model)
        {
            try
            {
                _dbcontext.PaisesMonedas.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(PaisesMoneda model)
        {
            try
            {
                var entity = await _dbcontext.PaisesMonedas
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.IdPais = model.IdPais;
                entity.Nombre = model.Nombre;
                entity.Cotizacion = model.Cotizacion;

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
                var entity = await _dbcontext.PaisesMonedas
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) return false;

                _dbcontext.PaisesMonedas.Remove(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PaisesMoneda?> Obtener(int id)
        {
            return await _dbcontext.PaisesMonedas
                .Include(x => x.IdPaisNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IQueryable<PaisesMoneda>> ObtenerTodos()
        {
            IQueryable<PaisesMoneda> query = _dbcontext.PaisesMonedas
                .Include(x => x.IdPaisNavigation);

            return await Task.FromResult(query);
        }

        public async Task<bool> ActualizarMasivo(Dictionary<int, decimal> monedas)
        {
            try
            {
                var ids = monedas.Keys.ToList();

                var entidades = await _dbcontext.PaisesMonedas
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync();

                if (entidades.Count == 0)
                    return false;

                foreach (var entity in entidades)
                {
                    if (monedas.TryGetValue(entity.Id, out decimal nuevaCotizacion))
                    {
                        entity.Cotizacion = nuevaCotizacion;
                    }
                }

                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
