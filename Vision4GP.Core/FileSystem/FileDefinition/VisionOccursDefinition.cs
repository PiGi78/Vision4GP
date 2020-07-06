using System.Collections.Generic;

namespace Vision4GP.Core.FileSystem
{


    /// <summary>
    /// Definition of a Vision array (occurs clause)
    /// </summary>
    public class VisionOccursDefinition
    {

        /// <summary>
        /// How many times the element is repeted (OCCURS n TIMES clause)
        /// </summary>
        public int Count { get; set; }


        /// <summary>
        /// Total size of the occurs (sum of all fields + filler)
        /// </summary>
        public int Size { get; set; }


        /// <summary>
        /// Fields of the occurs
        /// </summary>
        public List<VisionFieldDefinition> Fields { get; } = new List<VisionFieldDefinition>();


        /// <summary>
        /// Nested occurses
        /// </summary>
        public List<VisionOccursDefinition> InnerOccurses { get; } = new List<VisionOccursDefinition>();

    }


}
