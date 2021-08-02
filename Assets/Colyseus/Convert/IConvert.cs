using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * A generic interface for transforming one object to another. Implementations
 * of this interface can be used for decoding and encoding objects, maybe while
 * reading from a {@link MessageBuffer} or writing to a Netty channel.
 * 
 * @author Abraham Menacherry
 * 
 * @param <T>
 * @param <V>
 */
public interface IConvert<T, V>
{

	/**
	 * Convert Object of type T to type V.
	 * 
	 * @param object
	 *            The incoming object, mostly a buffer or byte array.
	 * @return Returns the converted object.
	 * @throws Exception
	 */
	V convert(T input);
}
