using System.Linq;
using System.Xml;

namespace Mapping.Common
{
    /// <summary>
    /// A storage class for various extension methods.
    /// Created by Timothy J Cowen.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Checks whether an <c>XmlNode</c> object contains the specified children.
        /// The node must contain all specified children for this method to return true.
        /// </summary>
        /// <param name="node">The parent node</param>
        /// <param name="children">The children to find</param>
        /// <returns>True if the parent contains all specified children; false otherwise</returns>
        public static bool Contains(this XmlNode node, params string[] children)
        {
            // Iterate through children of parent node and check that the specified ones exist
            return children.All(name =>
                node.ChildNodes.Cast<XmlNode>().Any(childNode =>
                    childNode.Name.Equals(name)));
        }
    }
}
