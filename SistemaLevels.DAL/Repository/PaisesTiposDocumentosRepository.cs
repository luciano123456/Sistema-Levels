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
    public class PaisesTiposDocumentosRepository : IPaisesTiposDocumentosRepository<PaisesTiposDocumento>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public PaisesTiposDocumentosRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(PaisesTiposDocumento model)
        {
            try
            {
                _dbcontext.PaisesTiposDocumentos.Update(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                PaisesTiposDocumento model = _dbcontext.PaisesTiposDocumentos.First(c => c.Id == id);
                _dbcontext.PaisesTiposDocumentos.Remove(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Insertar(PaisesTiposDocumento model)
        {
            try
            {
                _dbcontext.PaisesTiposDocumentos.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<PaisesTiposDocumento> Obtener(int id)
        {
            try
            {
                PaisesTiposDocumento model = await _dbcontext.PaisesTiposDocumentos
                    .Include(x => x.IdPaisNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);

                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public async Task<IQueryable<PaisesTiposDocumento>> ObtenerTodos()
        {
            try
            {
                IQueryable<PaisesTiposDocumento> query = _dbcontext.PaisesTiposDocumentos;

                return await Task.FromResult(query);

            }
            catch (Exception ex)
            {
                return null;
            }
        }




    }
}
