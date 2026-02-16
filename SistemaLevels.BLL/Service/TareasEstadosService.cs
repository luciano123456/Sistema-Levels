using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class TareasEstadosService : ITareasEstadosService
    {

        private readonly ITareasEstadosRepository<TareasEstado> _contactRepo;

        public TareasEstadosService(ITareasEstadosRepository<TareasEstado> contactRepo)
        {
            _contactRepo = contactRepo;
        }
        public async Task<bool> Actualizar(TareasEstado model)
        {
            return await _contactRepo.Actualizar(model);
        }

        public async Task<bool> Eliminar(int id)
        {
            return await _contactRepo.Eliminar(id);
        }

        public async Task<bool> Insertar(TareasEstado model)
        {
            return await _contactRepo.Insertar(model);
        }

        public async Task<TareasEstado> Obtener(int id)
        {
            return await _contactRepo.Obtener(id);
        }


        public async Task<IQueryable<TareasEstado>> ObtenerTodos()
        {
            return await _contactRepo.ObtenerTodos();
        }



    }
}
