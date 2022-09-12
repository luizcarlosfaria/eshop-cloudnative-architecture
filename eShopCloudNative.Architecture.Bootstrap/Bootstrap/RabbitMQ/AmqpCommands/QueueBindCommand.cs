using Ardalis.GuardClauses;
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

    public void Prepare()
    {
        Guard.Against.NullOrEmpty(this.Queue);
        Guard.Against.NullOrEmpty(this.Exchange);
        Guard.Against.Null(this.RoutingKey);
    }

    public void Execute(IModel model)
        => Guard.Against.Null(model)
        .QueueBind(queue: this.Queue, exchange: this.Exchange, routingKey: this.RoutingKey, arguments: this.Arguments);


}
