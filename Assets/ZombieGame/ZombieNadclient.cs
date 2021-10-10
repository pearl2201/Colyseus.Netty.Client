using Assets.Colyseus.NettyClient.App;
using Assets.Colyseus.NettyClient.App.Impl;
using Coleseus.Shared.Event;
using Coleseus.Shared.Event.Impl;
using Colyseus.NettyServer.ZombieGame.Domain;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static LoginHelper;

namespace Assets.ZombieGame
{
    public class ZombieNadclient : MonoBehaviour
    {

        async UniTaskVoid Start()
        {
            LoginBuilder builder = new LoginBuilder().setUsername("user")
                .setPassword("pass").setConnectionKey("Zombie_ROOM_1")
                .setNadronTcpHostName("127.0.0.1").setTcpPort(18090);
                //.setNadronUdpHostName("255.255.255.255").setUdpPort(18090);
            SessionFactory sessionFactory = null;
            for (int i = 1; i <= 5; i++)
            {
                builder.setConnectionKey("Zombie_ROOM_" + i);
                LoginHelper loginHelper = builder.build();
                if (i == 1)
                {
                    sessionFactory = new SessionFactory(loginHelper);
                }
                else
                {
                    // no need to create session factory objects again.
                    sessionFactory.setLoginHelper(loginHelper);
                }

                for (int j = 1; j <= 20; j++)
                {
                    ISession session = await sessionFactory.createAndConnectSession();
                    addDefaultHandlerToSession(session);
                    var iam = i % 2 == 0 ? IAM.DEFENDER : IAM.ZOMBIE;
                    GameObject go = new GameObject();
                    var task = go.AddComponent<GamePlay>();
                    task.SetupGamePlay(iam, session);
                }
            }
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



