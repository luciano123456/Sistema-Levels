using SistemaLevels.BLL.Common;
using SistemaLevels.Models;

public interface IProductorasService
{
    Task<ServiceResult> Insertar(Productora model, List<int> clientesIds);

    Task<ServiceResult> Actualizar(Productora model, List<int> clientesIds);

    Task<ServiceResult> Eliminar(int id);

    Task<Productora?> Obtener(int id);

    Task<IQueryable<Productora>> ObtenerTodos();

    Task<List<int>> ObtenerClientesAsociadosAutomaticos(int idProductora);
}