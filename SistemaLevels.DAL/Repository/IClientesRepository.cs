using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IClientesRepository<T> where T : class
    {
        Task<bool> Insertar(T model, List<int> productorasIds);

        Task<bool> Actualizar(T model, List<int> productorasIds);

        Task<bool> Eliminar(int id);

        Task<T?> Obtener(int id);

        Task<IQueryable<T>> ObtenerTodos();

        /* ================= DUPLICADOS ================= */

        Task<T?> BuscarDuplicado(
            int? idExcluir,
            string? nombre,
            string? numeroDocumento,
            string? dni);
    }
}