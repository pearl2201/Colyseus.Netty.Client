using Assets.Colyseus.NettyClient.Communication;
using Coleseus.Shared.Communication;
using Coleseus.Shared.Event;
using Coleseus.Shared.Protocols;
using Colyseus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.App
{
    public interface ISession
    {
        Object getId();

        void setId(Object id);

        void setAttribute(String key, Object value);

        Object getAttribute(String key);

        void removeAttribute(String key);

        void onEvent(IEvent @event);

        IEventDispatcher eventDispatcher { get; }


        bool isWriteable { get; set; }

        /**
		 * A session would not have UDP capability when created. Depending on the
		 * network abilities of the client, it can request UDP communication to be
		 * enabled with the LOGIN_UDP and CONNECT_UDP events of the {@link Events}
		 * class. Once UDP is enabled this flag will be set to true on the session.
		 * 
		 * @return Returns true if the a UDP {@link MessageSender} instance is
		 *         attached to this session, else false.
		 */
        bool isUDPEnabled { get; set; }

        bool isShuttingDown { get; }

        DateTime creationTime { get; }

        DateTime lastReadWriteTime { get; }





 

        void addHandler(IEventHandler eventHandler);

        void removeHandler(IEventHandler eventHandler);

        List<IEventHandler> getEventHandlers(int eventType);

        void close();

      

        Fast udpSender { get; set; }

   

        Reliable tcpSender { get; set; }

        /**
 * Implementations will generally clear the internal netty pipeline and
 * apply new set of handlers
 * 
 * @param protocol
 */
        void resetProtocol(IProtocol protocol);

        void reconnect(LoginHelper loginHelper);

        void setReconnectPolicy(ReconnectPolicy reconnectPolicy);

        ReconnectPolicy getReconnectPolicy();
    }
}
