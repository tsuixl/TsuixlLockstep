using System;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.Server;

namespace Server.Game
{
    public class GameService<TReceivePackageInfo> : SuperSocketService<TReceivePackageInfo>
        where TReceivePackageInfo : class
    {

        private static ILog log;
        public static GameService<TReceivePackageInfo> Instance { get; private set; }
        
        public GameService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(serviceProvider, serverOptions)
        {
            log = LogManager.GetLogger(this.GetType());
            Instance = this;
            log.InfoFormat("GameService init.");
        }
        
        protected override ValueTask OnSessionClosedAsync(IAppSession session, CloseEventArgs e)
        {
            log.InfoFormat("OnSessionClosedAsync : {0}", session.SessionID);
            return base.OnSessionClosedAsync(session, e);
        }
        
        protected override ValueTask OnSessionConnectedAsync(IAppSession session)
        {
            return base.OnSessionConnectedAsync(session);
        }
    }
}