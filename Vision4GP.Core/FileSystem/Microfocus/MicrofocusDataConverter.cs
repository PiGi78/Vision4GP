using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Vision4GP.Core.FileSystem;

namespace Vision4GP.Core.Microfocus
{

    /// <summary>
    /// Data converter for Microfocus
    /// </summary>
    internal class MicrofocusDataConverter
    {

        /// <summary>
        /// Creates a new instance of Microfocus data converter
        /// </summary>
        /// <param name="fileDefinition">File definition for conversions</param>
        public MicrofocusDataConverter(VisionFileDefinition fileDefinition)
        {
            FileDefinition = fileDefinition ?? throw new ArgumentNullException(nameof(fileDefinition));
        }


        /// <summary>
        /// Vision file definition for conversions
        /// </summary>
        private VisionFileDefinition FileDefinition { get; }


        /// <summary>
        /// Encoding vision
        /// </summary>
        private Encoding VisionEncoding { get; } = Encoding.ASCII;


        /// <summary>
        /// Extract the field of the file with the given name
        /// </summary>
        /// <param name="fieldName">Name of the file</param>
        /// <returns>Requested field, throw ApplicationException if not found</returns>
        private VisionFieldDefinition GetField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (_fieldsCache.ContainsKey(fieldName)) return _fieldsCache[fieldName];
            var nameToCheck = fieldName.ToUpper().Replace("-", "_");
            foreach (var field in FileDefinition.Fields)
            {
                if (field.Name.ToUpper().Replace("-", "_") == nameToCheck)
                {
                    _fieldsCache[fieldName] = field;
                    return field;
                }
            }

            throw new ApplicationException($"Cannot find field {fieldName} in file {FileDefinition.FileName}");
        }


        private Dictionary<string, VisionFieldDefinition> _fieldsCache = new Dictionary<string, VisionFieldDefinition>();


        /// <summary>
        /// Gets the value of a field as string
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record content</param>
        /// <returns>String value of the string</returns>
        public string GetStringValue(string fieldName, byte[] record)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var field = GetField(fieldName);

            var fiedlValue = new byte[field.Size];
            Array.Copy(record, field.Offset, fiedlValue, 0, field.Size);

            return VisionEncoding.GetString(fiedlValue).TrimEnd();
        }


        /// <summary>
        /// Gets the value of a field as int
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record content</param>
        /// <returns>Int value of the string</returns>
        public int GetIntValue(string fieldName, byte[] record)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            var longValue = GetLongValue(fieldName, record);
            return Convert.ToInt32(longValue);
        }


        /// <summary>
        /// Gets the value of a field as long
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record content</param>
        /// <returns>Long value of the string</returns>
        public long GetLongValue(string fieldName, byte[] record)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            var strValue = GetStringValue(fieldName, record);
            if (string.IsNullOrEmpty(strValue)) return 0;

            var field = GetField(fieldName);

            var multiplier = 1;
            if (field.IsSigned)
            {
                if (strValue.Contains('-')) multiplier = -1;
                strValue = strValue.Replace("+", "").Replace("-", "");
            }

            long.TryParse(strValue, out long result);
            return result * multiplier;
        }


        /// <summary>
        /// Gets the value of a field as decimal
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record content</param>
        /// <returns>Decimal value of the string</returns>
        public decimal GetDecimalValue(string fieldName, byte[] record)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            var strValue = GetStringValue(fieldName, record);
            if (string.IsNullOrEmpty(strValue)) return 0;

            var field = GetField(fieldName);

            var multiplier = 1;
            if (field.IsSigned)
            {
                if (strValue.Contains('-')) multiplier = -1;
                strValue = strValue.Replace("+", "").Replace("-", "");
            }

            var divider = Convert.ToDecimal(Math.Pow(10, field.Scale));
            if (field.Scale > 0)
            {
                strValue.Replace(".", "").Replace(",", "");
            }

            decimal.TryParse(strValue, out decimal result);
            return result * multiplier / divider;
        }


        /// <summary>
        /// Gets the value of a field as date
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record content</param>
        /// <returns>Date value of the string</returns>
        public DateTime? GetDateValue(string fieldName, byte[] record)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            var strValue = GetStringValue(fieldName, record);

            if (string.IsNullOrEmpty(strValue) ||
                string.IsNullOrEmpty(strValue.Trim('0'))) return null;

            var strSize = strValue.Length;
            if (strSize < 6) return null;

            var format = GetVisionDateFormat(strValue.Length);
            if (!DateTime.TryParseExact(strValue, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime result))
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// Gets vision date format based on the size of the field
        /// </summary>
        /// <param name="size">Field size</param>
        /// <returns>Vision string format</returns>
        private string GetVisionDateFormat(int size)
        {
            return size == 6 ? "yyMMdd" : "yyyyMMddHHmmssffff".Substring(0, size);
        }


        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        public void SetValue(string fieldName, byte[] record, string value)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            var strToSave = string.IsNullOrEmpty(value) ? string.Empty : value;

            var field = GetField(fieldName);

            // Check for max lenght
            var strLen = strToSave.Length;
            if (strLen > field.Bytes) 
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Field {field.Name} cannot be longer than {field.Bytes} bytes");
            }

            // Pads the value
            if (strLen < field.Bytes)
            {
                if (field.FieldType == VisionFieldType.JustifiedString)
                {
                    strToSave = strToSave.PadLeft(field.Bytes);
                }
                else
                {
                    strToSave = strToSave.PadRight(field.Bytes);
                }
            }

            // Saves the data
            var content = VisionEncoding.GetBytes(strToSave);
            Array.Copy(content, 0, record, field.Offset, field.Bytes);
        }




        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        public void SetValue(string fieldName, byte[] record, object value)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            var field = GetField(fieldName);

            // Null value
            if (value == null)
            {
                if (field.FieldType == VisionFieldType.Number)
                {
                    SetValue(fieldName, record, 0);
                }
                else
                {
                    SetValue(fieldName, record, "");
                }
                return;
            }

            // Other values
            switch (field.FieldType)
            {
                case VisionFieldType.Date:
                    var date = Convert.ToDateTime(value);
                    SetValue(fieldName, record, value);
                    break;
                case VisionFieldType.Number:
                    if (field.Scale > 0)
                    {
                        var decimalValue = Convert.ToDecimal(value);
                        SetValue(fieldName, record, decimalValue);
                    }
                    else
                    {
                        var longValue = Convert.ToInt64(value);
                        SetValue(fieldName, record, longValue);
                    }
                    break;
                default:
                    var strValue = value as string;
                    if (string.IsNullOrEmpty(strValue))
                    {
                        strValue = value.ToString();
                    }
                    SetValue(fieldName, record, strValue);
                    break;
            }

        }



        /// <summary>
        /// Get the value of a field
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record where to take data</param>
        /// <returns>Property value</returns>
        public object GetValue(string fieldName, byte[] record)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            var field = GetField(fieldName);

            switch (field.FieldType)
            {
                case VisionFieldType.Date:
                    return GetDateValue(fieldName, record);
                case VisionFieldType.Number:
                    if (field.Scale > 0) return GetDecimalValue(fieldName, record);
                    return GetLongValue(fieldName, record);
                default:
                    return GetStringValue(fieldName, record);
            }

        }



        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        public void SetValue(string fieldName, byte[] record, int value)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            

            var field = GetField(fieldName);

            // Size
            var size = field.Bytes;

            // Sign
            string strToSave;
            if (field.IsSigned)
            {
                size--;
                string strSign = "+";
                var valueToSave = value;
                if (value < 0)
                {
                    strSign = "-";
                    valueToSave *= -1;
                }
                strToSave = valueToSave.ToString(new string('0', size)) + strSign;
            }
            else
            {
                strToSave = value.ToString(new string('0', size));
            }

            SetValue(fieldName, record, strToSave);
        }



        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        public void SetValue(string fieldName, byte[] record, long value)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            

            var field = GetField(fieldName);

            // Size
            var size = field.Bytes;

            // Sign
            string strToSave;
            if (field.IsSigned)
            {
                size--;
                string strSign = "+";
                var valueToSave = value;
                if (value < 0)
                {
                    strSign = "-";
                    valueToSave *= -1;
                }
                strToSave = valueToSave.ToString(new string('0', size)) + strSign;
            }
            else
            {
                strToSave = value.ToString(new string('0', size));
            }


            SetValue(fieldName, record, strToSave);
        }



        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        public void SetValue(string fieldName, byte[] record, decimal value)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            

            var field = GetField(fieldName);

            // Size
            var size = field.Bytes;
            var valueToSave = value;

            // Sign
            string strToSave;
            if (field.IsSigned)
            {
                size--;
                string strSign = "+";
                if (valueToSave < 0)
                {
                    strSign = "-";
                    valueToSave *= -1;
                }
                valueToSave *= (decimal)Math.Pow(10, field.Scale);
                strToSave = valueToSave.ToString(new string('0', size)) + strSign;
            }
            else
            {
                valueToSave *= (decimal)Math.Pow(10, field.Scale);
                strToSave = valueToSave.ToString(new string('0', size));
            }
            
            SetValue(fieldName, record, strToSave);
        }



        /// <summary>
        /// /// Sets the value of a field
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        public void SetValue(string fieldName, byte[] record, DateTime? value)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            

            var field = GetField(fieldName);

            // Null
            if (!value.HasValue)
            {
                SetValue(fieldName, record, new string('0', field.Bytes));
                return;
            }

            // Size
            var size = field.Bytes;
            var format = GetVisionDateFormat(size);

            // Sets value
            var strToSave = value.Value.ToString(format);
            SetValue(fieldName, record, strToSave);
        }



       
        private byte[] _emptyRecord = null;

        /// <summary>
        /// Gets an empty record content
        /// </summary>
        /// <returns>Content of an empty record</returns>
        public byte[] GetEmptyRecordContent()
        {
            if (_emptyRecord == null)
            {
                _emptyRecord = new byte[FileDefinition.MaxRecordSize];
                var position = 0;

                var zeroByte = Convert.ToByte('0');
                var spaceByte = Convert.ToByte(' ');
                var plusByte = Convert.ToByte('+');

                foreach (var field in FileDefinition.Fields
                                                    .Where(x => !x.IsGroupField)
                                                    .OrderBy(x => x.Offset))
                {
                    if (field.Offset < position) continue;
                    // If offset is after the current position, it means there is a filler
                    while (position < field.Offset)
                    {
                        _emptyRecord[position] = spaceByte;
                        position++;
                    }
                    // Field
                    var lastByte = field.Bytes - 1;
                    for (int i = 0; i < field.Bytes; i++)
                    {
                        if (field.FieldType == VisionFieldType.Date ||
                            field.FieldType == VisionFieldType.Number)
                        {
                            _emptyRecord[position] = zeroByte;
                            if (field.IsSigned && i == lastByte)
                            {
                                _emptyRecord[position] = plusByte;
                            }
                        }
                        else
                        {
                            _emptyRecord[position] = spaceByte;
                        }
                        position ++;
                    }
                }
            }
            return _emptyRecord;
        }
    }


}