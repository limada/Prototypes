using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Limada.Data;
using Limada.IO;
using Limada.LinqData;
using Limaki.Common.IOC;
using Limaki.Data;

namespace Limaki.LinqData {

    public class DbQuoreResourceLoader : IContextResourceLoader {

        public void ApplyResources (IApplicationContext context) {

            var dbProviderPool = context.Pooled<DbProviderPool> ();
            dbProviderPool.Add (new InMemoryDbQuoreProvider ());
            dbProviderPool.Add (new MsSqlServerProvider ());

            var thingStoreSpot = context.Pooled<ThingQuoreContentSpot> ();
            thingStoreSpot.Add (new InMemoryThingQuoreFactory());

            var thingGraphContentPool = context.Pooled<ThingGraphIoPool> ();
            thingGraphContentPool.Add (new QuoreThingGraphIo ());
        }
    }
}
