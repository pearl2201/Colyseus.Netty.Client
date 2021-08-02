using Coleseus.Shared.Event;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Coleseus.Shared.Handlers.Netty
{
    public class UdpEventEncoder : MessageBufferEventEncoder
    {

        private EndPoint udpServerAddress;

        public UdpEventEncoder(EndPoint udpServerAddress)
        {
            this.udpServerAddress = udpServerAddress;
        }


        protected override void Encode(IChannelHandlerContext ctx, IEvent @event,
                List<Object> @out)
        {
            IByteBuffer data = (IByteBuffer)base.Encode(ctx, @event);
            @out.Add(new DatagramPacket(data, udpServerAddress));
            ctx.Flush();
        }

        public EndPoint getUdpServerAddress()
        {
            return udpServerAddress;
        }
        public void setUdpServerAddress(EndPoint udpServerAddress)
        {
            this.udpServerAddress = udpServerAddress;
        }

        public override bool IsSharable => true;
    }
}
