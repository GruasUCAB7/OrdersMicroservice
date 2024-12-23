﻿using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.InsuredVehicles.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.ValueObjects
{
    public class VehicleYear : IValueObject<VehicleYear>
    {
        private int Year { get; }

        public VehicleYear(int year)
        {
            if (year < 1995 || year > DateTime.Now.Year)
            {
                throw new InvalidVehicleYearException();
            }
            Year = year;
        }

        public int GetValue()
        {
            return Year;
        }

        public bool Equals(VehicleYear other)
        {
            return Year == other.Year;
        }
    }
}
