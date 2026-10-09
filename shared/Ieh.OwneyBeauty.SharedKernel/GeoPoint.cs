using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace Ieh.OwneyBeauty.SharedKernel;

/// <summary>Coordenada WGS84. Las ubicaciones exactas son datos sensibles (ver OB-14-004).</summary>
public sealed class GeoPoint : ValueObject
{
    private const double EarthRadiusKm = 6371.0088;

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    private GeoPoint()
    {
    }

    public GeoPoint(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90 || double.IsNaN(latitude))
        {
            throw new BusinessException(OwneyErrorCodes.Shared.InvalidCoordinates).WithData("latitude", latitude);
        }

        if (longitude is < -180 or > 180 || double.IsNaN(longitude))
        {
            throw new BusinessException(OwneyErrorCodes.Shared.InvalidCoordinates).WithData("longitude", longitude);
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Distancia en línea recta (haversine). Fallback cuando el proveedor de mapas no responde.</summary>
    public double DistanceKmTo(GeoPoint other)
    {
        Check.NotNull(other, nameof(other));
        var dLat = ToRadians(other.Latitude - Latitude);
        var dLon = ToRadians(other.Longitude - Longitude);
        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) * Math.Pow(Math.Sin(dLon / 2), 2);
        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    /// <summary>Redondea la coordenada para mostrar una ubicación aproximada pública (~1 km con 2 decimales).</summary>
    public GeoPoint Approximate(int decimals = 2) => new(Math.Round(Latitude, decimals), Math.Round(Longitude, decimals));

    private static double ToRadians(double degrees) => degrees * Math.PI / 180d;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
