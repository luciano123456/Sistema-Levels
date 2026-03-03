using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IVentasEstadosRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(VentasEstado model);
        Task<bool> Insertar(VentasEstado model);
        Task<VentasEstado> Obtener(int id);
        Task<IQueryable<VentasEstado>> ObtenerTodos();
    }
}
