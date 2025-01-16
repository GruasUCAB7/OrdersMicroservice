using OrdersMS.Core.Domain.Aggregates;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.Events;
using OrdersMS.src.Orders.Domain.Exceptions;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Domain
{
    public class Order(OrderId id) : AggregateRoot<OrderId>(id)
    {
        private new OrderId _id = id;
        private ContractId _contractId;
        private DriverId _driverAssigned;
        private UserId _operatorAssigned;
        private Coordinates _incidentAddress;
        private Coordinates _destinationAddress;
        private IncidentType _incidentType;
        private DateTime _incidentDate;
        private List<ExtraCost> _extraServicesApplied;
        private TotalCost _totalCost;
        private OrderStatus _status;

        public string GetId() => _id.GetValue();
        public string GetContractId() => _contractId.GetValue();
        public string GetOperatorAssigned() => _operatorAssigned.GetValue();
        public string GetDriverAssigned() => _driverAssigned.GetValue();
        public Coordinates GetIncidentAddress() => _incidentAddress;
        public Coordinates GetDestinationAddress() => _destinationAddress;
        public string GetIncidentType() => _incidentType.GetValue();
        public double GetIncidentAddressLatitude() => _incidentAddress.GetLatitude();
        public double GetIncidentAddressLongitude() => _incidentAddress.GetLongitude();
        public double GetDestinationAddressLatitude() => _destinationAddress.GetLatitude();
        public double GetDestinationAddressLongitude() => _destinationAddress.GetLongitude();
        public DateTime GetIncidentDate() => _incidentDate;
        public List<ExtraCost> GetExtrasServicesApplied() => _extraServicesApplied; 
        public decimal GetTotalCost() => _totalCost.GetValue();
        public string GetOrderStatus() => _status.GetValue();
        public void SetStatus(OrderStatus status) => _status = status;
        public void SetTotalCost(TotalCost amount) => _totalCost = amount;
        public void SetDriverAssigned(DriverId driverId) => _driverAssigned = driverId;
        public void SetExtraServicesApplied(List<ExtraCost> extraCosts) => _extraServicesApplied = extraCosts;
        public void SetIncidentDate(DateTime incidentDate) => _incidentDate = incidentDate;

        public static Order CreateOrder(
            OrderId Id, 
            ContractId ContractId,
            UserId OperatorId,
            DriverId DriverAssigned,
            Coordinates IncidentAddress,
            Coordinates DestinationAddress,
            IncidentType IncidentType,
            DateTime IncidentDate,
            List<ExtraCost> ExtraServicesApplied)
        {
            var order = new Order(Id);
            order.Apply(OrderCreated.CreateEvent(Id, ContractId, OperatorId, DriverAssigned, IncidentAddress, DestinationAddress, IncidentType, IncidentDate, ExtraServicesApplied));
            return order;
        }

        public void OnOrderCreatedEvent(OrderCreated context)
        {
            _id = new OrderId(context.Id);
            _contractId = new ContractId(context.ContractId);
            _operatorAssigned = new UserId(context.OperatorId);
            _driverAssigned = new DriverId(context.DriverId);
            _incidentAddress = new Coordinates(context.IncidentAddress.Latitude, context.IncidentAddress.Longitude);
            _destinationAddress = new Coordinates(context.DestinationAddress.Latitude, context.DestinationAddress.Longitude);
            _incidentType = new IncidentType(context.IncidentType);
            _incidentDate = context.IncidentDate;
            _extraServicesApplied = new List<ExtraCost>();
            _totalCost = new TotalCost(0);
            _status = new OrderStatus("Por Asignar");
        }

        public ExtraCost AddExtraCost(ExtraCostId id, ExtraCostName name, ExtraCostPrice price)
        {
            Apply(ExtraCostAdded.CreateEvent(_id, id, name, price));
            return _extraServicesApplied.Last();
        }

        public ExtraCost OnExtraCostAddedEvent(ExtraCostAdded context)
        {
            var extraCost = new ExtraCost(
                new ExtraCostId(context.id),
                new ExtraCostName(context.name),
                new ExtraCostPrice(context.price)
            );
            _extraServicesApplied.Add(extraCost);
            return extraCost;
        }

        public override void ValidateState()
        {
            if (_id == null ||  _contractId == null || _operatorAssigned == null || _incidentAddress == null || _destinationAddress == null || _incidentType == null)
            {
                throw new InvalidOrderException();
            }
        }
    }
}
