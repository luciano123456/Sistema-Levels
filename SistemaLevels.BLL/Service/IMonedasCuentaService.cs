using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IMonedasCuentaService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(MonedasCuenta model);
        Task<bool> Insertar(MonedasCuenta model);

        Task<MonedasCuenta> Obtener(int id);
        Task<IQueryable<MonedasCuenta>> ObtenerMoneda(int idMoneda);

        Task<IQueryable<MonedasCuenta>> ObtenerTodos();
    }

}
