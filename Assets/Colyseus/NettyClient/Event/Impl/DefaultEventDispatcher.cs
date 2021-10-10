
using Assets.Colyseus.NettyClient.App;
using System.Collections.Generic;
using UnityEngine;

namespace Coleseus.Shared.Event.Impl
{
    public class DefaultEventDispatcher : IEventDispatcher
    {

        private Dictionary<int, List<IEventHandler>> handlersByEventType;
        private List<IEventHandler> genericHandlers;
        private bool isShuttingDown;
        private readonly System.Object locker = new System.Object();



        public DefaultEventDispatcher() : this(new Dictionary<int, List<IEventHandler>>(2),
                    new List<IEventHandler>())
        {

        }

        public DefaultEventDispatcher(
                Dictionary<int, List<IEventHandler>> handlersByEventType,
                List<IEventHandler> genericHandlers)
        {
            this.handlersByEventType = handlersByEventType;
            this.genericHandlers = genericHandlers;
            this.isShuttingDown = false;
        }


        public void addHandler(IEventHandler eventHandler)
        {
            int eventType = eventHandler.getEventType();
            lock (locker)


            {
                if (eventType == Events.ANY)
                {
                    genericHandlers.Add(eventHandler);
                }
                else
                {

                    if (!this.handlersByEventType.TryGetValue(eventType, out var handlers))
                    {
                        handlers = new List<IEventHandler>();
                        this.handlersByEventType.Add(eventType, handlers);
                    }

                    handlers.Add(eventHandler);
                }
            }
        }


        public List<IEventHandler> getHandlers(int eventType)
        {
            return handlersByEventType[eventType];
        }


        public void removeHandler(IEventHandler eventHandler)
        {
            int eventType = eventHandler.getEventType();
            lock (locker)


            {
                if (eventType == Events.ANY)
                {
                    genericHandlers.Remove(eventHandler);
                }
                else
                {

                    if (this.handlersByEventType.TryGetValue(eventType, out var handlers))
                    {
                        handlers.Remove(eventHandler);
                        // Remove the reference if there are no listeners left.
                        if (handlers.Count == 0)
                        {
                            handlersByEventType.Add(eventType, null);
                        }
                    }
                }
            }

        }


        public void removeHandlersForEvent(int eventType)
        {
            lock (locker)
            {

                if (this.handlersByEventType.TryGetValue(eventType, out var handlers))
                {
                    handlers.Clear();
                }
            }
        }


        public bool removeHandlersForSession(ISession session)
        {
            List<IEventHandler> removeList = new List<IEventHandler>();
            var eventHandlersList = handlersByEventType.Values;
            foreach (List<IEventHandler> handlerList in eventHandlersList)
            {
                if (null != handlerList)
                {
                    foreach (IEventHandler handler in handlerList)
                    {
                        if (handler is SessionEventHandler)
                        {
                            SessionEventHandler sessionHandler = (SessionEventHandler)handler;
                            if (sessionHandler.getSession().Equals(session))
                            {
                                removeList.Add(handler);
                            }
                        }
                    }
                }
            }
            foreach (IEventHandler handler in removeList)
            {
                removeHandler(handler);
            }
            return (removeList.Count > 0);
        }


        public void clear()
        {
            lock (locker)
            {
                if (null != handlersByEventType)
                {
                    handlersByEventType.Clear();
                }
                if (null != genericHandlers)
                {
                    genericHandlers.Clear();
                }
            }

        }


        public void fireEvent(IEvent @event)
        {
            bool isShuttingDown = false;
            lock (locker)

            {
                isShuttingDown = this.isShuttingDown;
            }
            if (!isShuttingDown)
            {
                foreach (IEventHandler handler in genericHandlers)
                {
                    handler.onEvent(@event);
                }

                // retrieval is not thread safe, but since we are not setting it to
                // null
                // anywhere it should be fine.
               
                // Iteration is thread safe since we use copy on write.
                if (handlersByEventType.TryGetValue(@event.getType(), out var handlers))
                {
                    foreach (IEventHandler handler in handlers)
                    {
                        handler.onEvent(@event);
                    }
                }
            }

            else
            {
                Debug.LogError("Discarding event: " + @event


                        + " as dispatcher is shutting down");
            }

        }


        public void close()
        {
            lock (locker)

            {
                isShuttingDown = true;
                genericHandlers.Clear();
                handlersByEventType.Clear();
            }

        }
    }
}
