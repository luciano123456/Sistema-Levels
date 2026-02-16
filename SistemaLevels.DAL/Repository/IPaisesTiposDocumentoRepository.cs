using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SistemaLevels.DAL.Repository
{
    public interface IPaisesTiposDocumentosRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(PaisesTiposDocumento model);
        Task<bool> Insertar(PaisesTiposDocumento model);
        Task<PaisesTiposDocumento> Obtener(int id);
        Task<IQueryable<PaisesTiposDocumento>> ObtenerTodos();
    }
}
