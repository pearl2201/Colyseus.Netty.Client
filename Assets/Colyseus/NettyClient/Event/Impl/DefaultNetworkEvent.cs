﻿using Coleseus.Shared.Communication;
using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Text;
using Assets.Colyseus.NettyClient.Utils;
namespace Coleseus.Shared.Event.Impl
{

    /**
	 * Default implementation of {@link NetworkEvent} interface. This class wraps a
	 * message that needs to be transmitted to a remote Machine or VM.
	 * 
	 * @author Abraham Menacherry
	 * 
	 */
    public class DefaultNetworkEvent : DefaultEvent, INetworkEvent
    {
        private DeliveryGuaranty guaranty = DeliveryGuaranty.RELIABLE;

        /**
         * Default constructor which will set the {@link DeliveryGuaranty} to
         * RELIABLE. It will also set the type of the event to
         * {@link Events#NETWORK_MESSAGE}.
         */
        public DefaultNetworkEvent() : base()
        {
            base.setType(Events.NETWORK_MESSAGE);
            this.guaranty = DeliveryGuaranty.RELIABLE;
        }

        /**
       * Copy constructor which will take values from the event and set it on this
       * instance. It will disregard the type of the event and set it to
       * {@link Events#NETWORK_MESSAGE}. {@link DeliveryGuarantyOptions} is set to the
       * value passed in
       * 
       * @param event
       *            The instance from which payload, create time etc will be
       *            copied
       * 
       * @param deliveryGuaranty
       */
        public DefaultNetworkEvent(IEvent @event, DeliveryGuaranty deliveryGuaranty)

        {
            this.setSource(@event.getSource());
            this.setTimeStamp(@event.getTimeStamp());
            this.guaranty = DeliveryGuaranty.RELIABLE;
            base.setType(Events.NETWORK_MESSAGE);
        }

        /**
         * Copy constructor which will take values from the event and set it on this
         * instance. It will disregard the type of the event and set it to
         * {@link Events#NETWORK_MESSAGE}. {@link DeliveryGuarantyOptions} is set to
         * RELIABLE.
         * 
         * @param event
         *            The instance from which payload, create time etc will be
         *            copied
         */
        public DefaultNetworkEvent(IEvent @event) : this(@event, DeliveryGuaranty.RELIABLE)
        {

        }




        public DeliveryGuaranty getDeliveryGuaranty()
        {
            return guaranty;
        }


        public void setDeliveryGuaranty(DeliveryGuaranty deliveryGuaranty)
        {
            this.guaranty = deliveryGuaranty;
        }


        public override void setType(int type)
        {
            throw new Exception(
                    "Event type of this class is already set to NETWORK_MESSAGE. "
                            + "It should not be reset.");
        }

        public static ushort NetworkPackageId => 0;

        public override IByteBuffer getBufferData()
        {
            MessageBuffer<IByteBuffer> msgBuffer = getSourceBuffer();
            IByteBuffer data = msgBuffer.getNativeBuffer();
            IByteBuffer opcode = Unpooled.Buffer();
            opcode.WriteUnsignedShort(NetworkPackageId);
            var msg = Unpooled.WrappedBuffer(opcode, data);
            return msg;

        }
    }

    public class EntireStateEvent : DefaultNetworkEvent
    {
        public EntireStateEvent() : base()
        {

        }

        public EntireStateEvent(IEvent @event, DeliveryGuaranty deliveryGuaranty)

        {

        }

        /**
         * Copy constructor which will take values from the event and set it on this
         * instance. It will disregard the type of the event and set it to
         * {@link Events#NETWORK_MESSAGE}. {@link DeliveryGuarantyOptions} is set to
         * RELIABLE.
         * 
         * @param event
         *            The instance from which payload, create time etc will be
         *            copied
         */
        public EntireStateEvent(IEvent @event) : this(@event, DeliveryGuaranty.RELIABLE)
        {

        }

        public static new ushort NetworkPackageId => 1;
    }
}
