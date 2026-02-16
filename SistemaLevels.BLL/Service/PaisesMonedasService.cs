using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PaisesMonedaService : IPaisesMonedaService
    {

        private readonly IPaisesMonedaRepository<PaisesMoneda> _contactRepo;

        public PaisesMonedaService(IPaisesMonedaRepository<PaisesMoneda> contactRepo)
        {
            _contactRepo = contactRepo;
        }
        public async Task<bool> Actualizar(PaisesMoneda model)
        {
            return await _contactRepo.Actualizar(model);
        }

        public async Task<bool> Eliminar(int id)
        {
            return await _contactRepo.Eliminar(id);
        }

        public async Task<bool> Insertar(PaisesMoneda model)
        {
            return await _contactRepo.Insertar(model);
        }

        public async Task<PaisesMoneda> Obtener(int id)
        {
            return await _contactRepo.Obtener(id);
        }

        public async Task<IQueryable<PaisesMoneda>> ObtenerTodos()
        {
            return await _contactRepo.ObtenerTodos();
        }



    }
}
