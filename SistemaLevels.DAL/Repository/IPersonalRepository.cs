using SistemaLevels.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IPersonalRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Personal model);
        Task<bool> Insertar(Personal model);

        Task<Personal?> Obtener(int id);
        Task<IQueryable<Personal>> ObtenerTodos();
    }
}
