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
    }

    public void Execute(IModel model)
        => model.ExchangeBind(destination: this.Destination, source: this.Source, routingKey: this.RoutingKey, arguments: this.Arguments);


}
