using Assets.Colyseus.NettyClient.App;
using Assets.Colyseus.NettyClient.Protocols;
using Coleseus.Shared.Handlers.Netty;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace Coleseus.Shared.Protocols.Impl
{
    public class ChannelBufferProtocol : AbstractNettyProtocol
    {
        

      
        /**
         * Utility handler provided by netty to add the length of the outgoing
         * message to the message as a header.
         */
        private LengthFieldPrepender lengthFieldPrepender;
        private EventDecoder eventDecoder;
        private EventEncoder eventEncoder;

        public ChannelBufferProtocol() : base("CHANNEL_BUFFER_PROTOCOL")
        {

        }



        public override void applyProtocol(ISession playerSession)
        {
            IChannelPipeline pipeline = NettyUtils
                  .getPipeLineOfConnection((ISession)playerSession);
            // Upstream handlers or encoders (i.e towards server) are added to
            // pipeline now.
            pipeline.AddLast("lengthDecoder", createLengthBasedFrameDecoder());
            pipeline.AddLast("eventDecoder", eventDecoder);
            pipeline.AddLast("eventHandler", new DefaultToClientHandler(
                    (ISession)playerSession));

            // Downstream handlers - Filter for data which flows from server to
            // client. Note that the last handler added is actually the first
            // handler for outgoing data.
            pipeline.AddLast("lengthFieldPrepender", lengthFieldPrepender);
            pipeline.AddLast("eventEncoder", eventEncoder);
        }

        public LengthFieldPrepender getLengthFieldPrepender()
        {
            return lengthFieldPrepender;
        }

        public void setLengthFieldPrepender(LengthFieldPrepender lengthFieldPrepender)
        {
            this.lengthFieldPrepender = lengthFieldPrepender;
        }

        public EventDecoder getEventDecoder()
        {
            return eventDecoder;
        }

        public void setEventDecoder(EventDecoder eventDecoder)
        {
            this.eventDecoder = eventDecoder;
        }

        public EventEncoder getEventEncoder()
        {
            return eventEncoder;
        }

        public void setEventEncoder(EventEncoder eventEncoder)
        {
            this.eventEncoder = eventEncoder;
        }


    }
}
