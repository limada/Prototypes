
namespace Tridles.Tridles {

    public interface ITridle<K> {
        K Id { get; set; }
        K Key { get; set; }
        K Member { get; set; }
    }

    public interface ITridle<K, V>:ITridle<K> {
        V Value { get; set; }
    }
}