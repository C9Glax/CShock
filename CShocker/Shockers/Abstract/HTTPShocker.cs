﻿using CShocker.Shockers.ShockerSettings;
using Microsoft.Extensions.Logging;

namespace CShocker.Shockers.Abstract;

internal abstract class HttpShocker : Shocker
{
    public HttpShocker(HttpShockerSettings settings, ILogger? logger = null) : base(settings, logger)
    {
        
    }
}