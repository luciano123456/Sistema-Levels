using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PaisesCondicionesIvaService : IPaisesCondicionesIvaService
    {

        private readonly IPaisesCondicionesIvaRepository<PaisesCondicionesIva> _contactRepo;

        public PaisesCondicionesIvaService(IPaisesCondicionesIvaRepository<PaisesCondicionesIva> contactRepo)
        {
            _contactRepo = contactRepo;
        }
        public async Task<bool> Actualizar(PaisesCondicionesIva model)
        {
            return await _contactRepo.Actualizar(model);
        }

        public async Task<bool> Eliminar(int id)
        {
            return await _contactRepo.Eliminar(id);
        }

        public async Task<bool> Insertar(PaisesCondicionesIva model)
        {
            return await _contactRepo.Insertar(model);
        }

        public async Task<PaisesCondicionesIva> Obtener(int id)
        {
            return await _contactRepo.Obtener(id);
        }

        public async Task<IQueryable<PaisesCondicionesIva>> ObtenerTodos()
        {
            return await _contactRepo.ObtenerTodos();
        }



    }
}
