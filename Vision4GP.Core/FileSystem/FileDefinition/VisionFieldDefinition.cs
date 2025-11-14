namespace Vision4GP.Core.FileSystem
{

    /// <summary>
    /// Describes the structure of a Vision field
    /// </summary>
    public class VisionFieldDefinition
    {

        /// <summary>
        /// Name of the field
        /// </summary>
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Offset
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Bytes { get; set; }

        /// <summary>
        /// Size in chars/digits
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Scale (available only for numeric fields)
        /// </summary>
        public int Scale { get; set; }


        /// <summary>
        /// True if the field is signed (available only for numeric fields)
        /// </summary>
        public bool IsSigned { get; set; } = false;


        /// <summary>
        /// Level in the FD section
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// True for group fields (field-condition = 999)
        /// </summary>
        public bool IsGroupField { get; set; } = false;

        /// <summary>
        /// Data type
        /// </summary>
        public VisionFieldType FieldType { get; set; }


        /// <summary>
        /// Clone the field definition
        /// </summary>
        /// <returns>Copy of the field definition</returns>
        public VisionFieldDefinition Clone()
        {
            return new VisionFieldDefinition
            {
                Bytes = Bytes,
                FieldType = FieldType,
                IsGroupField = IsGroupField,
                IsSigned = IsSigned,
                Level = Level,
                Name = Name,
                Offset = Offset,
                Scale = Scale,
                Size = Size
            };
        }
    }
}
