using SistemaLevels.Models;

public interface IProductorasRepository
{
    Task<bool> Insertar(Productora model, List<int> clientesIds);

    Task<bool> Actualizar(Productora model, List<int> clientesIds);

    Task<bool> Eliminar(int id);

    Task<Productora?> Obtener(int id);

    Task<IQueryable<Productora>> ObtenerTodos();

    Task<List<int>> ObtenerClientesAsociadosAutomaticos(int idProductora);

    // ⭐ NUEVO
    Task<Productora?> BuscarDuplicado(
        int? id,
        string? nombre,
        string? dni,
        string? cuit);
}