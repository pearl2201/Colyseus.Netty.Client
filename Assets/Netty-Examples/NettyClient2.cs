using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NettyClient2 : MonoBehaviour
{
    private NettyClient client;
    // Start is called before the first frame update
    void Start()
    {
        client = new NettyClient();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartServer()
    {

        client.Run();
    }

    public void CloseServer()
    {
        client.Close();
    }
}
