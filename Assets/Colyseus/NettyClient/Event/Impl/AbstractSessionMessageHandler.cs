using Assets.Colyseus.NettyClient.App;
using Coleseus.Shared.Communication;
using DotNetty.Buffers;
using System;
using UnityEngine;

namespace Coleseus.Shared.Event.Impl
{
    /**
 * This abstract helper class can be used to quickly create a listener which
 * listens for SESSION_MESSAGE @events. Child classes need to override the
 * onEvent to plugin the logic.
 * 
 * @author Abraham Menacherry
 * 
 */
    public abstract class AbstractSessionEventHandler : SessionEventHandler
    {
        private readonly System.Object _lockObj = new System.Object();
        protected const int @eventType = Events.ANY;

        protected ISession session;

        protected volatile bool isReconnecting = false;

        public AbstractSessionEventHandler()
        {
        }

        public AbstractSessionEventHandler(ISession session)
        {
            this.session = session;
        }


        public int getEventType()
        {
            return @eventType;
        }


        public void onEvent(IEvent @event)
        {
            doEventHandlerMethodLookup(@event);
        }

        public void doEventHandlerMethodLookup(IEvent @event)
        {
            int @eventType = @event.getType();
            switch (eventType)
            {
                case Events.SESSION_MESSAGE:
                    onDataIn(@event);
                    break;
                case Events.NETWORK_MESSAGE:
                    onNetworkMessage((INetworkEvent)@event);
                    break;
                case Events.LOG_IN_SUCCESS:
                    onLoginSuccess(@event);
                    break;
                case Events.LOG_IN_FAILURE:
                    onLoginFailure(@event);
                    break;
                case Events.START:
                    onStart(@event);
                    break;
                case Events.STOP:
                    onStart(@event);
                    break;
                case Events.GAME_ROOM_JOIN_SUCCESS:
                    onGameRoomJoin(@event);
                    break;
                case Events.CONNECT_FAILED:
                    onConnectFailed(@event);
                    break;
                case Events.DISCONNECT:
                    onDisconnect(@event);
                    break;
                case Events.CHANGE_ATTRIBUTE:
                    onChangeAttribute(@event);
                    break;
                case Events.EXCEPTION:
                    onException(@event);
                    break;
                default:
                    onCustomEvent(@event);
                    break;
            }
        }

        public abstract void onDataIn(IEvent @event);

        public void onNetworkMessage(INetworkEvent networkEvent)
        {
            ISession session = getSession();
            bool writeable = session.isWriteable;
            MessageSender messageSender = null;
            if (networkEvent.getDeliveryGuaranty() == DeliveryGuaranty.FAST)
            {
                messageSender = session.udpSender;
            }
            else
            {
                messageSender = session.tcpSender;
            }
            if (writeable && null != networkEvent)
            {
                messageSender.sendMessage(networkEvent);
            }
        }

        public void onLoginSuccess(IEvent @event)
        {
        }

        public virtual void onGameRoomJoin(IEvent @event)
        {
            if (null != @event.getSource()
                    && (@event.getSource() is MessageBuffer<IByteBuffer>))
            {


                String reconnectKey = ((MessageBuffer<IByteBuffer>)@event
                        .getSource()).readString();
                if (null != reconnectKey)
                    getSession().setAttribute(Config.RECONNECT_KEY, reconnectKey);
            }
        }

        public void onLoginFailure(IEvent @event)
        {
        }

        public void onStart(IEvent @event)
        {
            isReconnecting = false;
            getSession().isWriteable = true;
        }

        public void onStop(IEvent @event)
        {
            getSession().isWriteable = false;
        }

        public void onConnectFailed(IEvent @event)
        {

        }

        public void onDisconnect(IEvent @event)
        {
            //onException(event);
        }

        public void onChangeAttribute(IEvent @event)
        {

        }

        public void onException(IEvent @event)
        {
            lock (_lockObj)
            {
                ISession session = getSession();
                String reconnectKey = (String)session
                        .getAttribute(Config.RECONNECT_KEY);
                if (null != reconnectKey)
                {
                    if (isReconnecting)
                    {
                        return;
                    }
                    else
                    {
                        isReconnecting = true;
                    }
                    session.isWriteable = false;
                    if (null != session.getReconnectPolicy())
                    {
                        session.getReconnectPolicy().ApplyPolicy(session);
                    }
                    else
                    {
                        Debug.LogError("Received exception @event in session. "
                                + "Going to close session");
                        onClose(@event);
                    }
                }
                else
                {
                    Debug.LogError("Received exception @event in session. "
                            + "Going to close session");
                    onClose(@event);
                }
            }
        }

        public void onClose(IEvent @event)
        {
            getSession().close();
        }

        public void onCustomEvent(IEvent @event)
        {

        }


        public ISession getSession()
        {
            return session;
        }


        public void setSession(ISession session)
        {
            this.session = session;
        }

    }

}
