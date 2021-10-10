using Assets.Colyseus.NettyClient.Communication;
using Coleseus.Shared.Communication;
using Coleseus.Shared.Event;
using Coleseus.Shared.Event.Impl;
using Coleseus.Shared.Protocols;
using Colyseus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using IEventHandler = Coleseus.Shared.Event.IEventHandler;

namespace Assets.Colyseus.NettyClient.App.Impl
{
    /**
  * The default implementation of the session class. This class is responsible
  * for receiving and sending events. For receiving it uses the
  * {@link #onEvent(Event)} method and for sending it uses the
  * {@link EventDispatcher} fireEvent method. Resetting id of this class after
  * creation will throw {@link IllegalAccessException} since class variable is
  * final.
  * 
  * @author Abraham Menacherry
  * 
  */
    public class DefaultSession : ISession
    {
        /**
         * session id
         */
        protected Object id;
        /**
         * event dispatcher
         */
        public IEventDispatcher eventDispatcher { get; protected set; }

        /**
         * session parameters
         */
        protected Dictionary<string, System.Object> sessionAttributes;

        public DateTime creationTime { get; protected set; }

        public DateTime lastReadWriteTime { get; set; }

        protected bool isWriteable { get; set; }

        /**
         * Life cycle variable to check if the session is shutting down. If it is,
         * then no more incoming events will be accepted.
         */
        public bool isShuttingDown { get; protected set; }

        protected bool isUDPEnabled { get; set; }

        protected ReconnectPolicy reconnectPolicy;

        public Reliable tcpSender { get; set; }
        public Fast udpSender { get; set; }


        public DefaultSession(SessionBuilder sessionBuilder)
        {
            // validate variables and provide default values if necessary. Normally
            // done in the builder.build() method, but done here since this class is
            // meant to be overriden and this could be easier.
            sessionBuilder.validateAndSetValues();
            this.id = sessionBuilder.id;
            this.eventDispatcher = sessionBuilder.eventDispatcher;
            this.sessionAttributes = sessionBuilder.sessionAttributes;
            this.creationTime = sessionBuilder.creationTime;
            this.lastReadWriteTime = sessionBuilder.lastReadWriteTime;
            this.isWriteable = sessionBuilder.isWriteable;
            this.isShuttingDown = sessionBuilder.isShuttingDown;
            this.isUDPEnabled = sessionBuilder.isUDPEnabled;
            this.reconnectPolicy = sessionBuilder.reconnectPolicy;
        }



        public void onEvent(IEvent @event)

        {
            if (!isShuttingDown)
            {
                eventDispatcher.fireEvent(@event);
            }
        }


        public Object getId()
        {
            return id;
        }


        public void setId(Object id)
        {
            throw new Exception("id cannot be reset since it is a final variable. "
                            + "It is set at constuction time.");
        }


        public IEventDispatcher getEventDispatcher()
        {
            return eventDispatcher;
        }


        public void addHandler(IEventHandler eventHandler)
        {
            eventDispatcher.addHandler(eventHandler);
        }


        public void removeHandler(IEventHandler eventHandler)
        {
            eventDispatcher.removeHandler(eventHandler);
        }


        public List<IEventHandler> getEventHandlers(int eventType)
        {
            return eventDispatcher.getHandlers(eventType);
        }


        public Object getAttribute(string key)
        {
            return sessionAttributes[key];
        }


        public void removeAttribute(string key)
        {
            sessionAttributes.Remove(key);
        }


        public void setAttribute(string key, Object value)
        {
            sessionAttributes.Add(key, value);
        }


        public void close()
        {
            isShuttingDown = true;
            eventDispatcher.close();
            if (null != tcpSender)
            {
                tcpSender.close();
                tcpSender = null;
            }
            if (null != udpSender)
            {
                udpSender.close();
                udpSender = null;
            }
        }


        public void reconnect(LoginHelper loginHelper)
        {
            String reconnectKey = (string)sessionAttributes[Config.RECONNECT_KEY];
            if (null != reconnectKey)
            {
                try
                {
                    new SessionFactory(loginHelper).reconnectSession(this, reconnectKey);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }




        public Dictionary<String, Object> getSessionAttributes()
        {
            return sessionAttributes;
        }




        public void resetProtocol(IProtocol protocol)
        {
            protocol.applyProtocol(this);
        }


        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((id == null) ? 0 : id.GetHashCode());
            return result;
        }


        public override bool Equals(Object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            DefaultSession other = (DefaultSession)obj;
            if (id == null)
            {
                if (other.id != null)
                    return false;
            }
            else if (!id.Equals(other.id))
                return false;
            return true;
        }

        public ReconnectPolicy getReconnectPolicy()
        {
            return reconnectPolicy;
        }

        public void setReconnectPolicy(ReconnectPolicy reconnectPolicy)
        {
            this.reconnectPolicy = reconnectPolicy;
        }

        bool ISession.isWriteable { get; set; }
        bool ISession.isUDPEnabled { get; set; }



    }

    /**
       * This class is roughly based on Joshua Bloch's Builder pattern. Since
       * Session class will be extended by child classes, the
       * {@link #validateAndSetValues()} method on this builder is actually called
       * by the {@link DefaultSession} constructor for ease of use. May not be good
       * design though.
       * 
       * @author Abraham, Menacherry
       * 
       */
    public class SessionBuilder
    {
        public Object id = null;
        public IEventDispatcher eventDispatcher = null;
        public Dictionary<string, System.Object> sessionAttributes = null;
        public DateTime creationTime { get; set; }
        public DateTime lastReadWriteTime { get; set; }
        public bool isWriteable { get; set; }
        public volatile bool isShuttingDown = false;
        public bool isUDPEnabled { get; set; } = false;// By default UDP is not enabled.
        public ReconnectPolicy reconnectPolicy = null;

        public ISession build()
        {
            return new DefaultSession(this);
        }

        /**
         * This method is used to validate and set the variables to default
         * values if they are not already set before calling build. This method
         * is invoked by the constructor of SessionBuilder. <b>Important!</b>
         * Builder child classes which override this method need to call
         * super.validateAndSetValues(), otherwise you could get runtime NPE's.
         */
        public void validateAndSetValues()
        {
            if (null == eventDispatcher)
            {
                eventDispatcher = new DefaultEventDispatcher();
            }
            if (null == sessionAttributes)
            {
                sessionAttributes = new Dictionary<string, System.Object>();
            }
            if (null == reconnectPolicy)
            {
                reconnectPolicy = ReconnectPolicy.NO_RECONNECT;
            }
            creationTime = DateTime.UtcNow;
        }

        public Object getId()
        {
            return id;
        }

        public SessionBuilder setId(Object id)
        {
            this.id = id;
            return this;
        }

        public SessionBuilder setEventDispatcher(
                 IEventDispatcher eventDispatcher)
        {
            this.eventDispatcher = eventDispatcher;
            return this;
        }

        public SessionBuilder setAessionAttributes(
                 Dictionary<string, System.Object> sessionAttributes)
        {
            this.sessionAttributes = sessionAttributes;
            return this;
        }

        public SessionBuilder setCreationTime(DateTime creationTime)
        {
            this.creationTime = creationTime;
            return this;
        }

        public SessionBuilder setLastReadWriteTime(DateTime lastReadWriteTime)
        {
            this.lastReadWriteTime = lastReadWriteTime;
            return this;
        }

        public SessionBuilder setWriteable(bool isWriteable)
        {
            this.isWriteable = isWriteable;
            return this;
        }

        public SessionBuilder setShuttingDown(bool isShuttingDown)
        {
            this.isShuttingDown = isShuttingDown;
            return this;
        }

        public SessionBuilder setUdpEnabled(bool isUDPEnabled)
        {
            this.isUDPEnabled = isUDPEnabled;
            return this;
        }

        public SessionBuilder setReconnectPolicy(ReconnectPolicy reconnectPolicy)
        {
            this.reconnectPolicy = reconnectPolicy;
            return this;
        }
    }

}
