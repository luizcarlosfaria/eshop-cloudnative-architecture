using Dawn;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;
public class QueueDeclareCommand : IAmqpCommand
{
    public string Queue { get; set; }
    public bool Durable { get; set; }
    public bool Exclusive { get; set; }
    public bool AutoDelete { get; set; }
    public IDictionary<string, object> Arguments { get; set; }

    public void Prepare()
    {
        Guard.Argument(this.Queue).NotNull().NotEmpty().NotWhiteSpace();
    }

    public void Execute(IModel model)
        => Guard.Argument(model).NotNull().Value
        .QueueDeclare(queue: this.Queue, durable: this.Durable, exclusive: this.Exclusive, autoDelete: this.AutoDelete, arguments: this.Arguments);

}
