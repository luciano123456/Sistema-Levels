using SistemaLevels.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IRepresentantesRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Representante model);
        Task<bool> Insertar(Representante model);

        Task<Representante?> Obtener(int id);
        Task<IQueryable<Representante>> ObtenerTodos();
    }
}
