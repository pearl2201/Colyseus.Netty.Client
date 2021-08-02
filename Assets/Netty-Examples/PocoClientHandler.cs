using DotNetty.Transport.Channels;
using Colyseus.Common;
using System;
using UnityEngine;

public class PersonClientHandler : SimpleChannelInboundHandler<Person>
{
    protected override void ChannelRead0(IChannelHandlerContext contex, Person person)
    {

        Debug.Log("Data returned from Server:");
        Debug.Log(person.ToString());

    }

    public override void ExceptionCaught(IChannelHandlerContext contex, Exception e)
    {

        Debug.Log(DateTime.Now.Millisecond);
        Debug.LogFormat("{0}", e.StackTrace);
        contex.CloseAsync();
    }
}

public class Person
{
    public string Name { get; set; }

    public int Age { get; set; }
}
