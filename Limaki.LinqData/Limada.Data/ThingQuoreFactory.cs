using System;
using Limaki.Data;

namespace Limada.Data {

    public abstract class ThingQuoreFactory {

        public abstract bool Supports (IDbProvider provider);
        public abstract DbGateway CreateGateway (IDbProvider provider);
        public abstract IQuore CreateQuore (DbGateway gateway);
        public abstract IThingQuore CreateThingQuore (Func<IQuore> createQuore);
    }
}