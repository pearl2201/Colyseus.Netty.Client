using Coleseus.Shared.Communication;
using Coleseus.Shared.Event;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coleseus.Shared.Handlers.Netty
{
    public class MessageBufferEventEncoder : MessageToMessageEncoder<IEvent>
    {
        protected override void Encode(IChannelHandlerContext ctx, IEvent @event,
            List<Object> @out)
        {
            @out.Add(Encode(ctx, @event));
        }

        /**
		 * Encode is separated out so that child classes can still reuse this
		 * functionality.
		 * 
		 * @param ctx
		 * @param event
		 *            The event to be encoded into {@link ByteBuf}. It will be
		 *            converted to 'opcode'-'payload' format.
		 * @return If only opcode is specified a single byte {@link ByteBuf} is
		 *         returned, otherwise a byte buf with 'opcode'-'payload' format is
		 *         returned.
		 */
        protected IByteBuffer Encode(IChannelHandlerContext ctx, IEvent @event)

        {
            IByteBuffer @out = ctx.Allocator.Buffer();
            @out.WriteByte((byte)@event.getType());
            if (Events.LOG_IN == @event.getType() || Events.RECONNECT == @event.getType())
            {
                // write protocol version also
                @out.WriteByte(Events.PROTCOL_VERSION);
            }

            if (null != @event.getSource())

            {


                MessageBuffer<IByteBuffer> msgBuffer = (MessageBuffer<IByteBuffer>)@event
                        .getSource();
                IByteBuffer data = msgBuffer.getNativeBuffer();
                @out.WriteBytes(data);
                data.Release();
            }
            return @out;
        }

        public override bool IsSharable => true;

    }

}
