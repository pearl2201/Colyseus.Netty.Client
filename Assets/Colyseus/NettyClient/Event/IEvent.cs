using System;
using System.Collections.Generic;
using System.Text;

namespace Coleseus.Shared.Event
{
	public interface IEvent
	{
		int getType();

		void setType(int type);

		Object getSource();

		void setSource(Object source);

		DateTime getTimeStamp();

		void setTimeStamp(DateTime timeStamp);
	}
}
