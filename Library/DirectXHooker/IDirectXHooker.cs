using System;

namespace RoeHack.Library
{
    public interface IDirectXHooker: IDisposable
    {
        void Hooking();
    }
}