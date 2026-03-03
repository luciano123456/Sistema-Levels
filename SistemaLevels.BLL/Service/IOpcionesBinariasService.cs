using SistemaLevels.Models;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IOpcionesBinariasService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(OpcionesBinaria model);
        Task<bool> Insertar(OpcionesBinaria model);

        Task<OpcionesBinaria> Obtener(int id);

        Task<IQueryable<OpcionesBinaria>> ObtenerTodos();
    }

}
