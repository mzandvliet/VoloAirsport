using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;

namespace RamjetAnvil.Unity.Utility
{
    public static class Schedulers
    {
        public static readonly IScheduler FileWriterScheduler = new EventLoopScheduler(ts => new Thread(ts) {Name = "File writer"});
    }
}
