using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SistemaLevels.DAL.Repository
{
    public class PersonalRolRepository : IPersonalRolRepository<PersonalRol>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public PersonalRolRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(PersonalRol model)
        {
            _dbcontext.PersonalRoles.Update(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            PersonalRol model = _dbcontext.PersonalRoles.First(c => c.Id == id);
            _dbcontext.PersonalRoles.Remove(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Insertar(PersonalRol model)
        {
            _dbcontext.PersonalRoles.Add(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<PersonalRol> Obtener(int id)
        {
            PersonalRol model = await _dbcontext.PersonalRoles.FindAsync(id);
            return model;
        }
        public async Task<IQueryable<PersonalRol>> ObtenerTodos()
        {
            IQueryable<PersonalRol> query = _dbcontext.PersonalRoles;
            return await Task.FromResult(query);
        }




    }
}
