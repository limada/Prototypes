
namespace Tridles.Tridles {

    /// <summary>
    /// Tridle is a triple with an id
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class Tridle<K, V> : ITridle<K,V> {

        public K Id { get; set; }
        public K Key { get; set; }
        public K Member { get; set; }
        public V Value { get; set; }

        public override string ToString () {
            // TODO: make formatstring static to avoid typecheck
            if (typeof (K) == typeof (long))
                return string.Format ("{{Id = {0:X16} Key = {1:X16} Member = {2:X16} Value = {3}}}", Id, Key, Member, Value);
            return string.Format ("{{Id = {0} Key = {1} Member = {2} Value = {3}}}", Id, Key, Member, Value);
        }
    }
}