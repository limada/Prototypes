using System;
using Limada.Data;
using Limaki.Data;

namespace Limada.LinqData {

    public class InMemoryThingQuoreFactory : ThingQuoreFactory {

        public override bool Supports (IDbProvider provider) { return provider.Name == "InMemoryProvider"; }

        public override DbGateway CreateGateway (IDbProvider provider) {
            return new DbGateway (provider);
        }

        public override IQuore CreateQuore (DbGateway gateway) {
            var p = gateway.Provider as InMemoryDbQuoreProvider;
            return new ConvertableQuore (p.GetCreateQuore (gateway.Iori));
        }

        public override IThingQuore CreateThingQuore (Func<IQuore> createQuore) {
            return new ThingQuore (createQuore);
        }
    }
}