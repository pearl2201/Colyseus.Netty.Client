using DotNetty.Buffers;

public class SimpleCredentials : Credentials
{
    private string username;
    private string password;

    public SimpleCredentials(string username, string password)
    {
        this.username = username;
        this.password = password;
    }

    public SimpleCredentials(IByteBuffer buffer)
    {
        this.username = NettyUtils.readString(buffer);
        this.password = NettyUtils.readString(buffer);
    }


    public string getUsername()
    {
        return username;
    }


    public string getPassword()
    {
        return password;
    }


    public override string ToString()
    {
        return username;
    }

}
