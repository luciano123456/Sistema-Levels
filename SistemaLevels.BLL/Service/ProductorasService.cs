using SistemaLevels.Models;

public class ProductorasService : IProductorasService
{
    private readonly IProductorasRepository<Productora> _repo;

    public ProductorasService(IProductorasRepository<Productora> repo)
    {
        _repo = repo;
    }

    public async Task<bool> Actualizar(Productora model, List<int> clientesIds)
        => await _repo.Actualizar(model, clientesIds);

    public async Task<bool> Eliminar(int id)
        => await _repo.Eliminar(id);

    public async Task<bool> Insertar(Productora model, List<int> clientesIds)
        => await _repo.Insertar(model, clientesIds);

    public async Task<Productora?> Obtener(int id)
        => await _repo.Obtener(id);

    public async Task<IQueryable<Productora>> ObtenerTodos()
        => await _repo.ObtenerTodos();
}
