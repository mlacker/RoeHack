using EasyHook;
using RoeHack.Library.DirectXHook;

namespace RoeHack.Library.Core
{
    public class DriectXEntryPoint: IEntryPoint
    {
        public DriectXEntryPoint(RemoteHooking.IContext context, Parameter parameter)
        {
            RemoteHooking.IpcConnectClient<ServerInterface>(parameter.ChannelName);
        }

        public void Run(RemoteHooking.IContext context, Parameter parameter)
        {
            InitialDriectXHook(parameter);
        }

        private void InitialDriectXHook(Parameter parameter)
        {
            IDirectXHook hook = new DriectX9Hook(parameter);

            hook.Hooking();
        }
    }
}
