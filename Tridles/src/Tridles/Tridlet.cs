using System;
using System.Linq.Expressions;

namespace Tridles.Tridles {

    public class Tridlet<K> {

        public Tridlet () {
            MetaId = new MetaId<K> ();
        }
        /// <summary>
        /// func to create a unique id
        /// </summary>
        public Func<K> CreateId { get; set; }

        public MetaId<K> MetaId { get; protected set; }

        public Tridle<K, V> Create<V> (K key, K member, V value) {
            return new Tridle<K, V> { Id = CreateId (), Key = key, Member = member, Value = value };
        }

        public Tridle<K, V> Create<V> (K id, K key, K member, V value) {
            return new Tridle<K, V> { Id = id, Key = key, Member = member, Value = value };
        }

        public ITridle<K> Create (K id, K key, K member, Type valueType, object value) {
            // TODO: this is slow; cache it
            Expression<Action> lambda = () => Create<object> (id, key, member, null);
            var method = (lambda.Body as MethodCallExpression).Method
                .GetGenericMethodDefinition ().MakeGenericMethod (valueType);

            return (ITridle<K>) method.Invoke (this, new object[] { id, key, member, value });
        }

        public ITridle<K> Create (K key, K member, Type valueType, object value) {
            // TODO: this is slow; cache it
            Expression<Action> lambda = () => Create<object> (key, member, null);
            var method = (lambda.Body as MethodCallExpression).Method
                .GetGenericMethodDefinition ().MakeGenericMethod (valueType);

            return (ITridle<K>) method.Invoke (this, new object[] { key, member, value });
        }
    }
}