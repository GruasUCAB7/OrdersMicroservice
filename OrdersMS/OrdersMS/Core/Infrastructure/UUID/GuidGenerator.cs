using OrdersMS.Core.Application.IdGenerator;

namespace OrdersMS.Core.Infrastructure.UUID
{
    public class GuidGenerator : IdGenerator<string>
    {
        public string Generate()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
