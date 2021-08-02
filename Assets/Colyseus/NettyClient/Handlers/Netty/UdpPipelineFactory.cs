using Coleseus.Shared.Handlers.Netty;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.Handlers.Netty
{
    public class UdpPipelineFactory : ChannelInitializer<IDatagramChannel>
    {
        private static readonly System.Object _lockObj = new System.Object();
        public readonly String EVENT_ENCODER_NAME = "eventEncoder";
       
        private static UdpPipelineFactory INSTANCE;
        private static UdpEventEncoder udpEventEncoder;
        public static UdpUpstreamHandler UDP_UPSTREAM_HANDLER = new UdpUpstreamHandler();

        private EndPoint udpServerAddress;
        public UdpPipelineFactory(EndPoint udpServerAddress)
        {
            this.udpServerAddress = udpServerAddress;
        }


        protected override void InitChannel(IDatagramChannel ch)
        {
            IChannelPipeline pipeline = ch.Pipeline;
            pipeline.AddLast(EVENT_ENCODER_NAME, getEventEncoder(udpServerAddress));
            pipeline.AddLast("UDPUpstreamHandler", UDP_UPSTREAM_HANDLER);
        }

        public static UdpPipelineFactory getInstance(EndPoint udpServerAddress)
        {
            lock (_lockObj)
            {
                if (null == INSTANCE)
                {
                    INSTANCE = new UdpPipelineFactory(udpServerAddress);
                }
                return INSTANCE;
            }

        }

        public static IChannelHandler getEventEncoder(EndPoint udpServerAddress)
        {
            lock (_lockObj)
            {
                if (null == udpEventEncoder)
                {
                    udpEventEncoder = new UdpEventEncoder(udpServerAddress);
                }
                return udpEventEncoder;
            }
        }


    }

}
