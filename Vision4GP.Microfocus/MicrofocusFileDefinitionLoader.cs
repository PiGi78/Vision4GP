using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Vision4GP.Core.FileSystem;

namespace Vision4GP.Microfocus
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

            result.SelectName = GetStringValueOrThrowException((string?)identification.Element(XfdNs + "select-name"), $"Element select-name not found in xfdFile: {xfdFilePath}");
            result.FileName = GetStringValueOrThrowException((string?)identification.Element(XfdNs + "table-name"), $"Element table-name not found in xfdFile: {xfdFilePath}")
                                                            .ToLowerInvariant();
            result.Alphabet = GetStringValueOrThrowException((string?)identification.Element(XfdNs + "alphabet"), $"Element alphabet not found in xfdFile: {xfdFilePath}");
            result.MinRecordSize = GetIntValueOrThrowException((int?)identification.Element(XfdNs + "minimum-record-size"), $"Element minimum-record-size not found in xfdFile: {xfdFilePath}");
            result.MaxRecordSize = GetIntValueOrThrowException((int?)identification.Element(XfdNs + "maximum-record-size"), $"Element maximum-record-size not found in xfdFile: {xfdFilePath}");
            result.NumberOfKeys = GetIntValueOrThrowException((int?)identification.Element(XfdNs + "number-of-keys"), $"Element number-of-keys not found in xfdFile: {xfdFilePath}");

            // Fields
            var fields = xfd.Element(XfdNs + "fields");
            if (fields == null) throw new ApplicationException($"Element fields not found in xfdFile: {xfdFilePath}");

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
            var keys = xfd.Element(XfdNs + "keys");
            if (keys == null) throw new ApplicationException($"Element keys not found in xfdFile: {xfdFilePath}");

            foreach (var key in keys.Elements(XfdNs + "key"))
            {
                if (key == null) continue;
                var vKey = new VisionKeyDefinition
                {
                    IsUnique = ((bool?)key.Attribute(XfdNs + "duplicates-allowed")).GetValueOrDefault(false)
                };
                // Key columns
                foreach (var keyColumn in key.Element(XfdNs + "key-columns")!.Elements(XfdNs + "key-column"))
                {
                    var fieldName = (string?)keyColumn.Attribute(XfdNs + "key-column-name");
                    if (string.IsNullOrEmpty(fieldName))
                    {
                        vKey.Fields.Add(result.Fields.Single(x => x.Name == fieldName));
                    }
                }
                // Key segments
                foreach (var segment in key.Elements(XfdNs + "segments").Elements(XfdNs + "segment"))
                {
                    var strSize = (string?)segment.Attribute(XfdNs + "segment-size");
                    var strOffset = (string?)segment.Attribute(XfdNs + "segment-offset");
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
        /// Checks that the given string is valid. If not, throws an exception with the given message
        /// </summary>
        /// <param name="valueToCheck">Value to check</param>
        /// <param name="exceptionMessage">Message of the exception, if thrown</param>
        /// <returns>String value</returns>
        /// <exception cref="ApplicationException">Exception thrown if the string is not valid</exception>
        private string GetStringValueOrThrowException(string? valueToCheck, string exceptionMessage)
        {
            if (string.IsNullOrEmpty(valueToCheck))
            {
                throw new ApplicationException(exceptionMessage);
            }
            return valueToCheck!;
        }


        /// <summary>
        /// Checks that the given integer is not null. If not, throws an exception with the given message
        /// </summary>
        /// <param name="valueToCheck">Value to check</param>
        /// <param name="exceptionMessage">Message of the exception, if thrown</param>
        /// <returns>String value</returns>
        /// <exception cref="ApplicationException">Exception thrown if the string is not valid</exception>
        private int GetIntValueOrThrowException(int? valueToCheck, string exceptionMessage)
        {
            if (!valueToCheck.HasValue)
            {
                throw new ApplicationException(exceptionMessage);
            }
            return valueToCheck.Value;
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
                occurs.Count = GetIntValueOrThrowException((int?)occursElement.Attribute(XfdNs + "occurs-count"), "occurs-count attribute not found");
                occurs.Size = GetIntValueOrThrowException((int?)occursElement.Attribute(XfdNs + "occurs-size"), "occurs-size attribute not found");
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
                Name = GetStringValueOrThrowException((string?)field.Attribute(XfdNs + "field-name"), "field-name attribute not found"),
                Size = GetIntValueOrThrowException((int?)field.Attribute(XfdNs + "field-length"), "field-length attribute not found"),
                Scale = GetIntValueOrThrowException((int?)field.Attribute(XfdNs + "field-scale") * -1, "field-scale attribute not found"),
                Offset = GetIntValueOrThrowException((int?)field.Attribute(XfdNs + "field-offset"), "field-offset attribute not found"),
                Bytes = GetIntValueOrThrowException((int?)field.Attribute(XfdNs + "field-bytes"), "field-bytes attribute not found"),
                Level = GetIntValueOrThrowException((int?)field.Attribute(XfdNs + "field-level"), "field-level attribute not found")
            };

            // Group field
            result.IsGroupField = (int?)field.Attribute(XfdNs + "field-condition") == 999;

            // Data type
            var fieldType = (int?)field.Attribute(XfdNs + "field-type");
            var userFlag = (int?)field.Attribute(XfdNs + "field-user-flags");
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
