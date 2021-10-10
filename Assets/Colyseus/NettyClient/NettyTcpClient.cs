using Coleseus.Shared.Event;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NettyTcpClient : IDisposable
{
    private readonly System.Object _lockObj = new System.Object();

    /**
	  * The remote server address to which this client should connect.
	  */
    private EndPoint serverAddress;
    /**
	 * The boss executor which will provide threads to Netty
	 * {@link ChannelFactory} for reading from the NIO selectors.
	 */
    private IEventLoopGroup boss;
    private Bootstrap bootstrap;
    /**
	 * The amount of time in seconds to wait for this client to close all
	 * {@link Channel}s and shutdown gracefully.
	 */
    private int maxShutdownWaitTime;
    /**
	 * Any successful TCP connection opened by the client to server is also
	 * added to this {@link ChannelGroup}. This will be used for shutting down
	 * the client gracefully.
	 */
    public static IChannelGroup ALL_CHANNELS = new DefaultChannelGroup(
            "NAD-CLIENT-CONNECTIONS", new SingleThreadEventExecutor("TcpNettyServer", TimeSpan.FromSeconds(10)));

    /**
	 * Creates an instance of a Netty TCP client which can then be used to
	 * connect to a remote Nadron server. This constructor delegates to
	 * {@link #NettyTCPClient(EndPoint)} constructor after creating a
	 * {@link EndPoint} instance based on the host and port number
	 * passed in.
	 * 
	 * @param nadronHost
	 *            The host name of the remote server on which nadron server is
	 *            running.
	 * @param port
	 *            The port to connect to, on the remote server.
	 */
    public NettyTcpClient(string nadronHost, int port) : this(new IPEndPoint(IPAddress.Parse(nadronHost), port))
    {

    }

    public NettyTcpClient(EndPoint serverAddress) : this(serverAddress, new SingleThreadEventLoop(), 5000)
    {

    }

    /**
	 * Creates a new instance of the {@link NettyTCPClient}. This constructor
	 * also registers a shutdown hook which will call close on
	 * {@link #ALL_CHANNELS} and call bootstrap.releaseExternalResources() to
	 * enable a graceful shutdown.
	 * 
	 * @param serverAddress
	 *            The remote servers address. This address will be used when any
	 *            of the default write/connect methods are used.
	 * @param boss
	 *            {@link EventLoopGroup} used for client.
	 * @param maxShutdownWaitTime
	 *            The amount of time in seconds to wait for this client to close
	 *            all {@link Channel}s and shutdown gracefully.
	 */
    public NettyTcpClient(EndPoint serverAddress,
             IEventLoopGroup boss,
             int maxShutdownWaitTime)
    {
        this.serverAddress = serverAddress;
        this.boss = boss;
        this.bootstrap = new Bootstrap();
        bootstrap.Group(boss).Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.SoKeepalive, true);
        this.maxShutdownWaitTime = maxShutdownWaitTime;

    }

    public void Dispose()
    {
        try
        {
            ALL_CHANNELS.CloseAsync().ContinueWith(async (x) =>
            {
                Thread.Sleep(maxShutdownWaitTime);
            }).Wait();

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        boss.ShutdownGracefullyAsync().Wait();
    }

    /**
     * This method delegates to the
     * {@link #connect(ChannelInitializer, Event, int, TimeUnit)} method
     * internally. It will pass in a default of 5 seconds wait time to the
     * delegated method.
     * 
     * @param pipelineFactory
     *            The factory used to create a pipeline of decoders and encoders
     *            for each {@link Channel} that it creates on connection.
     * @param loginEvent
     *            The event contains the {@link ByteBuf} to be transmitted
     *            to nadron server for logging in. Values inside this buffer include
     *            username, password, connection key, <b>optional</b> local
     *            address of the UDP channel used by this session.
     * @return Returns the Netty {@link Channel} which is the connection to the
     *         remote nadron server.
     * @throws InterruptedException
     */
    public Task<IChannel> connect(ChannelInitializer<ISocketChannel> pipelineFactory,
             IEvent loginEvent)
    {


        return connect(pipelineFactory, loginEvent, TimeSpan.FromSeconds(5));
    }

    /**
     * Method that is used to create the connection or {@link Channel} to
     * communicated with the remote nadron server.
     * 
     * @param pipelineFactory
     *            The factory used to create a pipeline of decoders and encoders
     *            for each {@link Channel} that it creates on connection.
     * @param loginEvent
     *            The event contains the {@link ByteBuf} to be transmitted
     *            to nadron server for logging in. Values inside this buffer include
     *            username, password, connection key, <b>optional</b> local
     *            address of the UDP channel used by this session.
     * @param timeout
     *            The amount of time to wait for this connection be created
     *            successfully.
     * @param unit
     *            The unit of timeout SECONDS, MILLISECONDS etc. Default is 5
     *            seconds.
     * @return Returns the Netty {@link Channel} which is the connection to the
     *         remote nadron server.
     * @throws InterruptedException
     */
    public Task<IChannel> connect(ChannelInitializer<ISocketChannel> pipelineFactory,
             IEvent loginEvent, TimeSpan time)


    {
        Task<IChannel> future;
        lock (bootstrap)
        {
            bootstrap.Handler(pipelineFactory);


            future = bootstrap.ConnectAsync(serverAddress);

            Debug.Log("bootstrap connect start");

            future.ContinueWith(x =>
            {
             
                if (x.IsCompleted)
                {
                    var result = x.Result;
                    result.WriteAndFlushAsync(loginEvent).Wait();
                    Debug.Log("bootstrap connect complete");
                    return result;
                }
                else
                {
                 
                    Debug.LogError("bootstrap connect exception: " + x.Exception.Message);
                    return null;
                }

            });


        }

        return future;
    }

    public EndPoint getServerAddress()
    {
        return serverAddress;
    }

    public IEventLoopGroup getBoss()
    {
        return boss;
    }


    public Bootstrap getBootstrap()
    {
        return bootstrap;
    }

    public int getMaxShutdownWaitTime()
    {
        return maxShutdownWaitTime;
    }
}
