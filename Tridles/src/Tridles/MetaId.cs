
namespace Tridles.Tridles {
   
    /// <summary>
    /// defines the metadata constants
    /// </summary>
    /// <typeparam name="K"></typeparam>
    public class MetaId<K> {
        /// <summary>
        /// Id of a type
        /// stored as key of a type tridle
        /// </summary>
        public K Type { get; set; }

        /// <summary>
        /// name of a type
        /// stored as member of a type tridle
        /// the value is typeof(clazz).FullName
        /// </summary>
        public K TypeName { get; set; }

        /// <summary>
        /// member of a type
        /// stored as member of a tridle
        /// the value is nameof(type.Member)
        /// the key is the Id of the type-tridle
        /// </summary>
        public K TypeMember { get; set; }

        /// <summary>
        /// dynamic property of a class
        /// stored as member of a tridle
        /// the value is name of the dynamic property
        /// the key is the Id of the type-tridle
        /// </summary>
        public K Dyn { get; set; }

        /// <summary>
        /// type of dynamic property of a class
        /// stored as member of a tridle
        /// the value is the Type.FullName of the property type
        /// the key is the id of the DynId-tridle
        /// </summary>
        public K DynType { get; set; }

        public override string ToString () {
            // TODO: make formatstring static to avoid typecheck
            if (typeof (K) == typeof (long))
                return string.Format ("{{Type = {0:X16} TypeName = {1:X16} TypeMember = {2:X16} Dyn = {3:X16} DynType = {4:X16} }}", Type, TypeName, TypeMember, Dyn, DynType); ;
            return string.Format ("{{Type = {0} TypeName = {1} TypeMember = {2} Dyn = {3} DynType = {4}}}", Type, TypeName, TypeMember, Dyn, DynType);
        }
    }
}