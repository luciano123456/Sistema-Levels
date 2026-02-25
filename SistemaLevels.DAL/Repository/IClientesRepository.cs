using SistemaLevels.Models;

public interface IClientesRepository<TEntityModel> where TEntityModel : class
{
    Task<bool> Insertar(Cliente model, List<int> productorasIds);

    Task<bool> Actualizar(Cliente model, List<int> productorasIds);

    Task<bool> Eliminar(int id);

    Task<Cliente?> Obtener(int id);

    Task<IQueryable<Cliente>> ObtenerTodos();
}