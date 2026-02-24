using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IPaisesMonedaRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Insertar(PaisesMoneda model);
        Task<bool> Actualizar(PaisesMoneda model);
        Task<bool> Eliminar(int id);

        Task<PaisesMoneda?> Obtener(int id);
        Task<IQueryable<PaisesMoneda>> ObtenerTodos();

        Task<bool> ActualizarMasivo(Dictionary<int, decimal> monedas);
    }
}
