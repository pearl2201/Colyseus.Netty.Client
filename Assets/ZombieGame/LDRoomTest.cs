using Assets.Colyseus.NettyClient.App;
using Assets.Colyseus.NettyClient.App.Impl;
using Assets.Colyseus.NettyClient.Event.Impl;
using Assets.Colyseus.NettyClient.Utils;
using Assets.LostDecade;
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
    public interface IGameManager
    {
        void DispatchState(LDGameState state);

        void RegisterHandler(AbstractSessionEventHandler handler);
    }
    public class LDRoomTest : MonoBehaviour, IGameManager
    {

        private LDGameState state;
        private AbstractSessionEventHandler handler;

        private List<GEntityView> entities = new List<GEntityView>();

        public string myPlayerId;

        public GameObject viewPrefab;
        private ISession session;
        public void Start()
        {
            LoginBuilder builder = new LoginBuilder().setUsername("user")
              .setPassword("pass").setConnectionKey("LDGameRoom")
              .setNadronTcpHostName("127.0.0.1").setTcpPort(18090);


            LoginHelper loginHelper = builder.build();
            SessionFactory sessionFactory = new SessionFactory(loginHelper);
            sessionFactory.setLoginHelper(loginHelper);
            session = sessionFactory.createSession();

            session.addHandler(new GameStartEventHandler(session, this));

            // Connect the session, so that the above start event will be sent by server.
            sessionFactory.connectSession(session);

        }

        public void DispatchState(LDGameState state)
        {
            this.state = state;
            myPlayerId = session.getId().ToString();
            if (state.Entities != null)
            {
                foreach (var entity in state.Entities)
                {
                    var current = entities.FirstOrDefault(x => x.entity.Id == entity.Id);
                    if (current == null)
                    {
                        GameObject go = Instantiate(viewPrefab);
                        go.transform.position = new Vector3(entity.X, entity.Y);
                        var entityView = go.GetComponent<GEntityView>();
                        entityView.Setup(entity.Id == session.PlayerId, entity);

                        entityView.manager = this;
                        entityView.entity = entity;
                        entities.Add(entityView);
                    }
                    else
                    {
                        current.UpdateEntity(entity);
                    }
                }
            }


            //var entityRemove = entities.Where(x => !state.Entities.Select(y => y.Id).Contains(x.entity.Id)).ToList();
            //foreach (var entity in entityRemove)
            //{
            //    Destroy(entity.gameObject);
            //}
        }

        public void UpdatePlayer(GEntity entity)
        {
            LDUpdateHeroPositionEvent state = new LDUpdateHeroPositionEvent()
            {
                X = entity.X,
                Y = entity.Y
            };
            IEvent @event = Events.CreateEvent(state, Events.NETWORK_MESSAGE);
            INetworkEvent networkEvent = new LDEvent(2, @event);

            handler.onNetworkMessage(networkEvent);
            var termBuffer = networkEvent.getBufferData();
            var sourceId = termBuffer.ReadUnsignedShort();
            Debug.Log("sourceId: " + sourceId);
            //session.onEvent(networkEvent);
        }
        public void RegisterHandler(AbstractSessionEventHandler handler)
        {
            this.handler = handler;
        }


        public class GameStartEventHandler : AbstractSessionEventHandler
        {

            private IGameManager gameManager;
            public GameStartEventHandler(ISession session, IGameManager gameManager) : base(session)
            {
                this.gameManager = gameManager;
            }



            public override void onEvent(IEvent @event)
            {
                base.onEvent(@event);
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
                GamePlaySessionHandler handler = new GamePlaySessionHandler(session, gameManager);

                session.addHandler(handler);
            }

            public override void onDataIn(IEvent @event)
            {

            }
        }

        public class GamePlaySessionHandler : AbstractSessionEventHandler
        {
            public LDGameState state;
            private IGameManager gameManager;
            public GamePlaySessionHandler(ISession session, IGameManager gameManager) : base(session)
            {
                this.gameManager = gameManager;
                gameManager.RegisterHandler(this);
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
                if (sourceId == 0)
                {
                    Debug.Log("Unknow message: " + sourceId);
                }
                else if (sourceId == 1)
                {
                    var state = LDGameState.FromMessageBuffer(buffer);
                    Loom.QueueOnMainThread(() =>
                    {
                        gameManager.DispatchState(state);

                    });
                }
            }
        }


    }
}
