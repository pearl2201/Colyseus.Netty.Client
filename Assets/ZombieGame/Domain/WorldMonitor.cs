
using Coleseus.Shared.Communication;
using Coleseus.Shared.Event;
using Colyseus.NettyServer.ZombieGame.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colyseus.NettyServer.ZombieGame.Domain
{
    public class WorldMonitor 
    {
        private World world;


        private Object id;

        public WorldMonitor(World world)
        {
            this.world = world;
          
            id = Guid.NewGuid();
        }

        public World getWorld()
        {
            return world;
        }

        public void setWorld(World world)
        {
            this.world = world;
        }

    
    }
}
