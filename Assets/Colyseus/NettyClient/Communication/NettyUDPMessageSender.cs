using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net;
using UnityEngine;

namespace Coleseus.Shared.Communication
{
    /**
  * This class is used to send messages to a remote UDP client or server. An
  * instance of this class will be created by the {@link UDPUpstreamHandler} when
  * a {@link Events#CONNECT} event is received from client. The created instance
  * of this class is then sent as payload of a {@link DefaultNetworkEvent} to the
  * {@link Session}.
  * 
  * 
  * @author Abraham Menacherry
  * 
  */
    public class NettyUDPMessageSender : Fast
    {


        private bool isClosed = false;
        private EndPoint remoteAddress;
        private IDatagramChannel channel;
        private readonly System.Object _lockObj = new System.Object();

        private const DeliveryGuaranty DELIVERY_GUARANTY = DeliveryGuaranty.FAST;

        public NettyUDPMessageSender(EndPoint remoteAddress,
                IDatagramChannel channel
              )
        {
            this.remoteAddress = remoteAddress;
            this.channel = channel;

        }


        public System.Object sendMessage(System.Object message)
        {
            channel.WriteAndFlushAsync(message).GetAwaiter().GetResult();
            return null;
        }


        public DeliveryGuaranty getDeliveryGuaranty()
        {
            return DELIVERY_GUARANTY;
        }


        public void close()
        {
            lock (_lockObj)
            {
                if (isClosed)
                    return;
                bool removed = NettyUdpClient.CLIENTS.Remove(channel.LocalAddress);
                if (!removed)
                {
                    Debug.LogError("Possible memory leak occurred. "
                    + "The session associated with udp localaddress: "
                    + channel.LocalAddress
                    + " could not be removed from NettyUDPClient.CLIENTS map");
                }
                isClosed = true;
            }


        }

        public EndPoint getRemoteAddress()
        {
            return remoteAddress;
        }

        public IDatagramChannel getChannel()
        {
            return channel;
        }


        public override string ToString()
        {
            String channelId = "UDP Channel: ";
            if (null != channel)
            {
                channelId += channel.ToString();
            }
            else
            {
                channelId += "0";
            }
            String sender = "Netty " + channelId + " RemoteAddress: "
                    + remoteAddress;
            return sender;
        }
    }

}
