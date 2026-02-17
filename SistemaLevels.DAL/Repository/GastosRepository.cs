using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public class GastosRepository : IGastosRepository<Gasto>
    {
        private readonly SistemaLevelsContext _dbcontext;

        public GastosRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        public async Task<bool> Insertar(Gasto model)
        {
            try
            {
                _dbcontext.Gastos.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(Gasto model)
        {
            try
            {
                var entity = await _dbcontext.Gastos
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.Fecha = model.Fecha;
                entity.IdCategoria = model.IdCategoria;
                entity.IdMoneda = model.IdMoneda;
                entity.IdCuenta = model.IdCuenta;
                entity.IdPersonal = model.IdPersonal;
                entity.Concepto = model.Concepto;
                entity.Importe = model.Importe;
                entity.NotaInterna = model.NotaInterna;

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
                var entity = await _dbcontext.Gastos
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) return false;

                _dbcontext.Gastos.Remove(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Gasto?> Obtener(int id)
        {
            return await _dbcontext.Gastos
                .Include(x => x.IdCategoriaNavigation)
                .Include(x => x.IdCuentaNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdPersonalNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IQueryable<Gasto>> ObtenerTodos()
        {
            IQueryable<Gasto> query = _dbcontext.Gastos
                .Include(x => x.IdCategoriaNavigation)
                .Include(x => x.IdCuentaNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdPersonalNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation);

            return await Task.FromResult(query);
        }

        public async Task<IQueryable<Gasto>> ListarFiltrado(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idCategoria,
            int? idMoneda,
            int? idCuenta,
            int? idPersonal,
            string? concepto,
            decimal? importeMin
        )
        {
            IQueryable<Gasto> query = _dbcontext.Gastos
                .Include(x => x.IdCategoriaNavigation)
                .Include(x => x.IdCuentaNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdPersonalNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation);

            if (fechaDesde.HasValue)
                query = query.Where(x => x.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(x => x.Fecha <= fechaHasta.Value);

            if (idCategoria.HasValue)
                query = query.Where(x => x.IdCategoria == idCategoria.Value);

            if (idMoneda.HasValue)
                query = query.Where(x => x.IdMoneda == idMoneda.Value);

            if (idCuenta.HasValue)
                query = query.Where(x => x.IdCuenta == idCuenta.Value);

            if (idPersonal.HasValue)
                query = query.Where(x => x.IdPersonal == idPersonal.Value);

            if (!string.IsNullOrWhiteSpace(concepto))
                query = query.Where(x => x.Concepto.Contains(concepto));

            if (importeMin.HasValue)
                query = query.Where(x => x.Importe >= importeMin.Value);

            return await Task.FromResult(query);
        }
    }
}
