using Assets.Colyseus.NettyClient.App;
using Coleseus.Shared.Communication;
using Coleseus.Shared.Event;
using Colyseus.NettyServer.ZombieGame.Domain;
using DotNetty.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.ZombieGame.LoginTest;

public class GamePlay : MonoBehaviour
{
    public IAM IAM;
    private ISession session;
    private GamePlaySessionHandler hander;
    [SerializeField] bool _connected;
    public bool Connected { get => _connected; set { _connected = value; } }

    public void SetupGamePlay(IAM iam, ISession session)
    {
        IAM = iam;
        this.session = session;
    }

    public void SetupHandler(GamePlaySessionHandler hander)
    {
        this.hander = hander;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        int type = (int)IAM;
        int operation = 0;
        switch (IAM)
        {
            case IAM.DEFENDER:
                operation = (int)ZombieCommands.SHOT_GUN;
                break;
            case IAM.ZOMBIE:
                operation = (int)ZombieCommands.EAT_BRAINS;
                break;
        }

        for (int i = 1; i < 10; i++)
        {
            MessageBuffer<IByteBuffer> messageBuffer = new NettyMessageBuffer();
            messageBuffer.writeInt(type);
            messageBuffer.writeInt(operation);
            INetworkEvent @event = Events.NetworkEvent(messageBuffer, DeliveryGuaranty.RELIABLE);
            //session.onEvent(@event);
            this.hander.onNetworkMessage(@event);
            //session.tcpSender.sendMessage(@event);
        }
    }
}
