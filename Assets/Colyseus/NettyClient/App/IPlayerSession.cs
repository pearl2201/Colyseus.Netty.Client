using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.App
{
    public interface IPlayerSession : ISession
    {
        IPlayer getPlayer();
    }
}
