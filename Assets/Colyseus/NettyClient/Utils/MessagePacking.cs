using Coleseus.Shared.Communication;
using Coleseus.Shared.Event.Impl;
using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Colyseus.NettyClient.Utils
{
    public static class MessagePacking
    {
        // message header size
        public const int HeaderSize = sizeof(ushort);

        // max message content size (without header) calculation for convenience
        // -> Transport.GetMaxPacketSize is the raw maximum
        // -> Every message gets serialized into <<id, content>>
        // -> Every serialized message get put into a batch with a header
        //public static int MaxContentSize =>
        //    Transport.activeTransport.GetMaxPacketSize()
        //    - HeaderSize
        //    - Batcher.HeaderSize;

        public static ushort GetId<T>() where T : DefaultNetworkEvent
        {
            // paul: 16 bits is enough to avoid collisions
            //  - keeps the message size small
            //  - in case of collisions,  Mirror will display an error
            return (ushort)(typeof(T).FullName.GetStableHashCode() & 0xFFFF);
        }

        public static ushort GetId<T>(this T @event)
        {
            // paul: 16 bits is enough to avoid collisions
            //  - keeps the message size small
            //  - in case of collisions,  Mirror will display an error
            return (ushort)(typeof(T).FullName.GetStableHashCode() & 0xFFFF);
        }

        // pack message before sending
        // -> NetworkWriter passed as arg so that we can use .ToArraySegment
        //    and do an allocation free send before recycling it.
        public static void Pack<T>(T message, MessageBuffer<IByteBuffer> writer)
            where T : DefaultNetworkEvent
        {
            ushort msgType = GetId<T>();
            writer.writeShort(msgType);

            // serialize message into writer

            message.getBufferData();
        }

        // unpack message after receiving
        // -> pass NetworkReader so it's less strange if we create it in here
        //    and pass it upwards.
        // -> NetworkReader will point at content afterwards!
        //public static bool Unpack(MessageBuffer<IByteBuffer> messageReader, out ushort msgType)
        //{
        //    // read message type
        //    try
        //    {
        //        msgType = messageReader.readUnsignedShort();
        //        return true;
        //    }
        //    catch (System.IO.EndOfStreamException)
        //    {
        //        msgType = 0;
        //        return false;
        //    }
        //}

        //internal static NetworkMessageDelegate WrapHandler<T, C>(Action<C, T> handler, bool requireAuthentication)
        //    where T : DefaultNetworkEvent
        //    where C : NetworkConnection
        //    => (conn, reader, channelId) =>
        //    {
        //        // protect against DOS attacks if attackers try to send invalid
        //        // data packets to crash the server/client. there are a thousand
        //        // ways to cause an exception in data handling:
        //        // - invalid headers
        //        // - invalid message ids
        //        // - invalid data causing exceptions
        //        // - negative ReadBytesAndSize prefixes
        //        // - invalid utf8 strings
        //        // - etc.
        //        //
        //        // let's catch them all and then disconnect that connection to avoid
        //        // further attacks.
        //        T message = default;
        //        // record start position for NetworkDiagnostics because reader might contain multiple messages if using batching
        //        int startPos = reader.Position;
        //        try
        //        {
        //            if (requireAuthentication && !conn.isAuthenticated)
        //            {
        //                // message requires authentication, but the connection was not authenticated
        //                Debug.LogWarning($"Closing connection: {conn}. Received message {typeof(T)} that required authentication, but the user has not authenticated yet");
        //                conn.Disconnect();
        //                return;
        //            }

        //            //Debug.Log($"ConnectionRecv {conn} msgType:{typeof(T)} content:{BitConverter.ToString(reader.buffer.Array, reader.buffer.Offset, reader.buffer.Count)}");

        //            // if it is a value type, just use default(T)
        //            // otherwise allocate a new instance
        //            message = reader.Read<T>();
        //        }
        //        catch (Exception exception)
        //        {
        //            Debug.LogError($"Closed connection: {conn}. This can happen if the other side accidentally (or an attacker intentionally) sent invalid data. Reason: {exception}");
        //            conn.Disconnect();
        //            return;
        //        }
        //        finally
        //        {
        //            int endPos = reader.Position;
        //            // TODO: Figure out the correct channel
        //            NetworkDiagnostics.OnReceive(message, channelId, endPos - startPos);
        //        }

        //        // user handler exception should not stop the whole server
        //        try
        //        {
        //            // user implemented handler
        //            handler((C)conn, message);
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.LogError($"Disconnecting connId={conn.connectionId} to prevent exploits from an Exception in MessageHandler: {e.GetType().Name} {e.Message}\n{e.StackTrace}");
        //            conn.Disconnect();
        //        }
        //    };
    }
    public static class Extensions
    {
        // string.GetHashCode is not guaranteed to be the same on all machines, but
        // we need one that is the same on all machines. simple and stupid:
        public static int GetStableHashCode(this string text)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                    hash = hash * 31 + c;
                return hash;
            }
        }

        // previously in DotnetCompatibility.cs
        // leftover from the UNET days. supposedly for windows store?
        internal static string GetMethodName(this Delegate func)
        {
#if NETFX_CORE
            return func.GetMethodInfo().Name;
#else
            return func.Method.Name;
#endif
        }

        // helper function to copy to List<T>
        // C# only provides CopyTo(T[])
        public static void CopyTo<T>(this IEnumerable<T> source, List<T> destination)
        {
            // foreach allocates. use AddRange.
            destination.AddRange(source);
        }
    }

}
