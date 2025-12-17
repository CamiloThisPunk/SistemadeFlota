namespace TrackWay.Domain.Enums;

/// <summary>
/// Estado operativo del vehículo
/// </summary>
public enum EstadoOperativo
{
    Activo = 1,
    EnMantenimiento = 2,
    FueraDeServicio = 3,
    EnRuta = 4,
    Reservado = 5,
    BajaDefinitiva = 6
}

/// <summary>
/// Tipo de combustible del vehículo
/// </summary>
public enum TipoCombustible
{
    Gasolina = 1,
    Diesel = 2,
    GLP = 3,
    GNV = 4,
    Electrico = 5,
    Hibrido = 6
}

/// <summary>
/// Estado de una orden de mantenimiento
/// </summary>
public enum EstadoMantenimiento
{
    Pendiente = 1,
    Programado = 2,
    EnTaller = 3,
    EnEjecucion = 4,
    Finalizado = 5,
    Cancelado = 6
}

/// <summary>
/// Tipo de mantenimiento
/// </summary>
public enum TipoMantenimiento
{
    Preventivo = 1,
    Correctivo = 2,
    Predictivo = 3,
    Emergencia = 4
}

/// <summary>
/// Tipo de documento vehicular
/// </summary>
public enum TipoDocumento
{
    SOAT = 1,
    SeguroVehicular = 2,
    RevisionTecnica = 3,
    TarjetaPropiedad = 4,
    PermisoCirculacion = 5,
    CertificadoGNV = 6,
    PolizaTodoRiesgo = 7
}

/// <summary>
/// Categoría de licencia de conducir
/// </summary>
public enum CategoriaLicencia
{
    AI = 1,     // Vehículos menores
    AIIa = 2,   // Automóviles hasta 8 pasajeros
    AIIb = 3,   // Automóviles hasta 16 pasajeros
    AIIIa = 4,  // Transporte de mercancías hasta 3.5 ton
    AIIIb = 5,  // Transporte de mercancías mayor a 3.5 ton
    AIIIc = 6,  // Transporte de mercancías articulados
    AIVA = 7,   // Transporte de personas
    BIVB = 8    // Transporte especial
}
