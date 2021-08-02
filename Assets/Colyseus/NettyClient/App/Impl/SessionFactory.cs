using Assets.Colyseus.NettyClient.Handlers.Netty;
using Coleseus.Shared.Communication;
using Coleseus.Shared.Event;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.App.Impl
{
    /**
 * Class used to create a session in Nad client. SessionFactory will also create
 * the actual connection to the nadron server by initializing {@link NettyTCPClient}
 * and {@link NettyUDPClient} and using their connect methods.
 * 
 * @author Abraham Menacherry
 * 
 */
    public class SessionFactory
    {

        /**
		 * This class holds a number of variables like username, password etc which
		 * are necessary for creating connections to remote nadron server.
		 */
        private LoginHelper loginHelper;
        private NettyTcpClient tcpClient;
        private NettyUdpClient udpClient;
        private static int sessionId = 0;
        private readonly System.Object _lockObj = new System.Object();
        private ChannelInitializer<ISocketChannel> channelInitializer;

        /**
		 * This constructor will take a {@link LoginHelper} and initialize the
		 * {@link NettyTCPClient} and {@link NettyUDPClient}s using the connection
		 * parameters provided in this login helper class.
		 * 
		 * @param theLoginHelper
		 * @throws UnknownHostException
		 * @throws Exception
		 */
        public SessionFactory(LoginHelper theLoginHelper)

        {
            this.loginHelper = theLoginHelper;
            EndPoint tcpAddress = loginHelper.getTcpServerAddress();
            this.tcpClient = new NettyTcpClient(tcpAddress);
            EndPoint udpAddress = loginHelper.getUdpServerAddress();
            if (null == udpAddress)
            {
                udpClient = null;
            }
            else
            {
                udpClient = new NettyUdpClient((IPEndPoint)udpAddress,
                        UdpPipelineFactory.getInstance(udpAddress), null);
            }
        }

        /**
         * Creates a {@link Session} and connects it to the remote nadron server.
         * 
         * @return The session instance created and connected to remote nadron server.
         * @throws InterruptedException
         * @throws Exception
         */
        public ISession createAndConnectSession()


        {
            return createAndConnectSession((List<IEventHandler>)null);
        }

        /**
         * Creates a {@link Session}, adds the event handlers to the session and
         * then connects it to the remote nadron server. This way events will not be
         * lost on connect.
         * 
         * @param eventHandlers
         *            The handlers to be added to listen to session.
         * @return The session instance created and connected to remote nadron server.
         * @throws InterruptedException
         * @throws Exception
         */
        public ISession createAndConnectSession(List<IEventHandler> eventHandlers)

        {
            ISession session = createSession();
            connectSession(session, eventHandlers);
            return session;
        }

        /**
         * @return Returns the session instance created using a
         *         {@link SessionBuilder}.
         */
        public ISession createSession()
        {
            SessionBuilder sessionBuilder = new SessionBuilder().setId(Interlocked.Increment(ref sessionId));
            return sessionBuilder.build();
        }

        /**
         * Connects the session to remote nadron server. Depending on the connection
         * parameters provided to LoginHelper, it can connect both TCP and UDP
         * transports.
         * 
         * @param session
         *            The session to be connected to remote nadron server.
         * @throws InterruptedException
         * @throws Exception
         */
        public void connectSession(ISession session)

        {
            connectSession(session, (List<IEventHandler>)null);
        }

        /**
         * Connects the session to remote nadron server. Depending on the connection
         * parameters provided to LoginHelper, it can connect both TCP and UDP
         * transports.
         * 
         * @param session
         *            The session to be connected to remote nadron server.
         * @param eventHandlers
         *            The handlers to be added to session.
         * @throws InterruptedException
         * @throws Exception
         */
        public void connectSession(ISession session,
                List<IEventHandler> eventHandlers)
        {
            EndPoint udpAddress = null;
            if (null != udpClient)
            {
                udpAddress = doUdpConnection(session);
            }

            if (null != eventHandlers)
            {
                foreach (IEventHandler eventHandler in eventHandlers)
                {
                    session.addHandler(eventHandler);
                    if (eventHandler is SessionEventHandler)
                    {
                        ((SessionEventHandler)eventHandler).setSession(session);
                    }
                }
            }

            MessageBuffer<IByteBuffer> buffer = loginHelper
                    .getLoginBuffer((IPEndPoint)udpAddress);
            IEvent loginEvent = Events.CreateEvent(buffer, Events.LOG_IN);
            doTcpConnection(session, loginEvent);
        }

        /**
         * Method used to reconnect existing session which probably got disconnected
         * due to some exception. It will first close existing tcp and udp
         * connections and then try re-connecting using the reconnect key from
         * server.
         * 
         * @param session
         *            The session which needs to be re-connected.
         * @param reconnectKey
         *            This is provided by the server on
         *            {@link Events#GAME_ROOM_JOIN_SUCCESS} event and stored in the
         *            session.
         * @throws InterruptedException
         * @throws Exception
         */
        public void reconnectSession(ISession session, string reconnectKey)

        {
            session.tcpSender.close();
            if (null != session.udpSender)
                session.udpSender.close();

            EndPoint udpAddress = null;
            if (null != udpClient)
            {
                udpAddress = doUdpConnection(session);
            }

            IEvent reconnectEvent = Events.CreateEvent(
                            loginHelper.getReconnectBuffer(reconnectKey, (IPEndPoint)udpAddress),
                            Events.RECONNECT);

            doTcpConnection(session, reconnectEvent);
        }

        protected void doTcpConnection(ISession session, IEvent @event)

        {
            // This will in turn invoke the startEventHandler when server sends
            // Events.START event.
            IChannel channel = tcpClient.connect(getTcpPipelineFactory(session), @event);
            if (null != channel)
            {
                Reliable tcpMessageSender = new NettyTCPMessageSender(channel);
                session.tcpSender = (tcpMessageSender);
            }
            else
            {
                throw new Exception("Could not create TCP connection to server");
            }
        }

        /**
         * Return the pipeline factory or create the default messagebufferprotocol.
         * 
         * @param session
         *            The final handler in the protocol chain needs the session so
         *            that it can send messages to it.
         * @return
         */
        protected ChannelInitializer<ISocketChannel> getTcpPipelineFactory(
                 ISession session)
        {
            lock (_lockObj)
            {
                if (null == channelInitializer)
                {
                    channelInitializer = new TcpPipelineFactory(session);
                }
                return channelInitializer;
            }

        }

        /**
         * Set the channel initializer. This will be used when connecting the session.
         * @param channelInitializer
         */
        protected void setTCPChannelInitializer(
                ChannelInitializer<ISocketChannel> channelInitializer)
        {
            lock (_lockObj)
            {
                this.channelInitializer = channelInitializer;
            }

        }

        protected EndPoint doUdpConnection(ISession session)
        {

            EndPoint localAddress;
            IDatagramChannel datagramChannel = udpClient
                           .createDatagramChannel();
            localAddress = datagramChannel.LocalAddress;
            // Add a start event handler to the session which will send the udp
            // connect on server START signal.
            IEventHandler startEventHandler = new StartEventHandler(udpClient, session, datagramChannel);
            session.addHandler(startEventHandler);
            Fast udpMessageSender = new NettyUDPMessageSender(
                    udpClient.getServerAddress(), datagramChannel);
            session.udpSender = (udpMessageSender);
            return localAddress;
        }

        public class StartEventHandler : IEventHandler
        {
            private NettyUdpClient udpClient;
            private ISession session;
            private IDatagramChannel datagramChannel;
            public StartEventHandler(NettyUdpClient udpClient, ISession session, IDatagramChannel datagramChannel)
            {
                this.udpClient = udpClient;
                this.session = session;
                this.datagramChannel = datagramChannel;
            }

            public void onEvent(IEvent @event)
            {
                try
                {
                    udpClient.connect(session, datagramChannel);
                    // remove after use
                    session.removeHandler(this);
                }
                catch (InvalidOperationException e)
                {
                    throw new Exception(e.Message);
                }
                catch (ThreadInterruptedException e)
                {
                    throw new Exception(e.Message);
                }
            }


            public int getEventType()
            {
                return Events.START;
            }
        }

        public IPlayerSession createPlayerSession(IPlayer player)
        {
            SessionBuilder sessionBuilder = new SessionBuilder();
            DefaultPlayerSession playerSession = new DefaultPlayerSession(
                    sessionBuilder, player);
            return playerSession;
        }

        public LoginHelper getLoginHelper()
        {
            return loginHelper;
        }

        public NettyTcpClient getTcpClient()
        {
            return tcpClient;
        }

        public NettyUdpClient getUdpClient()
        {
            return udpClient;
        }

        public void setLoginHelper(LoginHelper loginHelper)
        {
            this.loginHelper = loginHelper;
        }

    }
}
