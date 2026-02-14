using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IRolesRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(UsuariosRole model);
        Task<bool> Insertar(UsuariosRole model);
        Task<UsuariosRole> Obtener(int id);
        Task<IQueryable<UsuariosRole>> ObtenerTodos();
    }
}
