using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface ITiposContratosRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(TiposContrato model);
        Task<bool> Insertar(TiposContrato model);
        Task<TiposContrato> Obtener(int id);
        Task<IQueryable<TiposContrato>> ObtenerTodos();
    }
}
