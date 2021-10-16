using Coleseus.Shared.Event.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colyseus.NettyServer.LostDecade
{
    public class LDEvent : DefaultEvent
    {
        private new LDGameState source;


        public override object getSource()
        {
            return source;
        }

        public void setSource(LDGameState source)
        {
            this.source = source;
        }
    }
}
