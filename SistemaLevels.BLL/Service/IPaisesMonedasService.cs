using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPaisesMonedaService
    {
        Task<bool> Insertar(PaisesMoneda model);
        Task<bool> Actualizar(PaisesMoneda model);
        Task<bool> Eliminar(int id);

        Task<PaisesMoneda?> Obtener(int id);
        Task<IQueryable<PaisesMoneda>> ObtenerTodos();
    }
}
