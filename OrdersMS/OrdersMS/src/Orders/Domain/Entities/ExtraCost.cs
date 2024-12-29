using OrdersMS.Core.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Domain.Entities
{
    public class ExtraCost(ExtraCostId id, ExtraCostName name, ExtraCostPrice price) : Entity<ExtraCostId>(id)
    {
        private ExtraCostId _id = id;
        private ExtraCostName _name = name;
        private ExtraCostPrice _price = price;
        private bool _isActive = true;

        public string GetId() => _id.GetValue();
        public string GetName() => _name.GetValue();
        public bool GetIsActive() => _isActive;
        public decimal GetPrice() => _price.GetValue();
        public void SetIsActive(bool isActive) => _isActive = isActive;
    }
}
