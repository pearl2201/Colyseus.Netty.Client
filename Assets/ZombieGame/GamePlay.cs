using Assets.Colyseus.NettyClient.App;
using Coleseus.Shared.Communication;
using Coleseus.Shared.Event;
using Colyseus.NettyServer.ZombieGame.Domain;
using DotNetty.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlay : MonoBehaviour
{
    private IAM IAM;
    private ISession session;

    public void SetupGamePlay(IAM iam, ISession session)
    {
        IAM = iam;
        this.session = session;
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
            IEvent @event = Events.NetworkEvent(messageBuffer, DeliveryGuaranty.FAST);
            session.onEvent(@event);
        }
    }
}
