using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class VentasEstadosService : IVentasEstadosService
    {

        private readonly IVentasEstadosRepository<VentasEstado> _contactRepo;

        public VentasEstadosService(IVentasEstadosRepository<VentasEstado> contactRepo)
        {
            _contactRepo = contactRepo;
        }
        public async Task<bool> Actualizar(VentasEstado model)
        {
            return await _contactRepo.Actualizar(model);
        }

        public async Task<bool> Eliminar(int id)
        {
            return await _contactRepo.Eliminar(id);
        }

        public async Task<bool> Insertar(VentasEstado model)
        {
            return await _contactRepo.Insertar(model);
        }

        public async Task<VentasEstado> Obtener(int id)
        {
            return await _contactRepo.Obtener(id);
        }


        public async Task<IQueryable<VentasEstado>> ObtenerTodos()
        {
            return await _contactRepo.ObtenerTodos();
        }



    }
}
