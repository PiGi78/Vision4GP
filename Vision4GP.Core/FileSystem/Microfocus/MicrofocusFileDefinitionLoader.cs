using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Vision4GP.Core.FileSystem;

namespace Vision4GP.Core.Microfocus
{

    /// <summary>
    /// Microfocus file definition factory
    /// </summary>
    internal class MicrofocusFileDefinitionLoader : IFileDefinitionLoader
    {

        // Microfocus namespace
        private static readonly XNamespace XfdNs = @"http://www.microfocus.com";


        /// <summary>
        /// Gets a vision file definition based on its XFD file
        /// </summary>
        /// <param name="xfdFilePath">XFD file path</param>
        /// <returns>Requested file definition</returns>
        public VisionFileDefinition LoadFromFile(string xfdFilePath)
        {
            if (string.IsNullOrEmpty(xfdFilePath)) throw new ArgumentNullException(nameof(xfdFilePath));
            if (!File.Exists(xfdFilePath)) throw new FileNotFoundException("XFD file not found", xfdFilePath);

            // Load file
            var xfd = XElement.Load(xfdFilePath);

            // Result
            var result = new VisionFileDefinition();

            // Identification data
            var identification = xfd.Element(XfdNs + "identification");
            if (identification == null)
            {
                throw new IOException(string.Format("Invalid xfd file: {0}", xfdFilePath));
            }

            result.SelectName = (string)identification.Element(XfdNs + "select-name");
            result.FileName = ((string)identification.Element(XfdNs + "table-name")).ToLowerInvariant();
            result.Alphabet = (string)identification.Element(XfdNs + "alphabet");
            result.MinRecordSize = (int)identification.Element(XfdNs + "minimum-record-size");
            result.MaxRecordSize = (int)identification.Element(XfdNs + "maximum-record-size");
            result.NumberOfKeys = (int)identification.Element(XfdNs + "number-of-keys");

            // Fields
            var fields = xfd.Element(XfdNs + "fields");
            foreach (var field in fields.Elements(XfdNs + "field"))
            {
                result.Fields.Add(GetFieldDefiniton(field));
            }

            // Occurs (Load fields with index suffix)
            foreach (var occurs in GetOccurses(fields))
            {
                result.Occurses.Add(occurs);
            }

            // Keys
            XElement keys = xfd.Element(XfdNs + "keys");
            foreach (XElement key in keys.Elements(XfdNs + "key"))
            {
                var vKey = new VisionKeyDefinition
                {
                    IsUnique = !(bool)key.Attribute(XfdNs + "duplicates-allowed")
                };
                // Key columns
                foreach (XElement keyColumn in key.Element(XfdNs + "key-columns")?.Elements(XfdNs + "key-column"))
                {
                    string fieldName = (string)keyColumn.Attribute(XfdNs + "key-column-name");
                    vKey.Fields.Add(result.Fields.Single(x => x.Name == fieldName));
                }
                // Key segments
                foreach (var segment in key.Elements(XfdNs + "segments").Elements(XfdNs + "segment"))
                {
                    var strSize = (string)segment.Attribute(XfdNs + "segment-size");
                    var strOffset = (string)segment.Attribute(XfdNs + "segment-offset");
                    if (int.TryParse(strSize, out int size) &&
                        int.TryParse(strOffset, out int offset))
                    {
                        vKey.Segments.Add(new VisionKeySegment
                        {
                            Offset = offset,
                            Size = size
                        });
                    }
                }

                result.Keys.Add(vKey);
            }

            return result;
        }


        /// <summary>
        /// Load occurs fields
        /// </summary>
        /// <param name="element">XElement from where load Occurs</param>
        private IEnumerable<VisionOccursDefinition> GetOccurses(XElement element)
        {
            foreach (XElement occursElement in element.Elements(XfdNs + "field-occurs"))
            {
                var occurs = new VisionOccursDefinition();
                occurs.Count = (int)occursElement.Attribute(XfdNs + "occurs-count");
                occurs.Size = (int)occursElement.Attribute(XfdNs + "occurs-size");
                // Occurs fields
                foreach (XElement item in occursElement.Elements(XfdNs + "field"))
                {
                    var field = GetFieldDefiniton(item);
                    occurs.Fields.Add(field);
                }
                // internal occurs
                foreach (var inner in GetOccurses(occursElement))
                {
                    occurs.InnerOccurses.Add(inner);
                }
                yield return occurs;
            }
        }


        /// <summary>
        /// Gets a field definition form an XElement
        /// </summary>
        /// <param name="field">XElement that describes the field</param>
        /// <returns>The requested field definition</returns>
        private VisionFieldDefinition GetFieldDefiniton(XElement field)
        {
            var result = new VisionFieldDefinition
            {
                Name = (string)field.Attribute(XfdNs + "field-name"),
                Size = (int)field.Attribute(XfdNs + "field-length"),
                Scale = (int)field.Attribute(XfdNs + "field-scale") * -1,
                Offset = (int)field.Attribute(XfdNs + "field-offset"),
                Bytes = (int)field.Attribute(XfdNs + "field-bytes"),
                Level = (int)field.Attribute(XfdNs + "field-level")
            };

            // Group field
            result.IsGroupField = (int)field.Attribute(XfdNs + "field-condition") == 999;

            // Data type
            var fieldType = (int)field.Attribute(XfdNs + "field-type");
            var userFlag = (int)field.Attribute(XfdNs + "field-user-flags");
            switch (fieldType)
            {
                case 1: // Unsigned numeric
                    result.FieldType = VisionFieldType.Number;
                    result.IsSigned = false;
                    if (userFlag == 1)
                    {
                        result.FieldType = VisionFieldType.Date;
                    }
                    break;
                case 2: // Signed numeric
                    result.FieldType = VisionFieldType.Number;
                    result.IsSigned = true;
                    break;
                case 16: // Alfanumeric
                case 18: // Alphabetic
                case 20: // Alphanumeric edited
                    result.FieldType = VisionFieldType.String;
                    result.IsSigned = false;
                    break;
                case 17: // Alphanumeric justified
                case 19: // Alphabetic justified
                case 21: // Alphanumerico edited justified
                    result.FieldType = VisionFieldType.JustifiedString;
                    result.IsSigned = false;
                    break;
                default: // Other
                    result.FieldType = VisionFieldType.String;
                    result.IsSigned = false;
                    break;
            }


            // Comp field 
            if (result.Bytes < result.Size)
            {
                result.Size = result.Bytes;
                result.FieldType = VisionFieldType.Comp;
                result.Scale = 0;
            }


            return result;
        }


    }
}
