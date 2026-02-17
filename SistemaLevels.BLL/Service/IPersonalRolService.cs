using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPersonalRolService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PersonalRol model);
        Task<bool> Insertar(PersonalRol model);

        Task<PersonalRol> Obtener(int id);

        Task<IQueryable<PersonalRol>> ObtenerTodos();
    }

}
