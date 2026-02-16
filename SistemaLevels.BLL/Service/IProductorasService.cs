using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IProductorasService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Productora model);
        Task<bool> Insertar(Productora model);

        Task<Productora?> Obtener(int id);
        Task<IQueryable<Productora>> ObtenerTodos();
    }
}
