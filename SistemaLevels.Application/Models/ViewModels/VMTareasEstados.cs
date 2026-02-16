using SistemaLevels.Models;

namespace SistemaLevels.Application.Models.ViewModels
{
    public class VMTareasEstados
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public virtual ICollection<User> Usuarios { get; set; } = new List<User>();

    }
}
