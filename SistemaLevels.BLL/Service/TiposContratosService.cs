using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class TiposContratosService : ITiposContratosService
    {

        private readonly ITiposContratosRepository<TiposContrato> _contactRepo;

        public TiposContratosService(ITiposContratosRepository<TiposContrato> contactRepo)
        {
            _contactRepo = contactRepo;
        }
        public async Task<bool> Actualizar(TiposContrato model)
        {
            return await _contactRepo.Actualizar(model);
        }

        public async Task<bool> Eliminar(int id)
        {
            return await _contactRepo.Eliminar(id);
        }

        public async Task<bool> Insertar(TiposContrato model)
        {
            return await _contactRepo.Insertar(model);
        }

        public async Task<TiposContrato> Obtener(int id)
        {
            return await _contactRepo.Obtener(id);
        }


        public async Task<IQueryable<TiposContrato>> ObtenerTodos()
        {
            return await _contactRepo.ObtenerTodos();
        }



    }
}
