using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.DataContext;

public partial class SistemaLevelsContext : DbContext
{
    public SistemaLevelsContext()
    {
    }

    public SistemaLevelsContext(DbContextOptions<SistemaLevelsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Artista> Artistas { get; set; }

    public virtual DbSet<ArtistasCuentaCorriente> ArtistasCuentaCorrientes { get; set; }

    public virtual DbSet<ArtistasPago> ArtistasPagos { get; set; }

    public virtual DbSet<Caja> Cajas { get; set; }

    public virtual DbSet<CajasTransferenciasCuenta> CajasTransferenciasCuentas { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<ClientesCuentaCorriente> ClientesCuentaCorrientes { get; set; }

    public virtual DbSet<ClientesProductorasAsignada> ClientesProductorasAsignadas { get; set; }

    public virtual DbSet<Gasto> Gastos { get; set; }

    public virtual DbSet<GastosCategoria> GastosCategorias { get; set; }

    public virtual DbSet<MonedasCuenta> MonedasCuentas { get; set; }

    public virtual DbSet<OpcionesBinaria> OpcionesBinarias { get; set; }

    public virtual DbSet<Pais> Paises { get; set; }

    public virtual DbSet<PaisesCondicionesIva> PaisesCondicionesIvas { get; set; }

    public virtual DbSet<PaisesMoneda> PaisesMonedas { get; set; }

    public virtual DbSet<PaisesProvincia> PaisesProvincias { get; set; }

    public virtual DbSet<PaisesTiposDocumento> PaisesTiposDocumentos { get; set; }

    public virtual DbSet<Personal> Personals { get; set; }

    public virtual DbSet<PersonalArtistasAsignado> PersonalArtistasAsignados { get; set; }

    public virtual DbSet<PersonalCargo> PersonalCargos { get; set; }

    public virtual DbSet<PersonalCuentaCorriente> PersonalCuentaCorrientes { get; set; }

    public virtual DbSet<PersonalPago> PersonalPagos { get; set; }

    public virtual DbSet<PersonalRol> PersonalRoles { get; set; }

    public virtual DbSet<PersonalRolesAsignado> PersonalRolesAsignados { get; set; }

    public virtual DbSet<PersonalSueldo> PersonalSueldos { get; set; }

    public virtual DbSet<PersonalSueldosPago> PersonalSueldosPagos { get; set; }

    public virtual DbSet<Presupuesto> Presupuestos { get; set; }

    public virtual DbSet<PresupuestosDetalle> PresupuestosDetalles { get; set; }

    public virtual DbSet<PresupuestosEstado> PresupuestosEstados { get; set; }

    public virtual DbSet<Productora> Productoras { get; set; }

    public virtual DbSet<ProductorasClientesAsignado> ProductorasClientesAsignados { get; set; }

    public virtual DbSet<Representante> Representantes { get; set; }

    public virtual DbSet<Tarea> Tareas { get; set; }

    public virtual DbSet<TareasEstado> TareasEstados { get; set; }

    public virtual DbSet<TiposComisione> TiposComisiones { get; set; }

    public virtual DbSet<TiposContrato> TiposContratos { get; set; }

    public virtual DbSet<Ubicacion> Ubicaciones { get; set; }

    public virtual DbSet<User> Usuarios { get; set; }

    public virtual DbSet<UsuariosEstado> UsuariosEstados { get; set; }

    public virtual DbSet<UsuariosRol> UsuariosRoles { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    public virtual DbSet<VentasArtista> VentasArtistas { get; set; }

    public virtual DbSet<VentasCobro> VentasCobros { get; set; }

    public virtual DbSet<VentasCobrosComisione> VentasCobrosComisiones { get; set; }

    public virtual DbSet<VentasEstado> VentasEstados { get; set; }

    public virtual DbSet<VentasPersonal> VentasPersonals { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-3MT5F5F; Database=Sistema_Levels; Integrated Security=true; Trusted_Connection=True; Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artista>(entity =>
        {
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EntreCalles)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaNacimiento).HasColumnType("date");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Localidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreArtistico)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PrecioNegMax).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PrecioNegMin).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TelefonoAlternativo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCondicionIvaNavigation).WithMany(p => p.Artista)
                .HasForeignKey(d => d.IdCondicionIva)
                .HasConstraintName("FK_Artistas_Paises_CondicionesIVA");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.Artista)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_Paises_Monedas");

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.Artista)
                .HasForeignKey(d => d.IdPais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_Paises");

            entity.HasOne(d => d.IdProductoraNavigation).WithMany(p => p.Artista)
                .HasForeignKey(d => d.IdProductora)
                .HasConstraintName("FK_Artistas_Productoras");

            entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.Artista)
                .HasForeignKey(d => d.IdProvincia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_Paises_Provincias");

            entity.HasOne(d => d.IdRepresentanteNavigation).WithMany(p => p.Artista)
                .HasForeignKey(d => d.IdRepresentante)
                .HasConstraintName("FK_Artistas_Representantes");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Artista)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("FK_Artistas_Paises_TiposDocumentos");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ArtistaIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Artistas_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.ArtistaIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_UsuariosRegistra");
        });

        modelBuilder.Entity<ArtistasCuentaCorriente>(entity =>
        {
            entity.ToTable("Artistas_CuentaCorriente");

            entity.Property(e => e.Concepto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Debe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Haber).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoMov)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdArtistaNavigation).WithMany(p => p.ArtistasCuentaCorrientes)
                .HasForeignKey(d => d.IdArtista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_CuentaCorriente_Artistas");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.ArtistasCuentaCorrientes)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_CuentaCorriente_Paises_Monedas");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ArtistasCuentaCorrienteIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Artistas_CuentaCorriente_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.ArtistasCuentaCorrienteIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_CuentaCorriente_UsuariosRegistra");
        });

        modelBuilder.Entity<ArtistasPago>(entity =>
        {
            entity.ToTable("Artistas_Pagos");

            entity.Property(e => e.Concepto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Conversion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Cotizacion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.IdArtistaCc).HasColumnName("IdArtistaCC");
            entity.Property(e => e.IdMonedaCc).HasColumnName("IdMonedaCC");
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NotaArtista)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NotaInterna)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdArtistaNavigation).WithMany(p => p.ArtistasPagos)
                .HasForeignKey(d => d.IdArtista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_Pagos_Artistas");

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.ArtistasPagos)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_Pagos_Monedas_Cuentas");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.ArtistasPagos)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_Pagos_Paises_Monedas");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ArtistasPagoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Artistas_Pagos_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.ArtistasPagoIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Artistas_Pagos_UsuariosRegistra");
        });

        modelBuilder.Entity<Caja>(entity =>
        {
            entity.Property(e => e.Concepto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Egrreso).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Ingreso).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Saldo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoMov)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cajas_Monedas_Cuentas");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cajas_Paises_Monedas");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.CajaIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Cajas_Usuarios1");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.CajaIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cajas_Usuarios");
        });

        modelBuilder.Entity<CajasTransferenciasCuenta>(entity =>
        {
            entity.Property(e => e.Cotizacion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.ImporteDestino).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ImporteOrigen).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NotaInterna)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCuentaDestinoNavigation).WithMany(p => p.CajasTransferenciasCuentaIdCuentaDestinoNavigations)
                .HasForeignKey(d => d.IdCuentaDestino)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CajasTransferenciasCuentas_Monedas_Cuentas1");

            entity.HasOne(d => d.IdCuentaOrigenNavigation).WithMany(p => p.CajasTransferenciasCuentaIdCuentaOrigenNavigations)
                .HasForeignKey(d => d.IdCuentaOrigen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CajasTransferenciasCuentas_Monedas_Cuentas");

            entity.HasOne(d => d.IdMonedaDestinoNavigation).WithMany(p => p.CajasTransferenciasCuentaIdMonedaDestinoNavigations)
                .HasForeignKey(d => d.IdMonedaDestino)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CajasTransferenciasCuentas_Paises_Monedas1");

            entity.HasOne(d => d.IdMonedaOrigenNavigation).WithMany(p => p.CajasTransferenciasCuentaIdMonedaOrigenNavigations)
                .HasForeignKey(d => d.IdMonedaOrigen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CajasTransferenciasCuentas_Paises_Monedas");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.CajasTransferenciasCuentaIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_CajasTransferenciasCuentas_Usuarios1");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.CajasTransferenciasCuentaIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CajasTransferenciasCuentas_Usuarios");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EntreCalles)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Localidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TelefonoAlternativo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCondicionIvaNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdCondicionIva)
                .HasConstraintName("FK_Clientes_Paises_CondicionesIVA");

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdPais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_Paises");

            entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdProvincia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_Paises_Provincias");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("FK_Clientes_Paises_TiposDocumentos");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ClienteIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Clientes_Usuarios");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.ClienteIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_UsuariosRegistra");
        });

        modelBuilder.Entity<ClientesCuentaCorriente>(entity =>
        {
            entity.ToTable("Clientes_CuentaCorriente");

            entity.Property(e => e.Concepto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Debe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Haber).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoMov)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ClientesCuentaCorrientes)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_CuentaCorriente_Clientes");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.ClientesCuentaCorrientes)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_CuentaCorriente_Paises_Monedas");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ClientesCuentaCorrienteIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Clientes_CuentaCorriente_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.ClientesCuentaCorrienteIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_CuentaCorriente_UsuariosRegistra");
        });

        modelBuilder.Entity<ClientesProductorasAsignada>(entity =>
        {
            entity.ToTable("Clientes_ProductorasAsignadas");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.ClientesProductorasAsignada)
                .HasForeignKey<ClientesProductorasAsignada>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_ProductorasAsignadas_Productoras");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ClientesProductorasAsignada)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK_Clientes_ProductorasAsignadas_Clientes");
        });

        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.Property(e => e.Concepto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NotaInterna)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gastos_GastosCategorias");

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gastos_Monedas_Cuentas");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gastos_Paises_Monedas");

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.IdPersonal)
                .HasConstraintName("FK_Gastos_Personal");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.GastoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Gastos_Usuarios1");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.GastoIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Gastos_Usuarios");
        });

        modelBuilder.Entity<GastosCategoria>(entity =>
        {
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MonedasCuenta>(entity =>
        {
            entity.ToTable("Monedas_Cuentas");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.MonedasCuenta)
                .HasForeignKey(d => d.IdMoneda)
                .HasConstraintName("FK_Monedas_Cuentas_Paises_Monedas");
        });

        modelBuilder.Entity<OpcionesBinaria>(entity =>
        {
            entity.ToTable("Opciones_Binarias");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Pais>(entity =>
        {
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PaisesCondicionesIva>(entity =>
        {
            entity.ToTable("Paises_CondicionesIVA");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.PaisesCondicionesIvas)
                .HasForeignKey(d => d.IdPais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Paises_CondicionesIVA_Paises");
        });

        modelBuilder.Entity<PaisesMoneda>(entity =>
        {
            entity.ToTable("Paises_Monedas");

            entity.Property(e => e.Cotizacion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.PaisesMoneda)
                .HasForeignKey(d => d.IdPais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Paises_Monedas_Paises");
        });

        modelBuilder.Entity<PaisesProvincia>(entity =>
        {
            entity.ToTable("Paises_Provincias");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.PaisesProvincia)
                .HasForeignKey(d => d.IdPais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Paises_Provincias_Paises");
        });

        modelBuilder.Entity<PaisesTiposDocumento>(entity =>
        {
            entity.ToTable("Paises_TiposDocumentos");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.PaisesTiposDocumentos)
                .HasForeignKey(d => d.IdPais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Paises_TiposDocumentos_Paises");
        });

        modelBuilder.Entity<Personal>(entity =>
        {
            entity.ToTable("Personal");

            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaNacimiento).HasColumnType("date");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCondicionIvaNavigation).WithMany(p => p.Personals)
                .HasForeignKey(d => d.IdCondicionIva)
                .HasConstraintName("FK_Personal_Paises_CondicionesIVA");

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.Personals)
                .HasForeignKey(d => d.IdPais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Paises");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Personals)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("FK_Personal_Paises_TiposDocumentos");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.PersonalIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Personal_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.PersonalIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_UsuariosRegistra");
        });

        modelBuilder.Entity<PersonalArtistasAsignado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Personal__3214EC073A426D9C");

            entity.ToTable("Personal_Artistas_Asignados");

            entity.HasIndex(e => new { e.IdPersonal, e.IdArtista }, "UQ_Personal_Artista").IsUnique();

            entity.HasOne(d => d.IdArtistaNavigation).WithMany(p => p.PersonalArtistasAsignados)
                .HasForeignKey(d => d.IdArtista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PersonalArtistas_Artista");

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.PersonalArtistasAsignados)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PersonalArtistas_Personal");
        });

        modelBuilder.Entity<PersonalCargo>(entity =>
        {
            entity.ToTable("Personal_Cargos");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PersonalCuentaCorriente>(entity =>
        {
            entity.ToTable("Personal_CuentaCorriente");

            entity.Property(e => e.Concepto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Debe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Haber).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TipoMov)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.PersonalCuentaCorrientes)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_CuentaCorriente_Paises_Monedas");

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.PersonalCuentaCorrientes)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_CuentaCorriente_Personal");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.PersonalCuentaCorrienteIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Personal_CuentaCorriente_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.PersonalCuentaCorrienteIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_CuentaCorriente_UsuariosRegistra");
        });

        modelBuilder.Entity<PersonalPago>(entity =>
        {
            entity.ToTable("Personal_Pagos");

            entity.Property(e => e.Concepto)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Conversion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Cotizacion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.IdMonedaCc).HasColumnName("IdMonedaCC");
            entity.Property(e => e.IdPersonalCc).HasColumnName("IdPersonalCC");
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NotaInterna)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NotaPersonal)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.PersonalPagos)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Pagos_Monedas_Cuentas");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.PersonalPagos)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Pagos_Paises_Monedas");

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.PersonalPagos)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Pagos_Personal");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.PersonalPagoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Personal_Pagos_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.PersonalPagoIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Pagos_UsuariosRegistra");
        });

        modelBuilder.Entity<PersonalRol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Personal__3214EC0792E105B6");

            entity.ToTable("Personal_Roles");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PersonalRolesAsignado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Personal__3214EC07ACC4FE44");

            entity.ToTable("Personal_Roles_Asignados");

            entity.HasIndex(e => new { e.IdPersonal, e.IdRol }, "UQ_Personal_Rol").IsUnique();

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.PersonalRolesAsignados)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PersonalRoles_Personal");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.PersonalRolesAsignados)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PersonalRoles_Rol");
        });

        modelBuilder.Entity<PersonalSueldo>(entity =>
        {
            entity.ToTable("Personal_Sueldos");

            entity.Property(e => e.Concepto)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.IdPersonalCc).HasColumnName("IdPersonalCC");
            entity.Property(e => e.ImporteTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NotaInterna)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NotaPersonal)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.PersonalSueldos)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Sueldos_Paises_Monedas");

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.PersonalSueldos)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Sueldos_Personal");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.PersonalSueldoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Personal_Sueldos_Usuarios1");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.PersonalSueldoIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Sueldos_Usuarios");
        });

        modelBuilder.Entity<PersonalSueldosPago>(entity =>
        {
            entity.ToTable("Personal_Sueldos_Pagos");

            entity.Property(e => e.Conversion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Cotizacion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.IdPersonalCc).HasColumnName("IdPersonalCC");
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.PersonalSueldosPagos)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Sueldos_Pagos_Monedas_Cuentas");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.PersonalSueldosPagos)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Sueldos_Pagos_Paises_Monedas");

            entity.HasOne(d => d.IdSueldoNavigation).WithMany(p => p.PersonalSueldosPagos)
                .HasForeignKey(d => d.IdSueldo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Sueldos_Pagos_Personal_Sueldos");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.PersonalSueldosPagoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Personal_Sueldos_Pagos_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.PersonalSueldosPagoIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Personal_Sueldos_Pagos_UsuariosRegistra");
        });

        modelBuilder.Entity<Presupuesto>(entity =>
        {
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.NotaCliente)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NotaInterna)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Presupuestos)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Presupuestos_Clientes");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Presupuestos)
                .HasForeignKey(d => d.IdEstado)
                .HasConstraintName("FK_Presupuestos_PresupuestosEstados");

            entity.HasOne(d => d.IdProductoraNavigation).WithMany(p => p.Presupuestos)
                .HasForeignKey(d => d.IdProductora)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Presupuestos_Productoras");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.PresupuestoIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Presupuestos_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.PresupuestoIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Presupuestos_UsuariosRegistra");
        });

        modelBuilder.Entity<PresupuestosDetalle>(entity =>
        {
            entity.ToTable("PresupuestosDetalle");

            entity.Property(e => e.Duracion).HasColumnType("datetime");
            entity.Property(e => e.FechaEvento).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NombreEvento)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NotaCliente)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NotaInterna)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdArtistaNavigation).WithMany(p => p.PresupuestosDetalles)
                .HasForeignKey(d => d.IdArtista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PresupuestosDetalle_Artistas");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.PresupuestosDetalles)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PresupuestosDetalle_Paises_Monedas");

            entity.HasOne(d => d.IdPresupuestoNavigation).WithMany(p => p.PresupuestosDetalles)
                .HasForeignKey(d => d.IdPresupuesto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PresupuestosDetalle_Presupuestos");

            entity.HasOne(d => d.IdUbicacionNavigation).WithMany(p => p.PresupuestosDetalles)
                .HasForeignKey(d => d.IdUbicacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PresupuestosDetalle_Ubicaciones");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.PresupuestosDetalleIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_PresupuestosDetalle_Usuarios1");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.PresupuestosDetalleIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PresupuestosDetalle_Usuarios");
        });

        modelBuilder.Entity<PresupuestosEstado>(entity =>
        {
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Productora>(entity =>
        {
            entity.Property(e => e.CodigoPostal)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EntreCalles)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Localidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TelefonoAlternativo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCondicionIvaNavigation).WithMany(p => p.Productoras)
                .HasForeignKey(d => d.IdCondicionIva)
                .HasConstraintName("FK_Productoras_Paises_CondicionesIVA");

            entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.Productoras)
                .HasForeignKey(d => d.IdProvincia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Productoras_Paises_Provincias");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Productoras)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("FK_Productoras_Paises_TiposDocumentos");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.ProductoraIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Productoras_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.ProductoraIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Productoras_UsuariosRegistra");

            entity.HasOne(d => d.IdpaisNavigation).WithMany(p => p.Productoras)
                .HasForeignKey(d => d.Idpais)
                .HasConstraintName("FK_Productoras_Paises");
        });

        modelBuilder.Entity<ProductorasClientesAsignado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC072FA0984E");

            entity.ToTable("Productoras_ClientesAsignados");

            entity.HasIndex(e => e.IdCliente, "IX_ProdCliente_Cliente");

            entity.HasIndex(e => e.IdProductora, "IX_ProdCliente_Productora");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.ProductorasClientesAsignados)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProdCliente_Cliente");

            entity.HasOne(d => d.IdProductoraNavigation).WithMany(p => p.ProductorasClientesAsignados)
                .HasForeignKey(d => d.IdProductora)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProdCliente_Productora");
        });

        modelBuilder.Entity<Representante>(entity =>
        {
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.Representantes)
                .HasForeignKey(d => d.IdPais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Representantes_Paises");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Representantes)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("FK_Representantes_Paises_TiposDocumentos");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.RepresentanteIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Representantes_UsuariosModifica");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.RepresentanteIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Representantes_UsuariosRegistra");
        });

        modelBuilder.Entity<Tarea>(entity =>
        {
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaLimite).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Tareas)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tareas_TareasEstados");

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.Tareas)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tareas_Personal");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.TareaIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Tareas_Usuarios1");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.TareaIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tareas_Usuarios");
        });

        modelBuilder.Entity<TareasEstado>(entity =>
        {
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TiposComisione>(entity =>
        {
            entity.ToTable("Tipos_Comisiones");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TiposContrato>(entity =>
        {
            entity.ToTable("Tipos_Contratos");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ubicacion>(entity =>
        {
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Espacio)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CodigoRecuperacion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Usuario)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Usuario");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Usuarios_Estados");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Usuarios_Roles");
        });

        modelBuilder.Entity<UsuariosEstado>(entity =>
        {
            entity.ToTable("Usuarios_Estados");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UsuariosRol>(entity =>
        {
            entity.ToTable("Usuarios_Roles");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.Property(e => e.Duracion).HasColumnType("datetime");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaHasta).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.IdClienteCc).HasColumnName("IdClienteCC");
            entity.Property(e => e.ImporteAbonado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ImporteTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NombreEvento)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NotaCliente)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NotaInterna)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Saldo).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Clientes");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Ventas_Estados");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Paises_Monedas");

            entity.HasOne(d => d.IdOpExclusividadNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdOpExclusividad)
                .HasConstraintName("FK_Ventas_Opciones_Binarias");

            entity.HasOne(d => d.IdProductoraNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdProductora)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Productoras");

            entity.HasOne(d => d.IdTipoContratoNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdTipoContrato)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Tipos_Contratos");

            entity.HasOne(d => d.IdUbicacionNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUbicacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Ubicaciones");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.VentaIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Ventas_Usuarios1");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.VentaIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Usuarios");
        });

        modelBuilder.Entity<VentasArtista>(entity =>
        {
            entity.Property(e => e.IdArtistaCc).HasColumnName("IdArtistaCC");
            entity.Property(e => e.PorcComision).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalComision).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdArtistaNavigation).WithMany(p => p.VentasArtista)
                .HasForeignKey(d => d.IdArtista)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VentasArtistas_Artistas");

            entity.HasOne(d => d.IdRepresentanteNavigation).WithMany(p => p.VentasArtista)
                .HasForeignKey(d => d.IdRepresentante)
                .HasConstraintName("FK_VentasArtistas_Representantes");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.VentasArtista)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VentasArtistas_Ventas");
        });

        modelBuilder.Entity<VentasCobro>(entity =>
        {
            entity.ToTable("Ventas_Cobros");

            entity.Property(e => e.Conversion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Cotizacion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.IdArtistaCc).HasColumnName("IdArtistaCC");
            entity.Property(e => e.IdClienteCc).HasColumnName("IdClienteCC");
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NotaCliente)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NotaInterna)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.VentasCobros)
                .HasForeignKey(d => d.IdCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Cobros_Monedas_Cuentas");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.VentasCobros)
                .HasForeignKey(d => d.IdMoneda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Cobros_Paises_Monedas");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.VentasCobroIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Ventas_Cobros_UsuariosModifiac");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.VentasCobroIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Cobros_UsuariosRegistra");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.VentasCobros)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Cobros_Ventas");
        });

        modelBuilder.Entity<VentasCobrosComisione>(entity =>
        {
            entity.ToTable("Ventas_Cobros_Comisiones");

            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.IdPersonalCc).HasColumnName("IdPersonalCC");
            entity.Property(e => e.TotalComision).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.VentasCobrosComisiones)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Cobros_Comisiones_Personal");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.VentasCobrosComisioneIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Ventas_Cobros_Comisiones_Usuarios1");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.VentasCobrosComisioneIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Cobros_Comisiones_Usuarios");

            entity.HasOne(d => d.IdVentaCobroNavigation).WithMany(p => p.VentasCobrosComisiones)
                .HasForeignKey(d => d.IdVentaCobro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Cobros_Comisiones_Ventas_Cobros");
        });

        modelBuilder.Entity<VentasEstado>(entity =>
        {
            entity.ToTable("Ventas_Estados");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VentasPersonal>(entity =>
        {
            entity.ToTable("Ventas_Personal");

            entity.Property(e => e.FechaModifica).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistra).HasColumnType("datetime");
            entity.Property(e => e.PorcComision).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalComision).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdCargoNavigation).WithMany(p => p.VentasPersonals)
                .HasForeignKey(d => d.IdCargo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Personal_Personal_Cargos");

            entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.VentasPersonals)
                .HasForeignKey(d => d.IdPersonal)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Personal_Personal");

            entity.HasOne(d => d.IdTipoComisionNavigation).WithMany(p => p.VentasPersonals)
                .HasForeignKey(d => d.IdTipoComision)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Personal_Tipos_Comisiones");

            entity.HasOne(d => d.IdUsuarioModificaNavigation).WithMany(p => p.VentasPersonalIdUsuarioModificaNavigations)
                .HasForeignKey(d => d.IdUsuarioModifica)
                .HasConstraintName("FK_Ventas_Personal_Usuarios1");

            entity.HasOne(d => d.IdUsuarioRegistraNavigation).WithMany(p => p.VentasPersonalIdUsuarioRegistraNavigations)
                .HasForeignKey(d => d.IdUsuarioRegistra)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Personal_Usuarios");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.VentasPersonals)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ventas_Personal_Ventas");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
