using SistemaLevels.Models;
using System.Linq;

namespace SistemaLevels.DAL.Repository
{
    public interface IPersonalSueldosRepository<T> where T : class
    {
        Task<bool> Insertar(PersonalSueldo sueldo, List<PersonalSueldosPago> pagos);
        Task<bool> Actualizar(PersonalSueldo sueldo, List<PersonalSueldosPago> pagos);
        Task<bool> Eliminar(int id);

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