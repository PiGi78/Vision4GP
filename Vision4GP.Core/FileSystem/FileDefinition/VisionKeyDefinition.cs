using System.Collections.Generic;
using System.Text;

namespace Vision4GP.Core.FileSystem
{

    /// <summary>
    /// Describe the structure of a vision file key
    /// </summary>
    public class VisionKeyDefinition
    {


        /// <summary>
        /// True if it is a unique key (no duplicate allowed)
        /// </summary>
        public bool IsUnique { get; set; }


        /// <summary>
        /// Fields that compose the key
        /// </summary>
        public List<VisionFieldDefinition> Fields { get; } = new List<VisionFieldDefinition>();


        /// <summary>
        /// Segments of the key
        /// </summary>
        public List<VisionKeySegment> Segments { get; } = new List<VisionKeySegment>();


        /// <summary>
        /// Key definition in the V6 info string provided by Microfocus
        /// </summary>
        /// <remarks>
        /// Needed to check the index of the key for the start statement
        /// </remarks>
        public string V6InfoString
        {
            get
            {
                var builder = new StringBuilder()
                    .Append(Segments.Count.ToString("00"))
                    .Append(",")
                    .Append(IsUnique ? "0" : "1");
                foreach (var segment in Segments)
                {
                    builder.Append(",")
                           .Append(segment.Size.ToString("000"))
                           .Append(",")
                           .Append(segment.Offset.ToString("0000000000"));
                }
                return builder.ToString();
            }
        }

    }

}
