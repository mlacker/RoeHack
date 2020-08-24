using System;
using System.Collections.Generic;

namespace RoeHack.Library.Core
{
    [Serializable]
    public class Parameter
    {
        public string ChannelName { get; set; }

        public DirectXVersion DirectXVersion { get; set; }

        public List<IntPtr> ProcAddress { get; set; }
    }
}
