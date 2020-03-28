﻿using System;

namespace RoeHack.Library.Core
{
    [Serializable]
    public class Parameter
    {
        public string ChannelName { get; set; }

        public DirectXVersion DirectXVersion { get; set; }
    }
}
