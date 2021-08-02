using Assets.Colyseus.NettyClient.App;
using Coleseus.Shared.Event;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

public class NettyUdpClient : IDisposable
{
    /**
	  * The remote server address to which this client should connect.
	  */
    private readonly IPEndPoint serverAddress;
    /**
	 * The boss executor which will provide threads to Netty
	 * {@link ChannelFactory} for reading from the NIO selectors.
	 */
    private readonly IEventLoopGroup boss;
    /**
	 * For UDP there can only be one pipelineFactory per
	 * {@link Bootstrap}. This factory is hence part of the client
	 * class.
	 */
    private readonly ChannelInitializer<IDatagramChannel> pipelineFactory;

    /**
	 * This map is used to store the local address to which a session has bound
	 * itself using the {@link DatagramChannel#bind(java.net.SocketAddress)}
	 * method. When an incoming UDP packet is recieved the
	 * {@link UDPUpstreamHandler} will resolve which session to pass the event,
	 * using this map.
	 */
    public static readonly Dictionary<EndPoint, ISession> CLIENTS = new Dictionary<EndPoint, ISession>();

    /**
	 * Creates an instance of a Netty UDP client which can then be used to
	 * connect to a remote Nadron server. This constructor delegates to
	 * {@link #NettyUDPClient(IPEndPoint, ChannelInitializer, string)}
	 * constructor after creating a {@link IPEndPoint} instance based on
	 * the host and port number passed in.
	 * 
	 * @param nadronHost
	 *            The host name of the remote server on which nadron server is
	 *            running.
	 * @param port
	 *            The port to connect to, on the remote server.
	 * @param pipelineFactory
	 *            The pipeline factory to be used while creating a Netty
	 *            {@link Channel}
	 * @throws UnknownHostException
	 * @throws Exception
	 */
    public NettyUdpClient(string nadronHost, int port,
             ChannelInitializer<IDatagramChannel> pipelineFactory) : this(new IPEndPoint(IPAddress.Parse(nadronHost), port), pipelineFactory, null)
    {

    }

    /**
     * Creates a new instance of the {@link NettyUDPClient}. It actually
     * delegates to
     * {@link #NettyUDPClient(IPEndPoint, ChannelInitializer, EventLoopGroup, string)}
     * . It will internally instantiate the {@link EventLoopGroup}.
     * 
     * @param serverAddress
     *            The remote servers address. This address will be used when any
     *            of the default write/connect methods are used.
     * @param pipelineFactory
     *            The Netty factory used for creating a pipeline. For UDP, this
     *            pipeline factory should not have any stateful i.e non
     *            share-able handlers in it. Since Netty only has one channel
     *            for <b>ALL</b> UPD traffic.
     * @param localhostName
     *            Name of the host to which this client is to be bound.
     *            Generally localhost. If null, then
     *            <code>InetAddress.getLocalHost().getHostAddress()</code> is
     *            used internally by default.
     * @throws UnknownHostException
     *             , Exception
     */
    public NettyUdpClient(IPEndPoint serverAddress,
             ChannelInitializer<IDatagramChannel> pipelineFactory, string localhostName) : this(serverAddress, pipelineFactory, new SingleThreadEventLoop(), localhostName)
    {

    }

    /**
     * Creates a new instance of the {@link NettyUDPClient}.
     * 
     * @param serverAddress
     *            The remote servers address. This address will be used when any
     *            of the default write/connect methods are used.
     * @param pipelineFactory
     *            The Netty factory used for creating a pipeline. For UDP, this
     *            pipeline factory should not have any stateful i.e non
     *            share-able handlers in it. Since Netty only has one channel
     *            for <b>ALL</b> UPD traffic.
     * @param boss
     *            The {@link EventLoopGroup} used for creating boss threads.
     * @param localhostName
     *            Name of the host to which this client is to be bound.
     *            Generally localhost. If null, then
     *            <code>InetAddress.getLocalHost().getHostAddress()</code> is
     *            used internally by default.
     * @throws UnknownHostException
     */
    public NettyUdpClient(IPEndPoint serverAddress,
             ChannelInitializer<IDatagramChannel> pipelineFactory,
             IEventLoopGroup boss, string localhostName)
    {
        this.boss = boss;
        this.serverAddress = serverAddress;
        this.pipelineFactory = pipelineFactory;
        if (null == localhostName)
        {
            localhostName = Dns.GetHostName();
        }

    }
    public void Dispose()
    {
        boss.ShutdownGracefullyAsync().Wait();
    }

    /**
     * This method will connect the datagram channel with the server and send
     * the {@link Events#CONNECT} message to server. This method will use
     * {@link #serverAddress} by default when sending the
     * {@link Events#CONNECT} message. <b>Note</b> Even if this connect
     * message does not reach server, the first UDP message that the server
     * receives from this particular DatagramChannels local address will be
     * converted by server and used as {@link Events#CONNECT}.
     * 
     * @param session
     *            The session for which the datagram channel is being created.
     * @param datagramChannel
     *            The channel on which the message is to be sent to remote
     *            server.
     * @return Returns a ChannelFuture which can be used to check the success of
     *         this operation. <b>NOTE</b> Success in case of UDP means message
     *         is sent to server. It does not mean that the server has received
     *         it.
     * @throws UnknownHostException
     */
    public Task connect(ISession session,
                IDatagramChannel datagramChannel)
    {
        return connect(session, datagramChannel, this.serverAddress, TimeSpan.FromSeconds(5));
    }

    /**
     * This method delegates to {@link #createDatagramChannel(string)}
     * internally, by passing the localhost's host name to it.
     * 
     * @return The newly created instance of the datagram channel.
     * @throws UnknownHostException
     *             , InterruptedException
     */
    public IDatagramChannel createDatagramChannel()
    {
        return createDatagramChannel(Dns.GetHostName());
    }

    /**
     * Creates a new datagram channel instance using the
     * {@link NioDatagramChannel} by binding to local host.
     * 
     * @param localhostName
     *            The host machine (for e.g. 'localhost') to which it needs to
     *            bind to. This is <b>Not</b> the remote Nadron server hostname.
     * @return The newly created instance of the datagram channel.
     * @throws UnknownHostException
     */
    public IDatagramChannel createDatagramChannel(string localhostName)

    {
        Bootstrap udpBootstrap = new Bootstrap();
        udpBootstrap.Group(boss).Channel<SocketDatagramChannel>()
                .Option(ChannelOption.SoBroadcast, true)
                .Handler(pipelineFactory);

        IDatagramChannel datagramChannel = (IDatagramChannel)udpBootstrap
                .BindAsync(new IPEndPoint(IPAddress.Parse(localhostName), 0)).Result;
        return datagramChannel;
    }

    /**
     * This method will connect the datagram channel with the server and send
     * the {@link Events#CONNECT} message to server.
     * 
     * @param session
     *            The session for which the datagram channel is being created.
     * @param datagramChannel
     *            The channel on which the message is to be sent to remote
     *            server.
     * @param serverAddress
     *            The remote address of the server to which to connect.
     * @param timeout
     *            Amount of time to wait for the connection to happen.
     *            <b>NOTE</b> Since this is UDP there is actually no "real"
     *            connection.
     * @return Returns a ChannelFuture which can be used to check the success of
     *         this operation. <b>NOTE</b> Success in case of UDP means message
     *         is sent to server. It does not mean that the server has received
     *         it.
     * @throws UnknownHostException
     */
    public Task connect(ISession session,
            IDatagramChannel datagramChannel, IPEndPoint serverAddress,
            TimeSpan timeout)
    {
        if (null == datagramChannel)
        {
            throw new ArgumentException(
                    "DatagramChannel passed to connect method cannot be null");
        }
        if (!datagramChannel.Active)
        {
            throw new InvalidOperationException("DatagramChannel: "
                    + datagramChannel
                    + " Passed to connect method is not bound");
        }

        IEvent @event = Events.CreateEvent(null, Events.CONNECT);

        Task future = datagramChannel.WriteAndFlushAsync(@event).ContinueWith(x =>
        {
            if (x.IsCanceled || x.IsFaulted)
            {
                throw new Exception(x.Exception.ToString());
            }
        });

        CLIENTS.Add((IPEndPoint)datagramChannel.LocalAddress, session);
        return future;
    }

    /**
     * Utility method used to send a message to the server. Users can also use
     * datagramChannel.write(message, serverAddress) directly.
     * 
     * @param datagramChannel
     *            The channel on which the message is to be sent to remote
     *            server.
     * @param message
     *            The message to be sent. <b>NOTE</b> The message should be a
     *            valid and encode-able by the encoders in the ChannelPipeline
     *            of this server.
     * @return Returns a ChannelFuture which can be used to check the success of
     *         this operation. <b>NOTE</b> Success in case of UDP means message
     *         is sent to server. It does not mean that the server has received
     *         it.
     */
    public static Task write(IDatagramChannel datagramChannel, System.Object message)
    {
        return datagramChannel.WriteAsync(message);
    }

    public IPEndPoint getServerAddress()
    {
        return serverAddress;
    }

    public IEventLoopGroup getBoss()
    {
        return boss;
    }

    public ChannelInitializer<IDatagramChannel> getPipelineFactory()
    {
        return pipelineFactory;
    }
}
