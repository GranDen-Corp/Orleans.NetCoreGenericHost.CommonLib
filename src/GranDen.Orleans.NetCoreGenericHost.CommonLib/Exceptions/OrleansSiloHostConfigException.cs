﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GranDen.Orleans.NetCoreGenericHost.CommonLib.Exceptions
{
    public class OrleansSiloHostConfigException : Exception
    {
        public OrleansSiloHostConfigException(string message) : base(message)
        { }
    }
}