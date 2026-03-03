using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class TiposComisionesService : ITiposComisionesService
    {

        private readonly ITiposComisionesRepository<TiposComision> _contactRepo;

        public TiposComisionesService(ITiposComisionesRepository<TiposComision> contactRepo)
        {
            _contactRepo = contactRepo;
        }
        public async Task<bool> Actualizar(TiposComision model)
        {
            return await _contactRepo.Actualizar(model);
        }

        public async Task<bool> Eliminar(int id)
        {
            return await _contactRepo.Eliminar(id);
        }

        public async Task<bool> Insertar(TiposComision model)
        {
            return await _contactRepo.Insertar(model);
        }

        public async Task<TiposComision> Obtener(int id)
        {
            return await _contactRepo.Obtener(id);
        }


        public async Task<IQueryable<TiposComision>> ObtenerTodos()
        {
            return await _contactRepo.ObtenerTodos();
        }



    }
}
