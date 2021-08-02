using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Colyseus.NettyClient.App
{
    public interface IGame
    {

        /**
		 * @return Returns the unique id associated with this game object.
		 */
        Object getId();

        /**
		 * @param id
		 *            Sets the unique id for this game.
		 */
        void setId(Object id);

        /**
		 * Get the name of the game. Preferably should be a unique name.
		 * 
		 * @return Returns the name of the game.
		 */
        string getGameName();

        /**
		 * Set the name of the game. Preferably it should be a unique value.
		 * 
		 * @param gameName
		 *            Set the preferably unique game name.
		 */
        void setGameName(string gameName);

      

        /**
		 * Unloads the current game, by closing all sessions. This will delegate
		 * to {@link GameRoom#close()}
		 * 
		 * @return In case of Netty Implementation it would return a collection of
		 *         {@link ChannelFuture} object.
		 */
        Object unload();
    }

}
