using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PaisesMonedaService : IPaisesMonedaService
    {
        private readonly IPaisesMonedaRepository<PaisesMoneda> _repo;

        public PaisesMonedaService(IPaisesMonedaRepository<PaisesMoneda> repo)
        {
            _repo = repo;
        }

        public async Task<bool> Insertar(PaisesMoneda model)
            => await _repo.Insertar(model);

        public async Task<bool> Actualizar(PaisesMoneda model)
            => await _repo.Actualizar(model);

        public async Task<bool> Eliminar(int id)
            => await _repo.Eliminar(id);

        public async Task<PaisesMoneda?> Obtener(int id)
            => await _repo.Obtener(id);

        public async Task<IQueryable<PaisesMoneda>> ObtenerTodos()
            => await _repo.ObtenerTodos();

        public async Task<bool> ActualizarMasivo(Dictionary<int, decimal> monedas)
        {
            return await _repo.ActualizarMasivo(monedas);
        }
    }
}
