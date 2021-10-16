using Coleseus.Shared.Communication;
using Coleseus.Shared.Handlers.Netty;
using DotNetty.Buffers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Colyseus.NettyServer.LostDecade
{
    public class LDGameState : IDataBufferSchema
    {
        public HashSet<GEntity> Entities { get; set; }
        public GEntity Monster { get; set; }
        public GEntity Hero { get; set; }
        public bool Reset { get; set; }

        public LDGameState()
        {

        }


        public LDGameState(HashSet<GEntity> entities, GEntity monster, GEntity hero)
        {
          
            this.Entities = entities;
            this.Monster = monster;
            this.Hero = hero;
        }

        public void AddEntitiy(GEntity hero)
        {
            // only the id will match, but other values maybe different.
            Entities.Remove(hero);
            Entities.Add(hero);
        }

        public MessageBuffer<IByteBuffer> ToMessageBuffer()
        {
            var info = JsonConvert.SerializeObject(this);
            MessageBuffer<IByteBuffer> messageBuffer = new NettyMessageBuffer();
            messageBuffer.writeString(info);

            return messageBuffer;
        }

        public static LDGameState FromMessageBuffer(MessageBuffer<IByteBuffer> messageBuffer)
        {
            var info = messageBuffer.readString();
            Debug.Log("message: " + info);
            var state = JsonConvert.DeserializeObject<LDGameState>(info);
            return state;
        }
    }
}
