using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface ITareasEstadosRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(TareasEstado model);
        Task<bool> Insertar(TareasEstado model);
        Task<TareasEstado> Obtener(int id);
        Task<IQueryable<TareasEstado>> ObtenerTodos();
    }
}
