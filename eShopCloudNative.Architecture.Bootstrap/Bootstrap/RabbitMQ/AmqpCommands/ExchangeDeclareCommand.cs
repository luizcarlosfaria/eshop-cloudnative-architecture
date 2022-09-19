using Dawn;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;
public class ExchangeDeclareCommand : IAmqpCommand
{
    public string Exchange { get; set; }
    public string Type { get; set; }
    public bool Durable { get; set; }
    public bool AutoDelete { get; set; }
    public IDictionary<string, object> Arguments { get; set; }

    public void Prepare()
    {
        Guard.Argument(this.Exchange).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(this.Type).NotNull().NotEmpty().NotWhiteSpace();
    }

    public void Execute(IModel model)
        => Guard.Argument(model).NotNull().Value
        .ExchangeDeclare(exchange: this.Exchange, type: this.Type, durable: this.Durable, autoDelete: this.AutoDelete, arguments: this.Arguments);

}
