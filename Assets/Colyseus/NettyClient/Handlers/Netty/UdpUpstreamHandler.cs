using Assets.Colyseus.NettyClient.App;
using Coleseus.Shared.Event;
using Coleseus.Shared.Handlers.Netty;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.Handlers.Netty
{
    public class UdpUpstreamHandler : SimpleChannelInboundHandler<DatagramPacket>
    {
        private readonly MessageBufferEventDecoder decoder;

        public UdpUpstreamHandler() : base()
        {

            decoder = new MessageBufferEventDecoder();
        }


        protected override void ChannelRead0(IChannelHandlerContext ctx,
                DatagramPacket packet)
        {
            ISession session = NettyUdpClient.CLIENTS[ctx.Channel.LocalAddress];
            if (null != session)
            {
                IEvent @event = (IEvent)decoder.Decode(null, packet.Content);
                // Pass the event on to the session
                session.onEvent(@event);
            }
        }

    }
}
