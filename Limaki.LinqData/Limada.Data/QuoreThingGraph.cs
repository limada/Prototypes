/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014-2015 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Limada.Model;
using Limaki.Common.Linqish;
using Limaki.Contents;
using Limaki.Data;
using Mono.Linq.Expressions;
using Id = System.Int64;

namespace Limada.Data {

    /// <summary>
    /// a <see cref="IThingGraph"/> backed by a <see cref="IThingQuore"/>
    /// </summary>
    public class QuoreThingGraph : DbGraph<IThing, ILink>, IThingGraph, IContentContainer<Id> {

        public QuoreThingGraph (Func<IThingQuore> createQuore) {
            this.CreateQuore = createQuore;
        }

        public Func<IThingQuore> CreateQuore { get; protected set; }

        #region DbGraph

        public override bool ValidEdge (ILink edge) { return base.ValidEdge (edge) && edge.Marker != null; }

        public override void Add (ILink edge) {
            if (edge != null)
                try {
                    CheckEdge (edge);
                    AddEdge (edge, edge.Marker);
                    base.Add (edge);
                } catch (Exception e) {
                    throw e;
                } finally { }
            
        }

        protected override void Upsert (IThing item) {
            var link = item as ILink;
            if (link != null) {
                Upsert (link);
                return;
            }
            using (var quore = CreateQuore ())
                quore.Upsert (new IThing[] { item });
        }

        protected override void Upsert (ILink edge) {
            using (var quore = CreateQuore ())
                quore.Upsert (new ILink[] { edge });
        }

        protected override void Delete (IThing item) {
            EvictItem (item);
            var link = item as ILink;
            if (link != null) {
                Delete (link);
                return;
            }
            using (var quore = CreateQuore ())
                quore.Remove (new Id[] { item.Id });
        }

        protected override void Delete (ILink edge) {
            EvictItem (edge);
            using (var quore = CreateQuore ())
                quore.Remove (new Id[] { edge.Id });
        }

        public override void Flush () {

        }

        protected override ICollection<ILink> EdgesOf (IThing item) {
            using (var quore = CreateQuore ()) {
                var edges = ActivateLinks (quore
                    .Links.Cast<Link> ()
                    .Where (e => e.LeafId == item.Id || e.RootId == item.Id),
                    quore);
                return edges.OfType<ILink> ()
                    .ToList ();
            }
        }

        protected virtual IEnumerable<IThing> ItemsQ (IThingQuore quore) {
            return
                quore
                    .Things.Yield().Union (quore
                        .StringThings.Yield ()).Union (quore
                            .StreamThings.Yield ()).Union (quore
                                .NumberThings.Yield ());
        }

        protected override IEnumerable<IThing> Items {
            get {
                using (var quore = CreateQuore ()) {
                    foreach (var item in ItemsQ (quore))
                        yield return Activate (item, quore);
                }
                   
            }
        }

        public override bool Contains (ILink edge) {
            if (edge == null)
                return false;

            if (ThingCache.ContainsKey (edge.Id))
                return true;

            using (var quore = CreateQuore ())
                return quore.Links.Any (e => e.Id == edge.Id);
        }

        public override int EdgeCount (IThing item) {
            using (var quore = CreateQuore ())
                return quore.IdLinks.Where (e => e.Leaf == item.Id || e.Root == item.Id).Count ();
        }

        public override IEnumerable<ILink> Edges () {
            using (var quore = CreateQuore ())
                return ActivateLinks (quore.Links, quore)
                    .ToArray ();
        }

        protected IQueryable<Id> IdUnionOfAll (IThingQuore quore) {
            return
                 quore.Things.Select (e => e.Id).Union (
                     quore.StringThings.Select (e => e.Id)).Union (
                         quore.StreamThings.Select (e => e.Id)).Union (
                             quore.NumberThings.Select (e => e.Id)).Union (
                                 quore.Links.Select (e => e.Id))
                 ;
        }

        protected IQueryable<Id> IdUnionOfAll (IThingQuore quore, Expression<Func<IThing, bool>> where) {
            return
                quore.Things.Where (where).Select (e => e.Id).Union (
                    quore.StringThings.Where (where).Select (e => e.Id)).Union (
                        quore.StreamThings.Where (where).Select (e => e.Id)).Union (
                            quore.NumberThings.Where (where).Select (e => e.Id)).Union (
                                quore.Links.Where (where).Select (e => e.Id))
                ;
        }

        public override bool Contains (IThing item) {
            if (item == null)
                return false;

            if (ThingCache.ContainsKey (item.Id))
                return true;

            using (var quore = CreateQuore ()) {
                Expression<Func<IThing, bool>> where = e => e.Id == 0;
                where = ExpressionUtils.ReplaceBody (where, Expression.Constant (item.Id), true);

                var stringThing = item as IThing<string>;
                if (stringThing != null)
                    return quore.StringThings.Any (where);

                var streamThing = item as IStreamThing;
                if (streamThing != null)
                    return quore.StreamThings.Any (where);

                var link = item as ILink;
                if (link != null)
                    return quore.Links.Any (where);

                var numberThing = item as INumberThing;
                if (numberThing != null)
                    return quore.NumberThings.Any (where);

                return quore.Things.Any (where);
            }
        }

        public override int Count {
            get {
                using (var quore = CreateQuore ())
                    return quore.Things.Count () +
                           quore.StringThings.Count () +
                           quore.StreamThings.Count () +
                           quore.NumberThings.Count () +
                           quore.Links.Count ();
            }
        }

        public override IEnumerator<IThing> GetEnumerator () {
            using (var quore = CreateQuore ()) {
                return Activate (ItemsQ (quore).ToArray(),quore)
                    .Union (ActivateLinks(quore.Links,quore))
                    .GetEnumerator ();
                ;
            }
        }

        public override void Clear () {
            using (var quore = CreateQuore ())
                quore.Remove (IdUnionOfAll (quore).ToList ());
            ClearCaches ();
        }

        public override void CopyTo (IThing[] array, int arrayIndex) {
            throw new NotSupportedException ();
        }

        public override bool IsReadOnly {
            get { return false; }
        }

        public override IEnumerable<IThing> WhereQ (Expression<Func<IThing, bool>> predicate) {
            using (var quore = CreateQuore ())
                return WhereQInternal (quore, predicate)
                    .ToArray ();
        }

        protected virtual IEnumerable<IThing> WhereQInternal (IThingQuore quore, Expression<Func<IThing, bool>> predicate) {
            return Activate (quore.WhereThings (predicate), quore);
        }

        #endregion

        #region IThingGraph

        public IEnumerable<T> WhereQ<T> (Expression<Func<T, bool>> predicate) where T : IThing {
            Func<IEnumerable<T>, IEnumerable<T>> result = r => r.ToArray ();
            using (var quore = CreateQuore ()) {
                var thing = predicate as Expression<Func<IThing, bool>>;
                if (thing != null)
                    return result (WhereQInternal (quore, thing) as IEnumerable<T>);

                var link = predicate as Expression<Func<ILink, bool>>;
                if (link != null)
                    return result (ActivateLinks (quore.Links.Where (link), quore) as IEnumerable<T>);

                var stringW = predicate as Expression<Func<IThing<string>, bool>>;
                if (stringW != null)
                    return result (Activate (quore.StringThings.Where (stringW),quore) as IEnumerable<T>);

                var stream = predicate as Expression<Func<IStreamThing, bool>>;
                if (stream != null)
                    return result (Activate (quore.StreamThings.Where (stream), quore) as IEnumerable<T>);

                var number = predicate as Expression<Func<INumberThing, bool>>;
                if (number != null)
                    return result (Activate (quore.NumberThings.Where (number), quore) as IEnumerable<T>);

                // this is not a IThing
                var idlink = predicate as Expression<Func<ILink<Id>, bool>>;
                if (idlink != null)
                    return result (quore.IdLinks.Where (idlink) as IEnumerable<T>);
            }
            return new T[0];
        }

        public IThing GetById (Id id) {
            var result = default (IThing);
            if (ThingCache.TryGetValue (id, out result))
                return result;

            Expression<Func<IThing, bool>> where = e => e.Id == 0;
            where = ExpressionUtils.ReplaceBody (where, Expression.Constant (id), true);

            using (var quore = CreateQuore ())
                return WhereQInternal (quore, where).FirstOrDefault ();
        }
        
        protected IThing GetById (Id id, IThingQuore quore) {
            var result = default (IThing);
            if (ThingCache.TryGetValue (id, out result))
                return result;

            Expression<Func<IThing, bool>> where = e => e.Id == 0;
            where = ExpressionUtils.ReplaceBody (where, Expression.Constant (id), true);

            return WhereQInternal (quore, where).FirstOrDefault ();
        }

        public IEnumerable<IThing> GetByData (object data, bool exact) {

            if (data is string) {
                var val = (string) data;
                Expression<Func<IThing<string>, bool>> where = e => e.Data == null;
                where = ExpressionUtils.ReplaceBody (where, Expression.Constant (val), true);
                return WhereQ<IThing<string>> (where);
            }

            Expression<Func<INumberThing, bool>> nwhere = e => e.Data == 0;
            if (data is Int64) {
                var val = (Int64) data;
                nwhere = ExpressionUtils.ReplaceBody (nwhere, Expression.Constant (val), true);
                return WhereQ<INumberThing> (nwhere.AndAlso (e => e.NumberType == NumberType.Long));
            }

            if (data is double) {
                var val = LongConverters.DoubleToLong ((double) data);
                nwhere = ExpressionUtils.ReplaceBody (nwhere, Expression.Constant (val), true);
                return WhereQ<INumberThing> (nwhere.AndAlso (e => e.NumberType == NumberType.Double));
            }

            if (data is DateTime) {
                var val = LongConverters.DateTimeToLong ((DateTime) data);
                nwhere = ExpressionUtils.ReplaceBody (nwhere, Expression.Constant (val), true);
                return WhereQ<INumberThing> (nwhere.AndAlso (e => e.NumberType == NumberType.DateTime));
            }

            if (data is Int32) {
                var val = LongConverters.IntToLong ((Int32) data);
                nwhere = ExpressionUtils.ReplaceBody (nwhere, Expression.Constant (val), true);
                return WhereQ<INumberThing> (nwhere.AndAlso (e => e.NumberType == NumberType.Integer));
            }

            if (data is Quad16) {
                var val = LongConverters.Quad16ToLong ((Quad16) data);
                nwhere = ExpressionUtils.ReplaceBody (nwhere, Expression.Constant (val), true);
                return WhereQ<INumberThing> (nwhere.AndAlso (e => e.NumberType == NumberType.Quad16));
            }

            return new IThing[0];
        }

        public IEnumerable<IThing> GetByData (object data) {
            return GetByData (data, true);
        }

        #region Markers

        public bool IsMarker (IThing thing) {
            if (MarkerCache.ContainsKey (thing.Id))
                return true;

            Expression<Func<ILink<Id>, bool>> where = e => e.Marker == 0;
            where = ExpressionUtils.ReplaceBody (where, Expression.Constant (thing.Id), true);

            using (var quore = CreateQuore ())
                return quore.IdLinks.Any (where);
        }

        public void AddMarker (IThing marker) {
            MarkerCache[marker.Id] = marker;
        }

        private bool markersLoaded = false;
        public ICollection<IThing> Markers () {
            if (!markersLoaded)
                using (var quore = CreateQuore ()) {
                    var markerIds = quore.IdLinks.Select (e => e.Marker);
                    WhereQInternal (quore, e => markerIds.Contains (e.Id))
                        .ForEach (m => MarkerCache[m.Id] = m);
                    markersLoaded = true;
                }
            return MarkerCache.Values;
        }

        #endregion

        #region Caches

        private IDictionary<long, IThing> _things = null;
        protected IDictionary<long, IThing> ThingCache {
            get { return _things ?? (_things = new Dictionary<long, IThing> ()); }
        }

        protected Dictionary<long, Type> _typeIdCache = null;
        protected Dictionary<long, Type> TypeIdCache {
            get {
                if (_typeIdCache == null) {
                    _typeIdCache = new Dictionary<long, Type> ();
                }
                return _typeIdCache;
            }
        }

        private IDictionary<Id, IThing> _markerCache = null;
        protected IDictionary<long, IThing> MarkerCache {
            get { return _markerCache ?? (_markerCache = new Dictionary<Id, IThing> ()); }
        }

        protected override void ClearCaches () {
            _things = null;
            _markerCache = null;
            _typeIdCache = null;
            base.ClearCaches ();
        }

        protected void AddToCache (IThing thing) {
            if (thing == null) {
                return;
            }

            ThingCache[thing.Id] = thing;
            TypeIdCache[thing.Id] = thing.GetType ();
        }

        protected void RemoveFromCache (IThing thing) {
            if (thing == null) {
                return;
            }

            ThingCache.Remove (thing.Id);
            TypeIdCache.Remove (thing.Id);

        }

        protected void FillTypeIdCache (Dictionary<long, Type> cache) {

            using (var store = CreateQuore ()) {
                var log = store.Quore.Log;
                try {
                    store.Quore.Log = null;

                    foreach (var item in store.Things.Select (e => e.Id))
                        cache.Add (item, typeof (IThing));

                    foreach (var item in store.StringThings.Select (e => e.Id))
                        cache.Add (item, typeof (IThing<string>));

                    foreach (var item in store.StreamThings.Select (e => e.Id))
                        cache.Add (item, typeof (IStreamThing));

                    foreach (var item in store.NumberThings.Select (e => e.Id))
                        cache.Add (item, typeof (INumberThing));

                    foreach (var item in store.Links.Select (e => e.Id))
                        cache.Add (item, typeof (ILink));


                } catch (Exception e) {
                    throw e;
                } finally {
                    store.Quore.Log = log;
                }
            }
        }

        #endregion

        public IThing UniqueThing (IThing item) {
            if (item == null)
                return null;

            var result = item;
            if (!ThingCache.TryGetValue (item.Id, out result)) {
                ThingCache.Add (item.Id, item);
                TypeIdCache[item.Id] = item.GetType ();
                result = item;
            }

            return result;
        }

        public override void EvictItem (IThing item) {
            ThingCache.Remove (item.Id);
            TypeIdCache.Remove (item.Id);
            MarkerCache.Remove (item.Id);
        }

        protected IEnumerable<T> Activate<T> (IEnumerable<T> things, IThingQuore quore) where T : IThing {
            //if (!things.Any ()) // no need for that, yield gives back empty IEnumerable
            //    return new T[0];
            if (things is IQueryable)
                things = things.Yield ();
            var result = things
                .Select (e => Activate (e, quore));
            if (result is IEnumerable<T>)
                return (IEnumerable<T>) result;
            return result.Cast<T> ();
        }

        protected IEnumerable<T> ActivateLinks<T> (IEnumerable<T> links, IThingQuore quore) where T : ILink {
            //if (!things.Any ()) // no need for that, yield gives back empty IEnumerable
            //    return new T[0];
            if (links is IQueryable)
                links = links.ToArray ();
            
            var result = links
                    .Select (e => Activate (e, quore));
            if (result is IEnumerable<T>)
                return (IEnumerable<T>) result;
            return result.Cast<T> ();
        }

        protected IThing Activate (IThing item, IThingQuore quore) {
            if (item == null)
                return null;

            var result = UniqueThing (item);
            if (!object.Equals (item, result)) {
                // TODO: check if item is newer and merge
                return result;
            }

            item.State.Clean = true;

            var streamThing = item as IStreamThing;
            if (streamThing != null) {
                streamThing.ContentContainer = this.ContentContainer;
                return streamThing;
            }

            var link = item as ILink;
            if (link != null) {
                var idLink = (ILink<Id>) item;
                if (link.Leaf == null) {
                    link.Leaf = GetById (idLink.Leaf, quore);
                }

                if (link.Root == null) {
                    link.Root = GetById (idLink.Root, quore);
                }

                if (link.Marker == null) {
                    link.Marker = GetById (idLink.Marker, quore);
                }

                return link;
            }
            return item;
        }

        #endregion

        #region IContentContainer

        public IContentContainer<Id> ContentContainer {
            get { return this; }
            set { throw new NotImplementedException (); }
        }

        bool IContentContainer<Id>.Contains (Id id) {
            using (var quore = CreateQuore ())
                return quore.StreamBytes.Any (e => e.Id == id);
        }

        bool IContentContainer<Id>.Contains (IIdContent<Id> item) {
            using (var quore = CreateQuore ())
                return quore.StreamBytes.Any (e => e.Id == item.Id);
        }

        void IContentContainer<Id>.Add (IIdContent<Id> item) {
            using (var quore = CreateQuore ())
                quore.UpsertContents (new IIdContent<Id>[] { item });
        }

        IIdContent<Id> IContentContainer<Id>.GetById (Id id) {
            using (var quore = CreateQuore ())
                return quore.StreamBytes.Where (e => e.Id == id).FirstOrDefault ();
        }

        bool IContentContainer<Id>.Remove (Id id) {
            using (var quore = CreateQuore ())
                quore.Remove (new Id[] { id });
            return true;
        }

        bool IContentContainer<Id>.Remove (IIdContent<Id> item) {
            return ((IContentContainer<Id>) this).Remove (item.Id);
        }

        #endregion

    }
}