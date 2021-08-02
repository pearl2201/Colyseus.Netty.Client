﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Coleseus.Shared.Event.Impl
{
    public class DefaultEvent : IEvent
    {
        /**
	 * 
	 */
        private const long serialVersionUID = -1114679476675012101L;
        protected int type;
        protected Object source;
        protected DateTime timeStamp;


       


        public virtual int getType()
        {
            return type;
        }


        public virtual Object getSource()
        {
            return source;
        }


        public DateTime getTimeStamp()
        {
            return timeStamp;
        }


        public virtual void setType(int type)
        {
            this.type = type;
        }


        public virtual void setSource(Object source)
        {
            this.source = source;
        }


        public void setTimeStamp(DateTime timeStamp)
        {
            this.timeStamp = timeStamp;

        }
       

        public override string ToString()
        {
            return "Event [type=" + type + ", source=" + source + ", timeStamp="
                    + timeStamp + "]";
        }
    }
}
