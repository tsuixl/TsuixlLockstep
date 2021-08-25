namespace Tsuixl.Net.Session
{

    public enum NetMessageType
    {
        ON_CONNECT_SUCCEED,         // 连接成功
        ON_CONNECT_FAILD,           // 连接失败
        
        ON_EXCEPTION,               // 异常                  
        ON_MESSAGE,                 // 消息
        ON_CLOSE,                   // 关闭
    }   
    
    public class SynchronizationSessionData
    {
        public NetMessageType Type;
        public PacketData Packet;
        public string StringContent;
        public int IntContent;
        public bool BoolContent;

        public void Reset()
        {
            Packet = null;
            StringContent = null;
        }
        
    }
}