using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Colyseus.NettyServer.ZombieGame.Domain
{

	public class World
	{
		
		private volatile int alive;
		private volatile int undead;

		public bool apocalypse()
		{
			if (alive <= 0)
			{
				return true;
			}
			return false;
		}

		public void report()
		{
			if (alive > 0)
			{
				Debug.Log(string.Format("alive= {0} undead= {1}", alive, undead));
			}
		}

		public int getAlive()
		{
			return alive;
		}

		public void setAlive(int alive)
		{
			this.alive = alive;
		}

		public int getUndead()
		{
			return undead;
		}

		public void setUndead(int undead)
		{
			this.undead = undead;
		}

		public void shotgun()
		{
			int newUndead = undead - 1;
			Debug.Log("Defender update, undead = " + undead + " new undead: " + newUndead);
			undead = newUndead;
		}

		public void eatBrains()
		{
			Debug.Log(string.Format("In eatBrains Alive: {0} Undead: {1}", alive, undead));
			alive--;
			undead += 2;
			Debug.Log(string.Format("New Alive: {0} Undead: {1}", alive, undead));
		}

	}

}
