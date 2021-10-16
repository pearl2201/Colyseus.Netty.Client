using Assets.Colyseus.NettyClient.App;
using Assets.Colyseus.NettyClient.App.Impl;
using Assets.Colyseus.NettyClient.Event.Impl;
using Assets.Colyseus.NettyClient.Utils;
using Coleseus.Shared.Event;
using Coleseus.Shared.Event.Impl;
using Colyseus.NettyServer.LostDecade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static LoginHelper;

namespace Assets.ZombieGame
{
    public class LDRoomTest : MonoBehaviour
    {
        public void Start()
        {
            LoginBuilder builder = new LoginBuilder().setUsername("user")
              .setPassword("pass").setConnectionKey("LDGameRoom")
              .setNadronTcpHostName("127.0.0.1").setTcpPort(18090);


            LoginHelper loginHelper = builder.build();
            SessionFactory sessionFactory = new SessionFactory(loginHelper);
            sessionFactory.setLoginHelper(loginHelper);
            ISession session = sessionFactory.createSession();

            session.addHandler(new GameStartEventHandler(session));

            // Connect the session, so that the above start event will be sent by server.
            sessionFactory.connectSession(session);
        }




        public class GameStartEventHandler : StartEventHandler
        {


            public GameStartEventHandler(ISession session) : base(session)
            {

            }



            public override void onEvent(IEvent @event)
            {
                Debug.Log("Received start event, going to change protocol");

                // create LDState objects send it to server.
                GEntity heroEntity = new GEntity();
                heroEntity.Id = "1";
                heroEntity.Type = (GEntity.HERO);
                heroEntity.Score = (100);
                heroEntity.X = 0;
                heroEntity.Y = 0;
                LDGameState state = new LDGameState(new HashSet<GEntity>(), null, heroEntity);
                INetworkEvent networkEvent = Events.NetworkEvent(state);
                session.removeHandler(this);
                addDefaultHandlerToSession(session);
                //session.onEvent(networkEvent);

                //session.tcpSender.sendMessage(Events.CreateEvent(null, Events.START));
            }

            private void addDefaultHandlerToSession(ISession session)
            {
                // we are only interested in data in, so override only that method.
                GamePlaySessionHandler handler = new GamePlaySessionHandler(session);

                session.addHandler(handler);
            }
        }

        public class GamePlaySessionHandler : AbstractSessionEventHandler
        {

            public GamePlaySessionHandler(ISession session) : base(session)
            {

            }

            public override void onGameRoomJoin(IEvent @event)
            {
                base.onGameRoomJoin(@event);
                session.tcpSender.sendMessage(Events.CreateEvent(null, Events.START));
            }
            public override void onDataIn(IEvent @event)
            {
                Debug.Log("On Data In");
                NettyMessageBuffer buffer = (NettyMessageBuffer)@event.getSource();
                var sourceId = buffer.readUnsignedShort();
                if (sourceId == DefaultNetworkEvent.NetworkPackageId)
                {
                    Debug.Log("Unknow message: " + sourceId);
                }
                else if (sourceId == EntireStateEvent.NetworkPackageId)
                {
                    var state = LDGameState.FromMessageBuffer(buffer);
                 
                }
            }
        }
    }
}
