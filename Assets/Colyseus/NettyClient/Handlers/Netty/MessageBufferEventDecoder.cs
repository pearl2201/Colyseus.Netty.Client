﻿using Coleseus.Shared.Event;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coleseus.Shared.Handlers.Netty
{


    /**
	 * This decoder will convert a Netty {@link ByteBuf} to a
	 * {@link NettyMessageBuffer}. It will also convert
	 * {@link Events#NETWORK_MESSAGE} events to {@link Events#SESSION_MESSAGE}
	 * event.
	 * 
	 * @author Abraham Menacherry
	 * 
	 */
    //TODO check if MessageToMessageDecoder can be replaced with MessageToByteDecoder

    public class MessageBufferEventDecoder : MessageToMessageDecoder<IByteBuffer>
    {


        protected override void Decode(IChannelHandlerContext ctx, IByteBuffer buffer,
                List<Object> @out)
        {
            if (buffer.ReadableBytes > 0)
            {
                @out.Add(Decode(ctx, buffer));
            }

        }


        public IEvent Decode(IChannelHandlerContext ctx, IByteBuffer @in)
        {
            if (@in.ReadableBytes > 0)
            {
                byte opcode = @in.ReadByte();
                if (opcode == Events.NETWORK_MESSAGE)
                {
                    opcode = Events.SESSION_MESSAGE;
                }
                IByteBuffer data = @in.ReadBytes(@in.ReadableBytes);
                return Events.CreateEvent(new NettyMessageBuffer(data), opcode);
            }
            return null;
        }

        public override bool IsSharable => true;
    }
}
