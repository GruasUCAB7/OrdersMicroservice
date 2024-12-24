using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class ContractStatus : IValueObject<ContractStatus>
    {
        public static readonly string Activo = "Activo";
        public static readonly string Expirado = "Expirado";
        public static readonly string Cancelado = "Cancelado";

        public string Status { get; }

        public ContractStatus(string status)
        {
            if (status != Activo && status != Expirado && status != Cancelado)
            {
                throw new InvalidContractStatusException($"Invalid contract status: {status}. Allowed values are: {Activo}, {Expirado}, {Cancelado}.");
            }
            Status = status;
        }

        public string GetValue()
        {
            return Status;
        }

        public bool Equals(ContractStatus other)
        {
            return Status == other.Status;
        }
    }
}
