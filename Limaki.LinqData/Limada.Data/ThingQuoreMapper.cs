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
using System.Linq;
using System.Linq.Expressions;
using Limada.Model;
using Limaki.Common.Linqish;
using Limaki.Contents;
using Limaki.Data;
using Id = System.Int64;

namespace Limada.Data {

    public class ThingQuoreMapper : IQuoreMapper {

        #region Expression-Conversion

        ExpressionCache _expressionCache = null;
        public ExpressionCache ExpressionCache {
            get { return _expressionCache ?? (_expressionCache = new ExpressionCache ()); }
            set { _expressionCache = value; }
        }

        protected Expression ChangeExpr<T> (Expression expr) {
            var type = typeof (T);
            return ExpressionChangerVisit.Change (expr, type, MapIn (type));
        }

        /// <summary>
        /// this is only needed if
        /// InnerContext does not support local constants or
        /// InnerContext does not support interface lamda expressions on entities
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public virtual Expression Map (Expression arg, Type queryType) {

            var expr = EvaluatingExpressionVisitor.Evaluate (arg);
            if (ExpressionCache.TryGet (ref expr)) {
                return expr;
            }

            expr = ChangeExpr<IThing> (expr);
            expr = ChangeExpr<ILink> (expr);
            expr = ChangeExpr<ILink<Id>> (expr);
            expr = ChangeExpr<IThing<string>> (expr);
            expr = ChangeExpr<INumberThing> (expr);
            expr = ChangeExpr<IStreamThing> (expr);

            expr = ChangeExpr<IIdContent<Id, byte[]>> (expr);
            expr = ChangeExpr<IIdContent<Id>> (expr);

            ExpressionCache.Add (expr);

            return expr;

        }

        #endregion

        public virtual Type MapIn (Type baseType) {

            if (baseType == typeof (ILink))
                return typeof (Link);

            if (baseType == typeof (ILink<long>))
                return typeof (IdLink);

            if (baseType == typeof (IThing<string>))
                return typeof (Thing<string>);

            if (baseType == typeof (IStreamThing))
                return typeof (StreamThing);

            if (baseType == typeof (INumberThing))
                return typeof (NumberThing);

            if (baseType == typeof (IIdContent<long, byte[]>))
                return typeof (RealData<byte[]>);

            if (baseType == typeof (IIdContent<long>))
                return typeof (RealData<byte[]>);

            if (baseType == typeof (IThing))
                return typeof (Thing);


            return null;
        }

        public virtual IEnumerable<T> MapIn<T> (IEnumerable<T> entities) {
            return entities;
        }

        public virtual IQueryable<T> MapQuery<T> (IQuore store) {

            if (typeof (IdLink) == typeof (T))
                return (IQueryable<T>) store.GetQuery<Link> ()
                    .Select (l => new IdLink { Id = l.Id, Root = l.RootId, Leaf = l.LeafId, Marker = l.MarkerId });

            return null;
        }

        public Expression<Func<T, IThing>> SelectThing<T> () where T : IThing {
            Expression<Func<T, IThing>> result = t => new Thing { Id = t.Id, ChangeDate = t.ChangeDate, CreationDate = t.CreationDate };
            return result;
        }

        public virtual IEnumerable<IThing> WhereThings (IThingQuore quore, Expression<Func<IThing, bool>> predicate) {
            return
                quore.Things.Select (SelectThing<IThing>()).Where (predicate).Union (
                    quore.StringThings.Select (SelectThing<IThing<string>> ()).Where (predicate)).Union (
                        quore.StreamThings.Select (SelectThing<IStreamThing> ()).Where (predicate)).Union (
                            quore.NumberThings.Select (SelectThing<INumberThing> ()).Where (predicate)).Union (
                                quore.Links.Select (SelectThing<ILink> ()).Where (predicate));
        }
    }
}