using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IPersonalRolRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PersonalRol model);
        Task<bool> Insertar(PersonalRol model);
        Task<PersonalRol> Obtener(int id);
        Task<IQueryable<PersonalRol>> ObtenerTodos();
    }
}
