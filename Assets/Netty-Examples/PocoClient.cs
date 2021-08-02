using Colyseus.Common;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class PocoClient : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("StartConnect", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void StartConnect()
    {
        NettyClient client = new NettyClient();
        client.Run().Wait();
    }

}

public class NettyClient
{
    private IChannel bootstrapChannel;
    public async Task Run()
    {
        Debug.Log("NettyClient start");


        var group = new MultithreadEventLoopGroup();

        var serverIP = IPAddress.Parse("127.0.0.1");
        int serverPort = 8080;

        try
        {
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                            .Channel<TcpSocketChannel>()
                            .Option(ChannelOption.TcpNodelay, true) // Do not buffer and send packages right away
                            .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                            {
                                IChannelPipeline pipeline = channel.Pipeline;
                                pipeline.AddLast(new PersonEncoder(), new PersonDecoder(), new PersonClientHandler());

                            }));
            Debug.Log("NettyClient connect");
            bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(serverIP, serverPort));
            Debug.Log("NettyClient connected");

            {

                string name = "Ann";





                int age = 32;

                Debug.Log("NettyClient write");
                await bootstrapChannel.WriteAndFlushAsync(new Person() { Name = name, Age = age });
            }

           
        }
        finally
        {
            group.ShutdownGracefullyAsync().Wait(1000);
        }
    }

    public async Task Close()
    {
        await bootstrapChannel.CloseAsync();
    }

}
