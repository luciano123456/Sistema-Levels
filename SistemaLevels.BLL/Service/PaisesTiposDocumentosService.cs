using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PaisesTiposDocumentosService : IPaisesTiposDocumentosService
    {

        private readonly IPaisesTiposDocumentosRepository<PaisesTiposDocumento> _contactRepo;

        public PaisesTiposDocumentosService(IPaisesTiposDocumentosRepository<PaisesTiposDocumento> contactRepo)
        {
            _contactRepo = contactRepo;
        }
        public async Task<bool> Actualizar(PaisesTiposDocumento model)
        {
            return await _contactRepo.Actualizar(model);
        }

        public async Task<bool> Eliminar(int id)
        {
            return await _contactRepo.Eliminar(id);
        }

        public async Task<bool> Insertar(PaisesTiposDocumento model)
        {
            return await _contactRepo.Insertar(model);
        }

        public async Task<PaisesTiposDocumento> Obtener(int id)
        {
            return await _contactRepo.Obtener(id);
        }

        public async Task<IQueryable<PaisesTiposDocumento>> ObtenerTodos()
        {
            return await _contactRepo.ObtenerTodos();
        }



    }
}
