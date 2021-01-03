using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Networking
{
    public class Publisher
    {
        public static void publish(UserInfo userInfo)
        {
            using (var bus = RabbitHutch.CreateBus("host=sparrow.rmq.cloudamqp.com;virtualHost=ryrrglkj;username=ryrrglkj;password=XMSeAe8LnWckqsGxlNGNb5ShUEW22HcK"))
            {
                bus.PubSub.PublishAsync(userInfo,"locations");
            }
        }
    }
}
