using SistemaLevels.Models;
using System.Linq;

namespace SistemaLevels.DAL.Repository
{
    public interface IVentasRepository<T> where T : class
    {
        Task<bool> Insertar(Venta venta, List<VentasArtista> artistas, List<VentasPersonal> personal, List<VentasCobro> cobros);
        Task<bool> Actualizar(Venta venta, List<VentasArtista> artistas, List<VentasPersonal> personal, List<VentasCobro> cobros);
        Task<bool> Eliminar(int id);

        Task<Venta?> Obtener(int id);
        Task<IQueryable<Venta>> ObtenerTodos();
        Task<IQueryable<Venta>> ObtenerPorCliente(int idCliente);
    }
}