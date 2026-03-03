using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IOpcionesBinariasRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(OpcionesBinaria model);
        Task<bool> Insertar(OpcionesBinaria model);
        Task<OpcionesBinaria> Obtener(int id);
        Task<IQueryable<OpcionesBinaria>> ObtenerTodos();
    }
}
