using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IPaisesTiposDocumentosService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PaisesTiposDocumento model);
        Task<bool> Insertar(PaisesTiposDocumento model);

        Task<PaisesTiposDocumento> Obtener(int id);

        Task<IQueryable<PaisesTiposDocumento>> ObtenerTodos();
    }

}
