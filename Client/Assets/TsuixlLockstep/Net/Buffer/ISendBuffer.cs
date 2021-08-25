namespace Tsuixl.Net.Buffer
{
    public interface ISendBuffer
    {
        int Offset { get; }
        int RemainSize { get; }
        byte[] Data { get; }
        bool IsSendFinish { get; }
        void Advance(int count);
        void OnRecycle();
    }
}