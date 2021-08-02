using Coleseus.Shared.Event;
using DotNetty.Transport.Channels;
using System;
using UnityEngine;

namespace Coleseus.Shared.Communication
{
    public class NettyTCPMessageSender : Reliable
    {
        private bool isClosed = false;
        private readonly IChannel _channel;
        private DeliveryGuaranty DELIVERY_GUARANTY = DeliveryGuaranty.RELIABLE;

        private readonly System.Object _lockObj = new System.Object();

        public NettyTCPMessageSender(IChannel channel)
        {

            _channel = channel;

        }


        public System.Object sendMessage(System.Object message)
        {
            return _channel.WriteAndFlushAsync(message);
        }


        public DeliveryGuaranty getDeliveryGuaranty()
        {
            return DELIVERY_GUARANTY;
        }

        public IChannel getChannel()
        {
            return _channel;
        }

        /**
		 * Writes an the {@link Events#DISCONNECT} to the client, flushes
		 * all the pending writes and closes the channel.
		 * 
		 */

        public void close()
        {
            lock(this)
            {
                if (isClosed)
                    return;
                Debug.Log("Going to close tcp connection");
                IEvent @event = Events.CreateEvent(null, Events.DISCONNECT);
                if (_channel.Active)
                {
                    _channel.WriteAsync(@event).Wait();
                }
                else
                {
                    _channel.CloseAsync().Wait();
                    Debug.Log($"Unable to write the Event {@event} with type {@event.GetType()} to socket");

                }
                isClosed = true;
            }
        }

        /**
 * Writes an event mostly the {@link Events}.CLOSE to the client, flushes
 * all the pending writes and closes the channel.
 * 
 * @param closeEvent
 */
        public void close(IEvent closeEvent)
        {
            closeAfterFlushingPendingWrites(_channel, closeEvent);
        }

        /**
         * This method will write an event to the channel and then add a close
         * listener which will close it after the write has completed.
         * 
         * @param channel
         * @param event
         */
        public void closeAfterFlushingPendingWrites(IChannel channel, IEvent @event)

        {
            if (channel.Active)
            {
                channel.WriteAsync(@event).ContinueWith((x) =>
               {
                   return channel.CloseAsync();
               }).Wait();
            }
            else
            {
                Debug.Log("Unable to write the Event :" + @event
                        + " to socket as channel is ot connected");
            }
        }

        public override string ToString()
        {
            String channelId = "TCP channel: ";
            if (null != _channel)
            {
                channelId += _channel.ToString();
            }
            else
            {
                channelId += "0";
            }
            String sender = "Netty " + channelId;
            return sender;
        }
    }

}

