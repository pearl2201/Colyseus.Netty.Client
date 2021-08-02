using Coleseus.Shared.Communication;
using DotNetty.Buffers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;


/**
 * The creation of a connection to a remote nadron server requires multiple
 * parameters, for e.g. username, password etc. These parameters are stored in
 * this class which uses a builder pattern to create an instance. This instance
 * is then passed on to {@link SessionFactory} to actually create the sessions,
 * connections etc.
 * 
 * @author Abraham Menacherry
 * 
 */
public class LoginHelper
{
    private string username;
    private string password;
    private string connectionKey;
    private IPEndPoint tcpServerAddress;
    private IPEndPoint udpServerAddress;

    protected LoginHelper(LoginBuilder loginBuilder)
    {
        loginBuilder.validateAndSetValues();
        this.username = loginBuilder.getUsername();
        this.password = loginBuilder.getPassword();
        this.connectionKey = loginBuilder.getConnectionKey();
        this.tcpServerAddress = loginBuilder.getTcpServerAddress();
        this.udpServerAddress = loginBuilder.getUdpServerAddress();
    }

    public class LoginBuilder
    {
        private string username;
        private string password;
        private string connectionKey;
        private string nadronTcpHostName;
        private int tcpPort;
        private string nadronUdpHostName;
        private int udpPort;
        private IPEndPoint tcpServerAddress;
        private IPEndPoint udpServerAddress;

        public string getUsername()
        {
            return username;
        }

        public LoginBuilder setUsername(string username)
        {
            this.username = username;
            return this;
        }

        public string getPassword()
        {
            return password;
        }

        public LoginBuilder setPassword(string password)
        {
            this.password = password;
            return this;
        }

        public string getConnectionKey()
        {
            return connectionKey;
        }

        public LoginBuilder setConnectionKey(string connectionKey)
        {
            this.connectionKey = connectionKey;
            return this;
        }

        public string getNadronTcpHostName()
        {
            return nadronTcpHostName;
        }

        public LoginBuilder setNadronTcpHostName(string nadronTcpHostName)
        {
            this.nadronTcpHostName = nadronTcpHostName;
            return this;
        }

        public int getTcpPort()
        {
            return tcpPort;
        }

        public LoginBuilder setTcpPort(int tcpPort)
        {
            this.tcpPort = tcpPort;
            return this;
        }

        public string getNadronUdpHostName()
        {
            return nadronUdpHostName;
        }

        public LoginBuilder setNadronUdpHostName(string nadronUdpHostName)
        {
            this.nadronUdpHostName = nadronUdpHostName;
            return this;
        }

        public int getUdpPort()
        {
            return udpPort;
        }

        public LoginBuilder setUdpPort(int udpPort)
        {
            this.udpPort = udpPort;
            return this;
        }

        public IPEndPoint getTcpServerAddress()
        {
            return tcpServerAddress;
        }

        public LoginBuilder setTcpServerAddress(IPEndPoint tcpServerAddress)
        {
            this.tcpServerAddress = tcpServerAddress;
            return this;
        }

        public IPEndPoint getUdpServerAddress()
        {
            return udpServerAddress;
        }

        public LoginBuilder setUdpServerAddress(IPEndPoint updServerAddress)
        {
            this.udpServerAddress = updServerAddress;
            return this;
        }

        public LoginHelper build()
        {
            return new LoginHelper(this);
        }

        /**
		 * This method is used to validate and set the variables to default
		 * values if they are not already set before calling build. This method
		 * is invoked by the constructor of LoginHelper. <b>Important!</b>
		 * Builder child classes which override this method need to call
		 * super.validateAndSetValues(), otherwise you could get runtime NPE's.
		 */
        public void validateAndSetValues()
        {
            if (null == username)
            {
                throw new ArgumentException("Username cannot be null");
            }
            if (null == password)
            {
                throw new ArgumentException("Password cannot be null");
            }
            if (null == connectionKey)
            {
                throw new ArgumentException(
                        "ConnectionKey cannot be null");
            }
            if (null == tcpServerAddress
                    && (null == nadronTcpHostName || null == tcpPort))
            {
                throw new ArgumentException(
                        "tcpServerAddress cannot be null");
            }

            if (null == tcpServerAddress)
            {
                tcpServerAddress = new IPEndPoint(IPAddress.Parse(nadronTcpHostName),
                        tcpPort);
            }

            if (null == udpServerAddress)
            {
                if (null != nadronUdpHostName && null != udpPort)
                {
                    udpServerAddress = new IPEndPoint(
                            IPAddress.Parse(nadronUdpHostName), udpPort);
                }
            }
        }
    }

    /**
	 * Creates the appropriate login buffer using username, password,
	 * connectionkey and the local address to which the UDP channel is bound.
	 * 
	 * @param localUDPAddress
	 *            <b>optional</b> If passed in, then this address is passed on
	 *            to nadron server, so that it can associate this address with its
	 *            session.
	 * @return Returns the ByteBuf representation of username, password,
	 *         connection key, udp local bind address etc.
	 * @throws Exception
	 */
    public MessageBuffer<IByteBuffer> getLoginBuffer(
            IPEndPoint localUDPAddress)
    {
        IByteBuffer loginBuffer;
        IByteBuffer credentials = NettyUtils.writeStrings(new[]{username, password,
                connectionKey});
        if (null != localUDPAddress)
        {
            IByteBuffer udpAddressBuffer = NettyUtils
                    .writeSocketAddress(localUDPAddress);
            loginBuffer = Unpooled.WrappedBuffer(credentials,
                    udpAddressBuffer);
        }
        else
        {
            loginBuffer = credentials;
        }
        return new NettyMessageBuffer(loginBuffer);
    }

    /**
	 * Creates a wrapped netty buffer with reconnect key and udp address as its
	 * payload.
	 * 
	 * @param reconnectKey
	 *            The key that was initially sent by server on game room join
	 *            success event will be sent back to server for reconnecting
	 * @param udpAddress
	 *            If udp connection is required, then the new udpAddress need to
	 *            be sent.
	 * @return Returns the channel buffer containing reconnect key and udp
	 *         address in binary format.
	 */
    public MessageBuffer<IByteBuffer> getReconnectBuffer(string reconnectKey,
            IPEndPoint udpAddress)
    {
        IByteBuffer reconnectBuffer = null;
        IByteBuffer buffer = NettyUtils.WriteString(reconnectKey);
        if (null != udpAddress)
        {
            reconnectBuffer = Unpooled.WrappedBuffer(buffer,
                    NettyUtils.writeSocketAddress(udpAddress));
        }
        else
        {
            reconnectBuffer = buffer;
        }
        return new NettyMessageBuffer(reconnectBuffer);
    }

    public string getUsername()
    {
        return username;
    }

    public string getPassword()
    {
        return password;
    }

    public string getConnectionKey()
    {
        return connectionKey;
    }

    public IPEndPoint getTcpServerAddress()
    {
        return tcpServerAddress;
    }

    public IPEndPoint getUdpServerAddress()
    {
        return udpServerAddress;
    }
}
