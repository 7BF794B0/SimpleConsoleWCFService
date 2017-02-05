using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Runtime.Serialization;

namespace Contracts
{
    /// <summary>
    /// The structure describing all telemetry data that we track.
    /// </summary>
    public struct Telemetry
    {
        /// <summary>
        /// Время записи.
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// Координаты.
        /// </summary>
        public GeoCoordinate Coordinates { get; set; }
        /// <summary>
        /// Скорость.
        /// </summary>
        /// <remarks>
        /// Измеряется в километрах в час.
        /// </remarks>
        public double SpeedKmh { get; set; }
        /// <summary>
        /// Состояние датчика работы двигателя.
        /// </summary>
        public bool Engine { get; set; }
        /// <summary>
        /// Текущий общий пробег.
        /// </summary>
        /// <remarks>
        /// Измеряется в километрах.
        /// </remarks>
        public double TotalMileageKm { get; set; }
    }

    [DataContract]
    public class TelemetryCollection
    {
        /// <summary>
        /// A collection of telemetry data.
        /// </summary>
        [DataMember]
        public List<Telemetry> Collection { get; set; }

        /// <summary>
        /// A collection of telemetry data.
        /// </summary>
        [DataMember]
        public int TerminalId { get; set; }

        public TelemetryCollection()
        {
            Collection = new List<Telemetry>();
        }

        public TelemetryCollection(List<Telemetry> lst)
        {
            Collection = lst;
        }
    }
}
