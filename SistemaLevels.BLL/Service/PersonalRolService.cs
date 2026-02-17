using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PersonalRolService : IPersonalRolService
    {

        private readonly IPersonalRolRepository<PersonalRol> _contactRepo;

        public PersonalRolService(IPersonalRolRepository<PersonalRol> contactRepo)
        {
            _contactRepo = contactRepo;
        }
        public async Task<bool> Actualizar(PersonalRol model)
        {
            return await _contactRepo.Actualizar(model);
        }

        public async Task<bool> Eliminar(int id)
        {
            return await _contactRepo.Eliminar(id);
        }

        public async Task<bool> Insertar(PersonalRol model)
        {
            return await _contactRepo.Insertar(model);
        }

        public async Task<PersonalRol> Obtener(int id)
        {
            return await _contactRepo.Obtener(id);
        }


        public async Task<IQueryable<PersonalRol>> ObtenerTodos()
        {
            return await _contactRepo.ObtenerTodos();
        }



    }
}
