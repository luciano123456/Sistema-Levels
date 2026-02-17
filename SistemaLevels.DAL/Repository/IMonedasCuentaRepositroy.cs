using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IMonedasCuentaRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(MonedasCuenta model);
        Task<bool> Insertar(MonedasCuenta model);
        Task<MonedasCuenta> Obtener(int id);
        Task<IQueryable<MonedasCuenta>> ObtenerMoneda(int idMoneda);
        Task<IQueryable<MonedasCuenta>> ObtenerTodos();
    }
}
