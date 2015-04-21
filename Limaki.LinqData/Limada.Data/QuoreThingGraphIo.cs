/*  
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Limada.IO;
using Limada.Model;
using Limaki.Common;
using Limaki.Contents;
using Limaki.Data;
using Id = System.Int64;

namespace Limada.Data {

    public class QuoreThingGraphIo : ThingGraphIo {

        public QuoreThingGraphIo () : base (Registry.Pooled<ThingQuoreContentSpot>()) { }

        public new ThingQuoreContentSpot Detector { get { return base.Detector as ThingQuoreContentSpot; } }

        protected override ThingGraphContent OpenInternal (Iori source) {
           
            try {
                var provider = Registry.Pooled<DbProviderPool> ().Get (source.Provider);
                var storeFactory = Detector.GetFactory (provider);
                if (storeFactory == null)
                    throw new ArgumentException (string.Format ("Open failed: connection {0} does not support ThingStore", Iori.ToFileName (source)));
                
                var gateway = storeFactory.CreateGateway (provider);
                gateway.Open (source);
                Func<IThingQuore> store = () => storeFactory.CreateThingQuore (() => storeFactory.CreateQuore (gateway));
                var sink = new ThingGraphContent {
                    Data = new QuoreThingGraph (store),
                    Source = source,
                    ContentType = 0,
                };
                return sink;
            } catch (Exception ex) {
                throw ex;
            }
        }

        public override void Flush (ThingGraphContent sink) {
            var d = sink.Data as QuoreThingGraph;
            if (d != null)
                d.Flush ();
        }

        public override void Close (ThingGraphContent sink) {
            Flush (sink);
             var d = sink.Data as QuoreThingGraph;
            if (d != null)
                d.Close ();
        }
    }
}