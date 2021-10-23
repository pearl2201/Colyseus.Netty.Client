using Assets.ZombieGame;
using Colyseus.NettyServer.LostDecade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.LostDecade
{
    public class GEntityView : MonoBehaviour
    {
        public GEntity entity;

        public LDRoomTest manager;
        public SpriteRenderer sprite;
        public bool isPlayer;

        public void Setup(bool isPlayer, GEntity entity)
        {
            this.isPlayer = isPlayer;
            sprite.color = isPlayer ? Color.green : Color.cyan;
            this.entity = entity;
            this.transform.position = new Vector2(entity.X, entity.Y);
            gameObject.name = isPlayer ? "player" : entity.Id;
        }

        private void Update()
        {
            if (isPlayer)
            {
                
                var x = Input.GetAxis("Horizontal");
                var y = Input.GetAxis("Vertical");
                entity.X += x;
                entity.Y += y;
                if (x != 0 || y != 0)
                {

                    manager.UpdatePlayer(entity);

                }
            }
        }
        public void UpdateEntity(GEntity entity)
        {
            this.entity = entity;
            this.transform.position = new Vector2(entity.X, entity.Y);
        }
    }
}
