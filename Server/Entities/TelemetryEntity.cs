using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Entities
{
    /// <summary>
    /// Отображение телеметрических данных для работы с БД.
    /// </summary>
    [Table("TelemetryDetails")]
    public class TelemetryEntity
    {
        /// <summary>
        /// Id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Время записи.
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// Широта.
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Долгота.
        /// </summary>
        public double Longitude { get; set; }
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
}
