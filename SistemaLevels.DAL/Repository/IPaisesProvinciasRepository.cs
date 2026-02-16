using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IPaisesProvinciaRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PaisesProvincia model);
        Task<bool> Insertar(PaisesProvincia model);
        Task<PaisesProvincia> Obtener(int id);
        Task<IQueryable<PaisesProvincia>> ObtenerPais(int idPais);
        Task<IQueryable<PaisesProvincia>> ObtenerTodos();
    }
}
