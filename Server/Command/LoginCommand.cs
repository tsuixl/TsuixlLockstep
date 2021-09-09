using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Proto;
using Server.Game;
using Server.IdUtil;
using Server.Package;
using SuperSocket;
using SuperSocket.Command;

namespace Server.Command
{
    
    [Command(Key = "LoginCommand")]
    public class LoginCommand : IAsyncCommand<DefaultPackage>
    {
        public async ValueTask ExecuteAsync(IAppSession session, DefaultPackage package)
        {
            if (SynchronizationContext.Current is SynchronizationQueue synchronizationQueue)
            {
                synchronizationQueue.Debug();
            }
            Console.WriteLine($"body count : {package.Buffer.Length}");
            var loginData = C2S_Login.Parser.ParseFrom(package.Buffer);
            Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] - [{session.SessionID}] - {loginData.ToString()}");
            MessageDistribution.Instance.OnThreadMessage(package.Code, loginData);
            // 返回
            var reply = new S2C_Login {Code = OpCode.S2C_LOGIN, UniqueId = UniqueIdGenerator.GetGUID()};
            await session.SendAsync(PackageUtil.GetMsgPackage(OpCode.S2C_LOGIN, reply));
        }
    }
}