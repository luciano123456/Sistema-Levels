using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PaisesProvinciaService : IPaisesProvinciaService
    {

        private readonly IPaisesProvinciaRepository<PaisesProvincia> _contactRepo;

        public PaisesProvinciaService(IPaisesProvinciaRepository<PaisesProvincia> contactRepo)
        {
            _contactRepo = contactRepo;
        }
        public async Task<bool> Actualizar(PaisesProvincia model)
        {
            return await _contactRepo.Actualizar(model);
        }

        public async Task<bool> Eliminar(int id)
        {
            return await _contactRepo.Eliminar(id);
        }

        public async Task<bool> Insertar(PaisesProvincia model)
        {
            return await _contactRepo.Insertar(model);
        }

        public async Task<PaisesProvincia> Obtener(int id)
        {
            return await _contactRepo.Obtener(id);
        }

        public async Task<IQueryable<PaisesProvincia>> ObtenerTodos()
        {
            return await _contactRepo.ObtenerTodos();
        }



    }
}
