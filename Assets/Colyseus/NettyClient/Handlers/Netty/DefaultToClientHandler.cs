using Assets.Colyseus.NettyClient.App;
using Coleseus.Shared.Event;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Coleseus.Shared.Handlers.Netty
{
    public class DefaultToClientHandler : SimpleChannelInboundHandler<IEvent>
    {
        const string NAME = "defaultHandler";
        private ISession session;


        public DefaultToClientHandler(ISession playerSession) : base()
        {

            this.session = playerSession;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, IEvent msg)
        {
            session.onEvent(msg);
        }


        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
        {
            Debug.LogError($"Exception during network communication: {exception}.");
            IEvent @event = Events.CreateEvent(exception, Events.EXCEPTION);
            session.onEvent(@event);
        }


        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            NettyTcpClient.ALL_CHANNELS.Add(ctx.Channel);
            base.ChannelActive(ctx);
        }


        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            if (!session.isShuttingDown)
            {
                // Should not send close to session, since reconnection/other
                // business logic might be in place.
                IEvent @event = Events.CreateEvent(null, Events.DISCONNECT);
                session.onEvent(@event);
            }
        }


        public ISession getPlayerSession()
        {
            return session;
        }

        public static String getName()
        {
            return NAME;
        }
    }

}
