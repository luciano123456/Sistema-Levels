using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IRolesService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(UsuariosRole model);
        Task<bool> Insertar(UsuariosRole model);

        Task<UsuariosRole> Obtener(int id);

        Task<IQueryable<UsuariosRole>> ObtenerTodos();
    }

}
