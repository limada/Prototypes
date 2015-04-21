using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Limada.Model;
using Limaki.Common;
using System.Linq;
using Limaki.Common.Linqish;

namespace Limada.Data {

    public static class QThingExtensions {

        public const int Thing = 1;
        public const int StringThing = 2;
        public const int Link = 3;
        public const int StreamThing = 5;
        public const int NumberThing = 6;

        public static Func<int, Expression<Func<IThing, QThing>>> Selector = type => thing =>
           new QThing {
               Id = thing.Id,
               CreationDate = thing.CreationDate,
               ChangeDate = thing.ChangeDate,
               Type = type,
           };

        public static int DType<T> () {
            if (typeof (IThing) == typeof (T))
                return Thing;
            if (typeof (ILink) == typeof (T))
                return Link;
            if (typeof (IThing<string>) == typeof (T) || typeof (IStringThing) == typeof (T))
                return StringThing;
            if (typeof (IStreamThing) == typeof (T))
                return StreamThing;
            if (typeof (INumberThing) == typeof (T))
                return NumberThing;
            throw new ArgumentException (string.Format ("Type {0} not supported", typeof (T).Name));
        }

        public static Expression<Func<IThing, QThing>> ToQThing<T> () { // remark: don't use where T:IThing
            return Selector (DType<T> ());
        }

        public static IQueryable<QThing> ToQThing<T> (this IQueryable<IThing> things)  { // remark: don't use where T:IThing
            return things.Select (ToQThing<T> ());
        }

        public static IQueryable<QThing> ToQThing<T> (this IQueryable<IThing> things, Expression<Func<IThing, bool>> predicate) {
            var r =  things.Where(predicate);
            return r.Select<IThing, QThing> (ToQThing<T> ());
        }

        /// <summary>
        /// this doesn't work with IThing{T} in Ef6
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="qThings"></param>
        /// <param name="things"></param>
        /// <returns></returns>
        public static IQueryable<T> JoinQThing<T> (this IQueryable<QThing> qThings, IQueryable<T> things) where T : IThing {
            var r =  qThings.Where (a => a.Type == DType<T>())
                .Join (things, a => a.Id, c => c.Id, (a, c) => c);
            return r;
        }

        public static IQueryable<QThing> QThingsOf<T> (this IQueryable<QThing> qThings) where T : IThing {
            var r = qThings.Where (a => a.Type == DType<T> ());
            return r;
        }

    }

    public class QThing  {

        public int Type { get; set; }

        public Int64 Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ChangeDate { get; set; }

    }
}