using Assets.Colyseus.NettyClient.App;
using Coleseus.Shared.Handlers.Netty;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.Handlers.Netty
{
    public class TcpPipelineFactory : ChannelInitializer<ISocketChannel>
    {
        /**
	 * Prepends the length of transmitted message before sending to remote
	 * nadron server.
	 */
        private LengthFieldPrepender LENGTH_FIELD_PREPENDER = new LengthFieldPrepender(
            2);
        /**
		 * Decodes incoming messages from remote nadron server to {@link MessageBuffer}
		 * type, puts this as the payload for an {@link Event} and passes this
		 * {@link Event} instance to the next decoder/handler in the chain.
		 */
        private MessageBufferEventEncoder EVENT_ENCODER = new MessageBufferEventEncoder();
        /**
		 * Used to transmit the message to {@link Session}.
		 */
        private DefaultToClientHandler defaultToClientHandler;

        public TcpPipelineFactory(ISession session)
        {
            this.defaultToClientHandler = new DefaultToClientHandler(session);
        }


        protected override void InitChannel(ISocketChannel ch)
        {
            IChannelPipeline pipeline = ch.Pipeline;
            pipeline.AddLast("lengthDecoder", new LengthFieldBasedFrameDecoder(
                    int.MaxValue, 0, 2, 0, 2));
            pipeline.AddLast("eventDecoder", new MessageBufferEventDecoder());
            pipeline.AddLast(DefaultToClientHandler.getName(),
                    defaultToClientHandler);

            // Down stream handlers are added now. Note that the last one added to
            // pipeline is actually the first encoder in the pipeline.
            pipeline.AddLast("lengthFieldPrepender", LENGTH_FIELD_PREPENDER);
            pipeline.AddLast("eventEncoder", EVENT_ENCODER);

        }
    }
}
