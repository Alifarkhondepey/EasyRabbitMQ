using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRabbitMQ.RabbitMQ.Enums
{
    public enum ExchangeType
    {
        Direct,
        Topic,
        Fanout,
        Headers
    }
}
