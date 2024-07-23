using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRabbitMQ.RabbitMQ.Interface;

public delegate void MessageReceivedCallback<TMessage>(TMessage message);
