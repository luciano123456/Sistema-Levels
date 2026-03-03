using SistemaLevels.BLL.Common;
using SistemaLevels.Models;
using System.Linq;

namespace SistemaLevels.BLL.Service
{
    public interface IVentasService
    {
        Task<ServiceResult> Insertar(Venta venta, List<VentasArtista> artistas, List<VentasPersonal> personal, List<VentasCobro> cobros);
        Task<ServiceResult> Actualizar(Venta venta, List<VentasArtista> artistas, List<VentasPersonal> personal, List<VentasCobro> cobros);
        Task<ServiceResult> Eliminar(int id);

        Task<Venta?> Obtener(int id);
        Task<IQueryable<Venta>> ObtenerTodos();
        Task<IQueryable<Venta>> ObtenerPorCliente(int idCliente);
    }
}