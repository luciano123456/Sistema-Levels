using SistemaLevels.BLL.Common;
using SistemaLevels.Models;
using System.Linq;

namespace SistemaLevels.BLL.Service
{
    public interface IPersonalSueldosService
    {
        Task<ServiceResult> Insertar(PersonalSueldo sueldo, List<PersonalSueldosPago> pagos);
        Task<ServiceResult> Actualizar(PersonalSueldo sueldo, List<PersonalSueldosPago> pagos);
        Task<ServiceResult> Eliminar(int id);

        Task<PersonalSueldo?> Obtener(int id);
        Task<IQueryable<PersonalSueldo>> ObtenerTodos();
        Task<IQueryable<PersonalSueldo>> ObtenerPorPersonal(int idPersonal);

        Task<IQueryable<PersonalSueldo>> ListarFiltrado(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? idPersonal,
            int? idMoneda,
            string? estado
        );
    }
}