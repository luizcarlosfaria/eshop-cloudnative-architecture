using RabbitMQ.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;
public class QueueBindCommand : IAmqpCommand
{
    public string Queue { get; set; }
    public string Exchange { get; set; }
    public string RoutingKey { get; set; }
    public IDictionary<string, object> Arguments { get; set; }

    public void Prepare() { }

    public void Execute(IModel model)
        => model.QueueBind(queue: this.Queue, exchange: this.Exchange, routingKey: this.RoutingKey, arguments: this.Arguments);


}
