using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Proto;
using Google.Protobuf;
using Network.Http;
using Tsuixl.Net;
using Tsuixl.Net.Buffer;
using Tsuixl.Net.Session;
using UnityEngine;

public class NetTest : MonoBehaviour, ISessionEvent
{
    private ClientSession _clientSession;
    
    private void Start()
    {
        Init();
    }


    private async void Init()
    {
        NetSynchronizationContext.Instance.Init();
        _clientSession = new ClientSession(new FixedHeadPacketFilter(6), this);
        _clientSession.Connect("127.0.0.1", 4040);
    }

    private void OnDestroy()
    {
        _clientSession?.Close();
    }

    public void OnConnected(bool succeed)
    {
        if (succeed)
        {
            StartCoroutine(TimerCall());
            // var login = new C2S_Login {Account = "account", Password = "password"};
            // _clientSession.Send(1, login);
            NetLog.Log("connect succeed");
        }
        else
        {
            NetLog.Log("connect faild");
        }
       
    }

    public void OnDisconnected(string error)
    {
        NetLog.Log("OnDisconnected " + error);
    }

    public void OnClose()
    {
    }

    public void OnException(string error)
    {
    }

    public void OnMessage(ISession session, PacketData package)
    {
        switch (package.Code)
        {
            case 20001 :
                var loginData = PacketData.ToProtobufMessage(ref package, S2C_Login.Parser);
                NetLog.Log($"UniqueId {loginData.UniqueId}");
                break;
        }
    }

    public IEnumerator TimerCall()
    {
        while (true)
        {
            var login = new C2S_Login {Account = "account", Password = "password"};
            _clientSession.Send(1, login);
            NetLog.Log("Send C2S_Login");
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
    }
}
