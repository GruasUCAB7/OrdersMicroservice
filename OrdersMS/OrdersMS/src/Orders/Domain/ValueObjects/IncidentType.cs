using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class IncidentType : IValueObject<IncidentType>
    {
        public static readonly string ColisionFrontal = "Colisión Frontal:";
        public static readonly string FalloFrenos = "Fallo de Frenos";
        public static readonly string TrenDelantero = "Rotura del Tren Delantero";
        public static readonly string FallaBateria = "Falla de Batería";
        public static readonly string Sobrecalentamiento = "Sobrecalentamiento del Motor";
        public static readonly string FugasLiquidos = "Fugas de Líquidos";
        public static readonly string FallaSistemaElectrico = "Fallas en el Sistema Eléctrico";
        public static readonly string ProblemaBujias = "Problemas con las Bujías";
        public static readonly string DesgasteNeumaticos = "Desgaste de Neumáticos";

        public string Type { get; }

        public IncidentType(string type)
        {
            if (type != ColisionFrontal
                && type != FalloFrenos
                && type != TrenDelantero
                && type != FallaBateria
                && type != Sobrecalentamiento
                && type != FugasLiquidos
                && type != FallaSistemaElectrico
                && type != ProblemaBujias
                && type != DesgasteNeumaticos)
            {
                throw new InvalidIncidentTypeException($"Invalid order status: {type}. Allowed values are: {ColisionFrontal}, {FalloFrenos}, {TrenDelantero}, {FallaBateria}, {Sobrecalentamiento}, {FugasLiquidos}, {FallaSistemaElectrico}, {ProblemaBujias}, {DesgasteNeumaticos}.");
            }
            Type = type;
        }

        public string GetValue()
        {
            return Type;
        }

        public bool Equals(IncidentType other)
        {
            return Type == other.Type;
        }
    }
}
