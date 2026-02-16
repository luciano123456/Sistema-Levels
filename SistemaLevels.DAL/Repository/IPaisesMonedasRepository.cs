using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IPaisesMonedaRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PaisesMoneda model);
        Task<bool> Insertar(PaisesMoneda model);
        Task<PaisesMoneda> Obtener(int id);
        Task<IQueryable<PaisesMoneda>> ObtenerTodos();
    }
}
