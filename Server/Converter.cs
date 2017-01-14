using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using Server.Entities;
using Contracts;

namespace Server
{
    public static class Converter
    {
        public static List<TelemetryEntity> ToEntities(this TelemetryCollection lst)
        {
            return lst.Collection.Select(e => new TelemetryEntity
            {
                Time = e.Time,
                Latitude = e.Coordinates.Latitude,
                Longitude = e.Coordinates.Longitude,
                SpeedKmh = e.SpeedKmh,
                Engine = e.Engine,
                TotalMileageKm = e.TotalMileageKm
            }).ToList();
        }

        public static TelemetryCollection ToBaseFormat(this List<TelemetryEntity> lst)
        {
            return new TelemetryCollection(
                lst.Select(e => new Telemetry
                {
                    Time = e.Time,
                    Coordinates = new GeoCoordinate(e.Latitude, e.Longitude),
                    SpeedKmh = e.SpeedKmh,
                    Engine = e.Engine,
                    TotalMileageKm = e.TotalMileageKm
                }).ToList());
        }
    }
}
