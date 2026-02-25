using Microsoft.EntityFrameworkCore;
using SistemaLevels.BLL.Service;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<SistemaLevelsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SistemaDB")));


// Agregar Razor Pages
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Registrar repositorios y servicios
builder.Services.AddScoped<IUsuariosRepository<User>, UsuariosRepository>();
builder.Services.AddScoped<IUsuariosService, UsuariosService>();

builder.Services.AddScoped<IEstadosUsuariosRepository<UsuariosEstado>, UsuariosEstadosRepository>();
builder.Services.AddScoped<IEstadosUsuariosService, EstadosUsuariosService>();

builder.Services.AddScoped<IRolesRepository<UsuariosRol>, RolesRepository>();
builder.Services.AddScoped<IRolesService, RolesService>();


builder.Services.AddScoped<IPaisRepository<Pais>, PaisRepository>();
builder.Services.AddScoped<IPaisService, PaisService>();

builder.Services.AddScoped<IPaisesTiposDocumentosRepository<PaisesTiposDocumento>, PaisesTiposDocumentosRepository>();
builder.Services.AddScoped<IPaisesTiposDocumentosService, PaisesTiposDocumentosService>();

builder.Services.AddScoped<IPaisesCondicionesIvaRepository<PaisesCondicionesIva>, PaisesCondicionesIvaRepository>();
builder.Services.AddScoped<IPaisesCondicionesIvaService, PaisesCondicionesIvaService>();

builder.Services.AddScoped<IPaisesMonedaRepository<PaisesMoneda>, PaisesMonedaRepository>();
builder.Services.AddScoped<IPaisesMonedaService, PaisesMonedaService>();


builder.Services.AddScoped<IPaisesProvinciaRepository<PaisesProvincia>, PaisesProvinciaRepository>();
builder.Services.AddScoped<IPaisesProvinciaService, PaisesProvinciaService>();

builder.Services.AddScoped<IRepresentantesRepository<Representante>, RepresentantesRepository>();
builder.Services.AddScoped<IRepresentantesService, RepresentantesService>();


builder.Services.AddScoped<IProductorasRepository, ProductorasRepository>();
builder.Services.AddScoped<IProductorasService, ProductorasService>();

builder.Services.AddScoped<IPersonalRepository, PersonalRepository>();
builder.Services.AddScoped<IPersonalService, PersonalService>();


builder.Services.AddScoped<IArtistasRepository<Artista>, ArtistasRepository>();
builder.Services.AddScoped<IArtistasService, ArtistasService>();

builder.Services.AddScoped<ITareasEstadosRepository<TareasEstado>, TareasEstadosRepository>();
builder.Services.AddScoped<ITareasEstadosService, TareasEstadosService>();

builder.Services.AddScoped<ITareasRepository<Tarea>, TareasRepository>();
builder.Services.AddScoped<ITareasService, TareasService>();

builder.Services.AddScoped<IClientesRepository, ClientesRepository>();
builder.Services.AddScoped<IClientesService, ClientesService>();

builder.Services.AddScoped<IGastosCategoriasRepository<GastosCategoria>, GastosCategoriasRepository>();
builder.Services.AddScoped<IGastosCategoriasService, GastosCategoriasService>();

builder.Services.AddScoped<IGastosRepository<Gasto>, GastosRepository>();
builder.Services.AddScoped<IGastosService, GastosService>();

builder.Services.AddScoped<IMonedasCuentaRepository<MonedasCuenta>, MonedasCuentaRepository>();
builder.Services.AddScoped<IMonedasCuentaService, MonedasCuentaService>();

builder.Services.AddScoped<IPersonalRolRepository<PersonalRol>, PersonalRolRepository>();
builder.Services.AddScoped<IPersonalRolService, PersonalRolService>();



builder.Services.AddScoped<ILoginRepository<User>, LoginRepository>();
builder.Services.AddScoped<ILoginService, LoginService>();



builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
    });



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });

// Definir el esquema de autenticación predeterminado
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});



var app = builder.Build();

// Configurar el pipeline de middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Clientes/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

// Asegúrate de que las rutas de login estén excluidas del middleware de autenticación
app.MapControllerRoute(
    name: "login",
    pattern: "Login/{action=Index}",
    defaults: new { controller = "Login", action = "Index" });
app.Run();
    