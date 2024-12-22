﻿namespace OrdersMS.src.InsuredVehicles.Application.Commands.UpdateInsuredVehicle.Types
{
    public record UpdateVehicleResponse
    (
        string Id,
        string Brand,
        string Model,
        string Plate,
        string VehicleSize,
        int Year,
        bool IsActive,
        string ClientDNI,
        string ClientName,
        string ClientEmail
    );
}
