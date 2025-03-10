﻿using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.Core.Utils.RegExps;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class VehicleId : IValueObject<VehicleId>
    {
        private string Id { get; }

        public VehicleId(string id)
        {
            if (!UUIDRegExps.UUIDRegExp.IsMatch(id))
            {
                throw new InvalidVehicleIdException();
            }
            Id = id;
        }

        public string GetValue()
        {
            return Id;
        }

        public bool Equals(VehicleId other)
        {
            return Id == other.Id;
        }
    }
}
