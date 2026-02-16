using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPaisesCondicionesIvaService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PaisesCondicionesIva model);
        Task<bool> Insertar(PaisesCondicionesIva model);

        Task<PaisesCondicionesIva> Obtener(int id);

        Task<IQueryable<PaisesCondicionesIva>> ObtenerTodos();
    }

}
