namespace Tridles.Tridles {

    public interface IDynValueStore<T> {
        /// <summary>
        /// stores value of entity
        /// </summary>
        /// <typeparam name="V">type of value to store</typeparam>
        /// <param name="entity">entity the value belongs to</param>
        /// <param name="name">name of the value (dynamic property name)</param>
        /// <param name="value">value to store</param>
        void SetDynValue<V> (T entity, string name, V value);

        V GetDynValue<V> (T entity, string name);
    }

}