using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPaisesMonedaService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PaisesMoneda model);
        Task<bool> Insertar(PaisesMoneda model);

        Task<PaisesMoneda> Obtener(int id);

        Task<IQueryable<PaisesMoneda>> ObtenerTodos();
    }

}
