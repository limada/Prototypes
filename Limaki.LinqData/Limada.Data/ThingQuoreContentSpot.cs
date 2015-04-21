using System;
using System.Collections.Generic;
using Limaki.Contents;
using Limaki.Data;
using System.Linq;

namespace Limada.Data {

    public class ThingStoreContentInfo : ContentInfo {

        public ThingStoreContentInfo ()
            : base (null, ThingQuoreContentSpot.ContentType, null, null, CompressionType.None) { }

        public ThingStoreContentInfo (Magic[] magics)
            : this () { base.Magics = magics; }

        public string ProviderName { get; set; }
        public new string Extension { get { return base.Extension; } set { base.Extension = value; } }
        public override string Description { get { return string.Format ("Limada ThingStore ({0})", ProviderName); } protected set { base.Description = value; } }
        public override string MimeType { get { return string.Format ("LimadaThingStore/{0}", ProviderName); } protected set { base.MimeType = value; } }
    }

    public class ThingQuoreContentSpot : ContentDetector {

        public ThingQuoreContentSpot () : base (new ContentInfo[0]) { }

        public static long ContentType = unchecked ((long) 0xb4764a9d78c73d90);

        private IList<ThingStoreContentInfo> _contentSpecs = new List<ThingStoreContentInfo> ();

        private IList<ThingQuoreFactory> _quoreFactories = new List<ThingQuoreFactory> ();
        public override IEnumerable<ContentInfo> ContentSpecs { get { return _contentSpecs; } protected set { } }

        public virtual void Add (ThingQuoreFactory factory) {
            _quoreFactories.Add (factory);
        }

        public virtual ThingQuoreFactory GetFactory (IDbProvider provider) {
            return _quoreFactories.Where (s => s.Supports (provider)).FirstOrDefault ();
        }
    }
}