using Assets.Colyseus.NettyClient.App;
using Coleseus.Shared.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Colyseus.NettyClient.Communication
{


    /**
	 * Implementations of this policy determine the logic to be applied for
	 * re-connecting sessions on exception/disconnect situations.
	 * 
	 * @author Abraham Menacherry
	 * 
	 */
    public abstract class ReconnectPolicy
    {
        public abstract void ApplyPolicy(ISession session);
        public static ReconnectPolicy NO_RECONNECT = new NoReconnect();

    }

    /**
     * This reconnect policy class will try to reconnect to server for n times
     * before giving up and closing the session. The number of times, to try,
     * the delay between reconnect attempts are configurable.
     * 
     * @author Abraham Menacherry
     * 
     */
    public class ReconnectNTimes : ReconnectPolicy
    {
        protected int times;
        protected int delay;
        protected LoginHelper loginHelper;

        /**
		 * Constructor used to initialize the number of times reconnect should
		 * be tried, along with the delay between retries.
		 * 
		 * @param times
		 *            The number of times session reconnect should be attempted.
		 * @param delay
		 *            The delay between reconnect attempts in milliseconds. For
		 *            e.g 5000 = 5 seconds delay.
		 * @param loginHelper
		 *            Used to pass configuration information to setup the
		 *            connection to server once again.
		 */
        public ReconnectNTimes(int times, int delay, LoginHelper loginHelper)
        {
            this.times = times;
            this.delay = delay;
            this.loginHelper = loginHelper;
        }
        public class LoginSuccessHandler : IEventHandler
        {
            public ISession session { get; set; }

            public CountdownEvent loginSuccessLatch { get; set; }
            public void onEvent(IEvent @event)

            {
                // remove after use
                session.removeHandler(this);
                loginSuccessLatch.AddCount();
            }




            public int getEventType()
            {
                return Events.LOG_IN_SUCCESS;
            }
        };



        public override void ApplyPolicy(ISession session)
        {
            // Listen for log in success event to be received on the session.
            CountdownEvent loginSuccessLatch = new CountdownEvent(1);
            IEventHandler loginSuccess = new LoginSuccessHandler()
            {
                session = session,
                loginSuccessLatch = loginSuccessLatch
            };




            session.addHandler(loginSuccess);

            // try to reconnect for n times.
            int tries = 1;
            for (; tries <= times; tries++)
            {
                session.reconnect(loginHelper);
                try
                {
                    if (loginSuccessLatch.Wait(TimeSpan.FromMilliseconds(delay)))
                    {
                        break;
                    }

                    else
                    {
                        Debug.LogError("Reconnect try " + tries + " did not succeed");
                    }
                }

                catch (ThreadInterruptedException e)
                {
                    throw new Exception(e.Message);
                }
            }

            // if times == tries, then all the reconnect attempts were a
            // failure, close session.
            if (tries > times)
            {
                loginSuccessLatch.AddCount();
                Debug.LogError("Reconnect attempted " + tries + " times did not succeed, going to close session");
                session.close();
            }
        }
    }
    public class NoReconnect : ReconnectPolicy
    {



        public override void ApplyPolicy(ISession session)
        {
            session.close();
        }
    }

}

