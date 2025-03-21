﻿using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class VehicleModel : IValueObject<VehicleModel>
    {
        private string Model { get; }
        public VehicleModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model) || model.Length < 2)
            {
                throw new InvalidVehicleModelException();
            }
            Model = model;
        }

        public string GetValue()
        {
            return Model;
        }

        public bool Equals(VehicleModel other)
        {
            return Model == other.Model;
        }
    }
}
