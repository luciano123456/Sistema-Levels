using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPaisesProvinciaService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PaisesProvincia model);
        Task<bool> Insertar(PaisesProvincia model);

        Task<PaisesProvincia> Obtener(int id);

        Task<IQueryable<PaisesProvincia>> ObtenerTodos();
    }

}
