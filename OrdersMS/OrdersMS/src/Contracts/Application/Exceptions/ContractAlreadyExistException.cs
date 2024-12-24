namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class ContractAlreadyExistException(string policyId, string vehicleId) 
        : ApplicationException($"Contract already exist with this PolicyId and VehicleId: {policyId}, {vehicleId}.")
    {
    }
}
