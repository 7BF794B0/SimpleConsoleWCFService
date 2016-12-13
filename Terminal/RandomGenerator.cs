using System;
using System.Device.Location;
using System.Linq;

using Contracts;

namespace Terminal
{
    class RandomGenerator
    {
        private readonly TelemetryCollection _dataSet = new TelemetryCollection();
        private bool _firstFlag = true;

        private static double CalculateSpeed(double time, double mileage)
        {
            return (mileage/1000)/time;
        }

        public TelemetryCollection Next()
        {
            Random rnd = new Random();
            Telemetry tel1;

            // Старотвая позиция используется 1 раз (в первый).
            double startLatitude = rnd.Next(10, 70);
            double startLongitude = rnd.Next(10, 70);
            // Рандомное значение времени в пути.
            DateTime randomTime;
            // Новые рандомные координаты.
            double newLatitude;
            double newLongitude;
            // Новая дистанция, которую мы проехали за время randomMinutes.
            double newDistanse;

            GeoCoordinate tempCoord;

            // Первый элемент списка в формирующийся коллекции должен знать о последнем элементе в предыдущей коллекции.
            if (_firstFlag)
            {
                tel1 = new Telemetry
                {
                    Time = DateTime.Now,
                    Coordinates = new GeoCoordinate(startLatitude, startLongitude),
                    SpeedKmh = 0.0d,
                    Engine = true,
                    TotalMileageKm = 0.0d
                };
                _dataSet.Collection.Add(tel1);
                _firstFlag = false;
            }
            else
            {
                randomTime = _dataSet.Collection.Last().Time.AddMinutes(rnd.Next(30, 90));
                newLatitude = _dataSet.Collection.Last().Coordinates.Latitude + rnd.NextDouble();
                newLongitude = _dataSet.Collection.Last().Coordinates.Longitude + rnd.NextDouble();
                tempCoord = new GeoCoordinate(newLatitude, newLongitude);
                newDistanse = _dataSet.Collection.Last().Coordinates.GetDistanceTo(tempCoord);

                tel1 = new Telemetry
                {
                    Time = randomTime,
                    Coordinates = new GeoCoordinate(newLatitude, newLongitude),
                    SpeedKmh = CalculateSpeed((randomTime - _dataSet.Collection.Last().Time).TotalHours, newDistanse),
                    Engine = true,
                    TotalMileageKm = (_dataSet.Collection.Last().TotalMileageKm + newDistanse) / 1000
                };

                _dataSet.Collection.Clear();
                _dataSet.Collection.Add(tel1);
            }

            randomTime = _dataSet.Collection.Last().Time.AddMinutes(rnd.Next(30, 90));
            newLatitude = _dataSet.Collection.Last().Coordinates.Latitude + rnd.NextDouble();
            newLongitude = _dataSet.Collection.Last().Coordinates.Longitude + rnd.NextDouble();
            tempCoord = new GeoCoordinate(newLatitude, newLongitude);
            newDistanse = _dataSet.Collection.Last().Coordinates.GetDistanceTo(tempCoord);

            var tel2 = new Telemetry
            {
                Time = randomTime,
                Coordinates = new GeoCoordinate(newLatitude, newLongitude),
                SpeedKmh = CalculateSpeed((randomTime - _dataSet.Collection.Last().Time).TotalHours, newDistanse),
                Engine = true,
                TotalMileageKm = (_dataSet.Collection.Last().TotalMileageKm + newDistanse) / 1000
            };
            _dataSet.Collection.Add(tel2);

            randomTime = _dataSet.Collection.Last().Time.AddMinutes(rnd.Next(30, 90));
            newLatitude = _dataSet.Collection.Last().Coordinates.Latitude + rnd.NextDouble();
            newLongitude = _dataSet.Collection.Last().Coordinates.Longitude + rnd.NextDouble();
            tempCoord = new GeoCoordinate(newLatitude, newLongitude);
            newDistanse = _dataSet.Collection.Last().Coordinates.GetDistanceTo(tempCoord);

            var tel3 = new Telemetry
            {
                Time = randomTime,
                Coordinates = new GeoCoordinate(newLatitude, newLongitude),
                SpeedKmh = CalculateSpeed((randomTime - _dataSet.Collection.Last().Time).TotalHours, newDistanse),
                Engine = true,
                TotalMileageKm = (_dataSet.Collection.Last().TotalMileageKm + newDistanse) / 1000
            };
            _dataSet.Collection.Add(tel3);

            return _dataSet;
        }
    }
}
