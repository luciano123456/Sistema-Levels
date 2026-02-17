using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaLevels.BLL.Service
{
    public class GastosService : IGastosService
    {
        private readonly IGastosRepository<Gasto> _repo;

        public GastosService(IGastosRepository<Gasto> repo)
        {
            _repo = repo;
        }

        public async Task<bool> Insertar(Gasto model)
            => await _repo.Insertar(model);

        public async Task<bool> Actualizar(Gasto model)
            => await _repo.Actualizar(model);

        public async Task<bool> Eliminar(int id)
            => await _repo.Eliminar(id);

        public async Task<Gasto?> Obtener(int id)
            => await _repo.Obtener(id);

        public async Task<IQueryable<Gasto>> ObtenerTodos()
            => await _repo.ObtenerTodos();

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
            return await _repo.ListarFiltrado(
                fechaDesde,
                fechaHasta,
                idCategoria,
                idMoneda,
                idCuenta,
                idPersonal,
                concepto,
                importeMin
            );
        }
    }
}
