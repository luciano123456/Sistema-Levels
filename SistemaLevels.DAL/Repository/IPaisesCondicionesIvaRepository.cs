using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IPaisesCondicionesIvaRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PaisesCondicionesIva model);
        Task<bool> Insertar(PaisesCondicionesIva model);
        Task<PaisesCondicionesIva> Obtener(int id);
        Task<IQueryable<PaisesCondicionesIva>> ObtenerTodos();
    }
}
