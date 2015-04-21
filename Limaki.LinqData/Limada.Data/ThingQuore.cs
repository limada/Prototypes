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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Limada.Model;
using Limaki.Common.Linqish;
using Limaki.Contents;
using Limaki.Data;
using Id = System.Int64;

namespace Limada.Data {

    public class ThingQuore : IThingQuore {

        public ThingQuore (Func<IQuore> createQuore) {
            this.CreateQuore = () => {
                var c = createQuore ();
                c.Log = this.Log;
                return c;
            };
        }

        public Func<IQuore> CreateQuore { get; protected set; }

        ExpressionCache _expressionCache = null;
        public ExpressionCache ExpressionCache {
            get { return _expressionCache ?? (_expressionCache = new ExpressionCache ()); }
            set { _expressionCache = value; }
        }

        protected ThingQuoreMapper _mapper;
        protected virtual ThingQuoreMapper Mapper  {
            get { return _mapper ?? (_mapper = new ThingQuoreMapper { ExpressionCache = this.ExpressionCache }); }
        }

        IQuore _quore = null;
        public virtual IQuore Quore {
            get {
                if (_quore == null) {
                    var dataStore = CreateQuore ();
                    _quore = new MappingQuore (dataStore, Mapper) { Disposed = () => _quore = null };
                }
                return _quore;

            }
        }

        public virtual IQueryable<IThing> Things {
            get { return Quore.GetQuery<IThing> (); }
        }

        public virtual IQueryable<ILink> Links {
            get { return Quore.GetQuery<ILink> (); }
        }

        public virtual IQueryable<ILink<long>> IdLinks {
            get { return Quore.GetQuery<ILink<Id>> (); }
        }

        public virtual IQueryable<IThing<string>> StringThings {
            get { return Quore.GetQuery<IThing<string>> (); }
        }

        public virtual IQueryable<INumberThing> NumberThings {
            get { return Quore.GetQuery<INumberThing> (); }
        }

        public virtual IQueryable<IStreamThing> StreamThings {
            get { return Quore.GetQuery<IStreamThing> (); }
        }

        public virtual IQueryable<IIdContent<Id, byte[]>> StreamBytes {
            get { return Quore.GetQuery<IIdContent<Id, byte[]>> (); }
        }

        public virtual IEnumerable<IThing> WhereThings (Expression<Func<IThing, bool>> predicate) {
            return
                Things.Where (predicate).Union (
                    StringThings.Where (predicate)).Union (
                        StreamThings.Where (predicate)).Union (
                            NumberThings.Where (predicate)).Union (
                                Links.Where (predicate));
        }

        public virtual void Upsert<T> (IEnumerable<T> things) where T : IThing {

            things.ForEach (e => {
                if (e.CreationDate == default(DateTime))
                    e.SetCreationDate (DateTime.Now);
            });

            Quore.Upsert<IThing> ((IEnumerable<IThing>) things.Where (t => t.GetType () == typeof (Thing)));
            Quore.Upsert (things.OfType<IThing<string>> ());
            Quore.Upsert (things.OfType<IStreamThing> ());
            Quore.Upsert (things.OfType<IIdContent<Id, byte[]>> ());
            Quore.Upsert (things.OfType<INumberThing> ());
            Quore.Upsert (things.OfType<ILink> ());
        }

        public virtual void Remove (IEnumerable<Id> ids) {
            foreach (var id in ids) {
                Quore.Remove<ILink> (e => e.Id == id);
                Quore.Remove<IThing> (e => e.Id == id);
                Quore.Remove<IThing<string>> (e => e.Id == id);
                Quore.Remove<INumberThing> (e => e.Id == id);
                Quore.Remove<IStreamThing> (e => e.Id == id);
                Quore.Remove<IIdContent<Id>> (e => e.Id == id);
                
            }
        }

        public virtual void UpsertContents (IEnumerable<IIdContent<Id>> contents) {
            Quore.Upsert (contents);
        }

        TextWriter _log = null;

        public virtual TextWriter Log {
            get { return _log ?? (_log = new StringWriter ()); }
            set { _log = value; }
        }

        public virtual void Dispose () {
            if (_quore != null)
                _quore.Dispose ();
            _quore = null;
        }

    }
}