using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IPaisRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Pais model);
        Task<bool> Insertar(Pais model);
        Task<Pais> Obtener(int id);
        Task<IQueryable<Pais>> ObtenerTodos();
    }
}
