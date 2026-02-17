using SistemaLevels.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IGastosRepository<TEntity> where TEntity : class
    {
        Task<bool> Insertar(Gasto model);
        Task<bool> Actualizar(Gasto model);
        Task<bool> Eliminar(int id);

        Task<Gasto?> Obtener(int id);

        Task<IQueryable<Gasto>> ObtenerTodos();

        Task<IQueryable<Gasto>> ListarFiltrado(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idCategoria,
            int? idMoneda,
            int? idCuenta,
            int? idPersonal,
            string? concepto,
            decimal? importeMin
        );
    }
}
