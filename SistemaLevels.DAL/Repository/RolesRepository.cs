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
    public class RolesRepository : IRolesRepository<UsuariosRole>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public RolesRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(UsuariosRole model)
        {
            _dbcontext.UsuariosRoles.Update(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            UsuariosRole model = _dbcontext.UsuariosRoles.First(c => c.Id == id);
            _dbcontext.UsuariosRoles.Remove(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Insertar(UsuariosRole model)
        {
            _dbcontext.UsuariosRoles.Add(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<UsuariosRole> Obtener(int id)
        {
            UsuariosRole model = await _dbcontext.UsuariosRoles.FindAsync(id);
            return model;
        }
        public async Task<IQueryable<UsuariosRole>> ObtenerTodos()
        {
            IQueryable<UsuariosRole> query = _dbcontext.UsuariosRoles;
            return await Task.FromResult(query);
        }




    }
}
