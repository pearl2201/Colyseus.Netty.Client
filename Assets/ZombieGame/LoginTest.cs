using Assets.Colyseus.NettyClient.App;
using Assets.Colyseus.NettyClient.App.Impl;
using Assets.Colyseus.NettyClient.Utils;
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
        private ISession session;
        private GamePlay gamePlay;


        public async void Login()
        {
            LoginBuilder builder = new LoginBuilder().setUsername("user")
                 .setPassword("pass").setConnectionKey("Zombie_ROOM_1")
                 .setNadronTcpHostName("127.0.0.1").setTcpPort(18090);
            //.setNadronUdpHostName("127.0.0.1").setUdpPort(18090);
            LoginHelper helper = builder.build();
            var sessionFactory = new SessionFactory(helper);
            session = await sessionFactory.createAndConnectSession();
            var iam = IAM.ZOMBIE;
            GameObject go = new GameObject("GamePlay");
            gamePlay = go.AddComponent<GamePlay>();
            var handler = addDefaultHandlerToSession(session, gamePlay);
            gamePlay.SetupGamePlay(iam, session);
            gamePlay.SetupHandler(handler);

        }


        private void OnApplicationQuit()
        {
            if (session != null)
                session.close();
        }
        private GamePlaySessionHandler addDefaultHandlerToSession(ISession session, GamePlay gameplay)
        {
            // we are only interested in data in, so override only that method.
            GamePlaySessionHandler handler = new GamePlaySessionHandler(session, gameplay);
            session.addHandler(handler);
            return handler;
        }

        public class GamePlaySessionHandler : AbstractSessionEventHandler
        {
            private GamePlay gamePlay;
            public GamePlaySessionHandler(ISession session, GamePlay gameplay) : base(session)
            {
                this.gamePlay = gameplay;
            }

            public override void onGameRoomJoin(IEvent @event)
            {
                base.onGameRoomJoin(@event);
                //session.tcpSender.sendMessage(Events.CreateEvent(null, Events.START));
            }
            public override void onDataIn(IEvent @event)
            {
                Debug.Log("On Data In");
                NettyMessageBuffer buffer = (NettyMessageBuffer)@event.getSource();
                var sourceId = buffer.readUnsignedShort();
                if (sourceId == 0)
                {
                    Debug.Log("Remaining Human Population: " + buffer.readInt());
                }

            }
        }
    }
}
