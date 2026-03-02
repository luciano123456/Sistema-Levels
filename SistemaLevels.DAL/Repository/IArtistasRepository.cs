using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IArtistasRepository<T> where T : class
    {
        Task<bool> Insertar(T model, List<int> personalIds);

        Task<bool> Actualizar(T model, List<int> personalIds);

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