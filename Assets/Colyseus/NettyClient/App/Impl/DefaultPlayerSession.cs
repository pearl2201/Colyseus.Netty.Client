using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.App.Impl
{
    public class DefaultPlayerSession : DefaultSession, IPlayerSession
    {
        protected IPlayer player;
        public DefaultPlayerSession(SessionBuilder sessionBuilder, IPlayer player) : base(sessionBuilder)
        {
            this.player = player;
        }
        public IPlayer getPlayer()
        {
            return player;
        }
    }
}
