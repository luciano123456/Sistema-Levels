using SistemaLevels.Models;

public interface IProductorasRepository<TEntityModel> where TEntityModel : class
{
    Task<bool> Eliminar(int id);
    Task<bool> Actualizar(Productora model, List<int> clientesIds);
    Task<bool> Insertar(Productora model, List<int> clientesIds);

    Task<Productora?> Obtener(int id);
    Task<IQueryable<Productora>> ObtenerTodos();
}
