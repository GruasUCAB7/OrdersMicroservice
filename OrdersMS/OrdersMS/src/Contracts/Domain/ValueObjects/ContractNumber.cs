using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class ContractNumber : IValueObject<ContractNumber>
    {
        private int Number { get; }

        public ContractNumber(int number)
        {
            if (number < 0)
            {
                throw new InvalidContractNumberException();
            }
            Number = number;
        }

        public int GetValue()
        {
            return Number;
        }

        public bool Equals(ContractNumber other)
        {
            return Number == other.Number;
        }
    }
}
