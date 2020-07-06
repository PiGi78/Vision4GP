namespace Vision4GP.Core.FileSystem
{


    /// <summary>
    /// Vision data type
    /// </summary>
    public enum VisionFieldType
    {
        /// <summary>
        /// Text field 
        /// </summary>
        String = 0,
        /// <summary>
        /// Right aligned text (justified clause)
        /// </summary>
        JustifiedString = 1,
        /// <summary>
        /// Date or date/time
        /// </summary>
        Date = 10,
        /// <summary>
        /// Numeric field
        /// </summary>
        Number = 20,
        /// <summary>
        /// Comp field
        /// </summary>
        Comp = 30
    }

}
