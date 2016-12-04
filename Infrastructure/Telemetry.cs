using System;
using System.Device.Location;
using System.Runtime.Serialization;

namespace Infrastructure
{
    [DataContract]
    public struct Telemetry
    {
        /// <summary>
        /// Время записи.
        /// </summary>
        [DataMember]
        public DateTime Time { get; set; }
        /// <summary>
        /// Координаты.
        /// </summary>
        [DataMember]
        public GeoCoordinate Coordinates { get; set; }
        /// <summary>
        /// Скорость.
        /// </summary>
        [DataMember]
        public double Speed { get; set; }
        /// <summary>
        /// Состояние датчика работы двигателя.
        /// </summary>
        [DataMember]
        public bool Engine { get; set; }
        /// <summary>
        /// Текущий общий пробег (в километрах).
        /// </summary>
        [DataMember]
        public double TotalMileage { get; set; }
    }
}
