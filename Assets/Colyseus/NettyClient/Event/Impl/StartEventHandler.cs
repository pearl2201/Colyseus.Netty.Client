using Assets.Colyseus.NettyClient.App;
using Coleseus.Shared.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.Event.Impl
{
    public abstract class StartEventHandler : SessionEventHandler
    {
        protected readonly ISession session;

        public StartEventHandler(ISession session)
        {
            this.session = session;
        }

        public ISession getSession()
        {
            return session;
        }

        public void setSession(ISession session)
        {
            throw new InvalidOperationException("Cannot set session again");
        }

        public abstract void onEvent(IEvent @event);

        public int getEventType()
        {
            return Events.START;
        }
    }
}
