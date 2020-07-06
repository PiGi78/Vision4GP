using System.Collections.Generic;

namespace Vision4GP.Core.FileSystem
{
    /// <summary>
    /// Vision file definition
    /// </summary>
    public class VisionFileDefinition
    {

        /// <summary>
        /// Logical file name
        /// </summary>
        public string SelectName { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File Alphabet (usually ASCII)
        /// </summary>
        public string Alphabet { get; set; }

        /// <summary>
        /// Number of keys
        /// </summary>
        public int NumberOfKeys { get; set; }

        /// <summary>
        /// Minimal record size
        /// </summary>
        public int MinRecordSize { get; set; }

        /// <summary>
        /// Maximal record size
        /// </summary>
        public int MaxRecordSize { get; set; }

        /// <summary>
        /// File keys
        /// </summary>
        public List<VisionKeyDefinition> Keys { get; set; } = new List<VisionKeyDefinition>();


        /// <summary>
        /// Fields
        /// </summary>
        public List<VisionFieldDefinition> Fields { get; set; } = new List<VisionFieldDefinition>();

        /// <summary>
        /// Occurses
        /// </summary>
        public List<VisionOccursDefinition> Occurses { get; set; } = new List<VisionOccursDefinition>();

    }

}
