using Assets.Colyseus.NettyClient.App;
using Assets.Colyseus.NettyClient.App.Impl;
using Coleseus.Shared.Event;
using Coleseus.Shared.Event.Impl;
using Colyseus.NettyServer.ZombieGame.Domain;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static LoginHelper;

namespace Assets.ZombieGame
{
    public class LoginTest : MonoBehaviour
    {

        public Button button;

  
        public async void Login()
        {
            LoginBuilder builder = new LoginBuilder().setUsername("user")
                 .setPassword("pass").setConnectionKey("Zombie_ROOM_1")
                 .setNadronTcpHostName("127.0.0.1").setTcpPort(18090);
                 //.setNadronUdpHostName("127.0.0.1").setUdpPort(18090);
            LoginHelper helper = builder.build();
            var sessionFactory = new SessionFactory(helper);
            ISession session = await sessionFactory.createAndConnectSession();
            addDefaultHandlerToSession(session);
            var iam = IAM.ZOMBIE;
            GameObject go = new GameObject();
            var task = go.AddComponent<GamePlay>();
            task.SetupGamePlay(iam, session);
        }

        private static void addDefaultHandlerToSession(ISession session)
        {
            // we are only interested in data in, so override only that method.
            AbstractSessionEventHandler handler = new GamePlaySessionHandler(session);
            session.addHandler(handler);
        }

        public class GamePlaySessionHandler : AbstractSessionEventHandler
        {
            public GamePlaySessionHandler(ISession session) : base(session) { }
            public override void onDataIn(IEvent @event)
            {
                NettyMessageBuffer buffer = (NettyMessageBuffer)@event.getSource();
                Debug.Log("Remaining Human Population: " + buffer.readInt());
            }
        }
    }
}
