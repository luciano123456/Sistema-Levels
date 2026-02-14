using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPaisService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Pais model);
        Task<bool> Insertar(Pais model);

        Task<Pais> Obtener(int id);

        Task<IQueryable<Pais>> ObtenerTodos();
    }

}
