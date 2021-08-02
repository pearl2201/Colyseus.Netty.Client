﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.App
{
    public interface  IPlayer
    {
		/**
	  * A unique key representing a gamer. This could be an email id or something
	  * unique.
	  * 
	  * @return Returns the unique key for the gamer.
	  */
		Object getId();

		/**
		 * A unique key representing a gamer. This could be an email id or something
		 * unique.
		 * 
		 */
		void setId(Object uniqueKey);

		/**
		 * Method used to get the name of the gamer.
		 * 
		 * @return Returns the name or null if none is associated.
		 */
		String getName();

		/**
		 * Method used to set the name of the gamer.
		 * 
		 * @param name
		 *            Set the string name, strings more than 100 characters long may
		 *            be rejected.
		 */
		void setName(String name);

		/**
		 * Method used to get the email id of the gamer.
		 * 
		 * @return Returns the email id string, null if none is set.
		 */
		String getEmailId();

		/**
		 * Method used to set the email id of the gamer.
		 * 
		 * @param emailId
		 *            Sets the email id string. strings more than 50 characters long
		 *            may be rejected.
		 */
		void setEmailId(String emailId);

		/**
		 * Add a session to a player. This session signifies the players
		 * session to a game.
		 * 
		 * @param session
		 *            The session to add.
		 * @return true if add was successful, false if not.
		 */
		bool addSession(IPlayerSession session);

		/**
		 * Remove the players session to a game.
		 * 
		 * @param session
		 *            The session to remove.
		 * @return true if remove is successful, false other wise.
		 */
		bool removeSession(IPlayerSession session);

		/**
		 * When a player logs out, this method can be called. It can also be called
		 * by the remove session method when it finds that there are no more
		 * sessions for this player.
		 * 
		 * @param playerSession
		 *            The session which is to be logged out.
		 */
		void logout(IPlayerSession playerSession);
	}
}
