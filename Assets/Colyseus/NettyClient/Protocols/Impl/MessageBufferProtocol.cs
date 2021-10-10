﻿using Assets.Colyseus.NettyClient.App;
using Assets.Colyseus.NettyClient.Protocols;
using Coleseus.Shared.Handlers.Netty;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace Coleseus.Shared.Protocols.Impl
{
    public class MessageBufferProtocol : AbstractNettyProtocol
    {
        string protocolName;

        /**
         * Utility handler provided by netty to add the length of the outgoing
         * message to the message as a header.
         */
        private LengthFieldPrepender lengthFieldPrepender;
        private MessageBufferEventDecoder messageBufferEventDecoder;
        private MessageBufferEventEncoder messageBufferEventEncoder;

        public MessageBufferProtocol() : base("MESSAGE_BUFFER_PROTOCOL")
        {
            this.protocolName = "MESSAGE_BUFFER_PROTOCOL";
        }

        public override void applyProtocol(ISession playerSession)
        {

            IChannelPipeline pipeline = NettyUtils
                    .getPipeLineOfConnection((ISession)playerSession);
            // Upstream handlers or encoders (i.e towards server) are added to
            // pipeline now.
            pipeline.AddLast("lengthDecoder", createLengthBasedFrameDecoder());
            pipeline.AddLast("messageBufferEventDecoder", messageBufferEventDecoder);
            pipeline.AddLast("eventHandler", new DefaultToClientHandler((ISession)
                    playerSession));

            // Downstream handlers - Filter for data which flows from server to
            // client. Note that the last handler added is actually the first
            // handler for outgoing data.
            pipeline.AddLast("lengthFieldPrepender", lengthFieldPrepender);
            pipeline.AddLast("messageBufferEventEncoder", messageBufferEventEncoder);

        }

        public LengthFieldPrepender getLengthFieldPrepender()
        {
            return lengthFieldPrepender;
        }

        public void setLengthFieldPrepender(LengthFieldPrepender lengthFieldPrepender)
        {
            this.lengthFieldPrepender = lengthFieldPrepender;
        }

        public MessageBufferEventDecoder getMessageBufferEventDecoder()
        {
            return messageBufferEventDecoder;
        }

        public void setMessageBufferEventDecoder(
                MessageBufferEventDecoder messageBufferEventDecoder)
        {
            this.messageBufferEventDecoder = messageBufferEventDecoder;
        }

        public MessageBufferEventEncoder getMessageBufferEventEncoder()
        {
            return messageBufferEventEncoder;
        }

        public void setMessageBufferEventEncoder(
                MessageBufferEventEncoder messageBufferEventEncoder)
        {
            this.messageBufferEventEncoder = messageBufferEventEncoder;
        }

     
    }

}
