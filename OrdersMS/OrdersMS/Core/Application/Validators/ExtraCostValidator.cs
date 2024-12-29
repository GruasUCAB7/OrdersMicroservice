namespace OrdersMS.Core.Application.Validators
{
    public class ExtraCostValidator
    {
        private static readonly List<string> _extraCostName = new List<string>
        {
            "Cambio de Neumático", 
            "Desbloqueo del vehículo", 
            "Retiro de Vehículo del Estacionamiento",
            "Cambio de Aceite",
            "Carga de Batería",
            "Servicio de Cerrajería",
            "Suministro de Combustible"
        };

        public static bool ValidateExtraCostName(string name)
        {
            return _extraCostName.Contains(name);
        }
    }
}
