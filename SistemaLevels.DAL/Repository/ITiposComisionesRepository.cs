using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface ITiposComisionesRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(TiposComision model);
        Task<bool> Insertar(TiposComision model);
        Task<TiposComision> Obtener(int id);
        Task<IQueryable<TiposComision>> ObtenerTodos();
    }
}
