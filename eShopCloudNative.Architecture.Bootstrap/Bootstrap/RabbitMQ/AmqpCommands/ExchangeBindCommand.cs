using Dawn;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;
public class ExchangeBindCommand : IAmqpCommand
{
    public string Destination { get; set; }
    public string Source { get; set; }
    public string RoutingKey { get; set; }
    public IDictionary<string, object> Arguments { get; set; }

    public void Prepare()
    {
        Guard.Argument(this.Destination, nameof(this.Destination)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(this.Source, nameof(this.Source)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(this.RoutingKey, nameof(this.RoutingKey)).NotNull();
    }

    public void Execute(IModel model)
        => Guard.Argument(model)
        .NotNull()
        .Value
        .ExchangeBind(destination: this.Destination, source: this.Source, routingKey: this.RoutingKey, arguments: this.Arguments);


}
