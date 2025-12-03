using System.Globalization;
using System.Text;
using Vision4GP.Core.FileSystem;

namespace Vision4GP.Microfocus
{

    /// <summary>
    /// Data converter for Microfocus
    /// </summary>
    internal class MicrofocusDataConverter : IDataConverter
    {


        /// <summary>
        /// Encoding vision
        /// </summary>
        private Encoding VisionEncoding { get; } = Encoding.ASCII;




        #region Extract/Set specific values


        /// <summary>
        /// Gets the value of a field as string
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record content</param>
        /// <returns>String value of the string</returns>
        private string GetStringValue(VisionFieldDefinition field, Span<byte> record)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

            return VisionEncoding.GetString(record.Slice(field.Offset, field.Size)).TrimEnd();
        }


        /// <summary>
        /// Gets the value of a field as int
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record content</param>
        /// <returns>Int value of the string</returns>
        private int GetIntValue(VisionFieldDefinition field, Span<byte> record)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

            int result = 0;
            short multiplier = 1;
            var zeroByte = (byte)'0';
            var nineByte = (byte)'9';
            var minusByte = (byte)'-';


            foreach (var byteValue in record.Slice(field.Offset, field.Bytes))
            {
                if (byteValue < zeroByte || byteValue > nineByte)
                {
                    continue;
                }
                if (byteValue == minusByte)
                {
                    multiplier = -1;
                    continue;
                }
                result = result * 10 + (byteValue - zeroByte);
            }
            return result * multiplier;
        }


        /// <summary>
        /// Gets the value of a field as long
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record content</param>
        /// <returns>Long value of the string</returns>
        private long GetLongValue(VisionFieldDefinition field, Span<byte> record)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

            long result = 0;
            var multiplier = 1;
            var divider = field.Scale == 0 ? 1 : field.Scale;
            var zeroByte = (byte)'0';
            var nineByte = (byte)'9';
            var minusByte = (byte)'-';


            foreach (var byteValue in record.Slice(field.Offset, field.Bytes))
            {
                if (byteValue < zeroByte || byteValue > nineByte)
                {
                    continue;
                }
                if (byteValue == minusByte)
                {
                    multiplier = -1;
                    continue;
                }
                result = result * 10 + (byteValue - zeroByte);
            }
            return result * multiplier / divider;
        }


        /// <summary>
        /// Gets the value of a field as decimal
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record content</param>
        /// <returns>Decimal value of the string</returns>
        private decimal GetDecimalValue(VisionFieldDefinition field, Span<byte> record)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

            long result = 0;
            var multiplier = 1;
            var divider = field.Scale == 0 ? 1 : field.Scale;
            var zeroByte = (byte)'0';
            var nineByte = (byte)'9';
            var minusByte = (byte)'-';


            foreach (var byteValue in record.Slice(field.Offset, field.Bytes))
            {
                if (byteValue < zeroByte || byteValue > nineByte)
                {
                    continue;
                }
                if (byteValue == minusByte)
                {
                    multiplier = -1;
                    continue;
                }
                result = result * 10 + (byteValue - zeroByte);
            }
            return result * multiplier / divider;
        }


        /// <summary>
        /// Gets the value of a field as date
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record content</param>
        /// <returns>Date value of the string</returns>
        private DateTime? GetDateValue(VisionFieldDefinition field, Span<byte> record)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

            var strValue = GetStringValue(field, record);

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
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        private void SetStringValue(VisionFieldDefinition field, Span<byte> record, string? value)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

            var strToSave = string.IsNullOrEmpty(value) ? string.Empty : value;

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
            VisionEncoding.GetBytes(strToSave).CopyTo(record.Slice(field.Offset, field.Bytes));
        }



        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        private void SetIntValue(VisionFieldDefinition field, Span<byte> record, int value)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));


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

            SetValue(field, record, strToSave);
        }



        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        private void SetLongValue(VisionFieldDefinition field, Span<byte> record, long value)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

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


            SetValue(field, record, strToSave);
        }



        /// <summary>
        /// Sets the value of a field
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        private void SetDecimalValue(VisionFieldDefinition field, Span<byte> record, decimal value)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

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

            SetValue(field, record, strToSave);
        }



        /// <summary>
        /// /// Sets the value of a field
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record where to put data</param>
        /// <param name="value">Value to store</param>
        private void SetDateValue(VisionFieldDefinition field, Span<byte> record, DateTime? value)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

            // Null
            if (!value.HasValue)
            {
                SetValue(field, record, new string('0', field.Bytes));
                return;
            }

            // Size
            var size = field.Bytes;
            var format = GetVisionDateFormat(size);

            // Sets value
            var strToSave = value.Value.ToString(format);
            SetValue(field, record, strToSave);
        }





        #endregion



        private byte[]? _emptyRecord = null;

        /// <summary>
        /// Gets an empty record content
        /// </summary>
        /// <param name="fileDefinition">File definition</param>
        /// <returns>Content of an empty record</returns>
        public byte[] GetEmptyRecordContent(VisionFileDefinition fileDefinition)
        {
            if (_emptyRecord == null)
            {
                _emptyRecord = new byte[fileDefinition.MaxRecordSize];
                var position = 0;

                var zeroByte = Convert.ToByte('0');
                var spaceByte = Convert.ToByte(' ');
                var plusByte = Convert.ToByte('+');

                foreach (var field in fileDefinition.Fields
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
                        position++;
                    }
                }
            }
            return _emptyRecord;
        }



        /// <summary>
        /// Gets a field value from the record
        /// </summary>
        /// <param name="field">Field you want the data</param>
        /// <param name="record">Record from where extract data</param>
        /// <returns>Requested data</returns>
        public T? GetValue<T>(VisionFieldDefinition field, Span<byte> record)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

            if (typeof(T) == typeof(int?) ||
                typeof(T) == typeof(int))
            {
                return GetIntValue(field, record) is T value ? value : default;
            }

            if (typeof(T) == typeof(long?) ||
                typeof(T) == typeof(long))
            {
                return GetLongValue(field, record) is T value ? value : default;
            }

            if (typeof(T) == typeof(Decimal?) ||
                typeof(T) == typeof(decimal))
            {
                return GetDecimalValue(field, record) is T value ? value : default;
            }

            if (typeof(T) == typeof(string))
            {
                return GetStringValue(field, record) is T value ? value : default;
            }

            if (typeof(T) == typeof(DateTime?) ||
                typeof(T) == typeof(DateTime) ||
                typeof(T) == typeof(DateOnly?) ||
                typeof(T) == typeof(DateOnly))
            {
                return GetDateValue(field, record) is T value ? value : default;
            }

            throw new NotSupportedException($"The type '{typeof(T).FullName}' is not supported. Field name: {field.Name}");
        }




        /// <summary>
        /// Sets the value of the specified property to the provided value.
        /// </summary>
        /// <typeparam name="T">The type of the property value to set.</typeparam>
        /// <param name="field">The field/property you want to set the value</param>
        /// <param name="value">The value to assign to the property. May be null for reference types or nullable value types.</param>
        public void SetValue<T>(VisionFieldDefinition field, Span<byte> record, T? value)
        {
            ArgumentNullException.ThrowIfNull(field);
            if (record.IsEmpty) throw new ArgumentNullException(nameof(record));

            if (typeof(T) == typeof(int?) ||
                typeof(T) == typeof(int))
            {
                var intValue = value is int intVal ? intVal : (int?)null;
                SetIntValue(field, record, intValue.GetValueOrDefault());
            }

            if (typeof(T) == typeof(long?) ||
                typeof(T) == typeof(long))
            {
                var longValue = value is long longVal ? longVal : (long?)null;
                SetLongValue(field, record, longValue.GetValueOrDefault());
            }

            if (typeof(T) == typeof(Decimal?) ||
                typeof(T) == typeof(decimal))
            {
                var decimalValue = value is decimal decVal ? decVal : (decimal?)null;
                SetDecimalValue(field, record, decimalValue.GetValueOrDefault());
            }

            if (typeof(T) == typeof(string))
            {
                var stringValue = value as string;
                SetStringValue(field, record, stringValue ?? string.Empty);
            }

            if (typeof(T) == typeof(DateTime?) ||
                typeof(T) == typeof(DateTime) ||
                typeof(T) == typeof(DateOnly?) ||
                typeof(T) == typeof(DateOnly))
            {
                var dateValue = value is DateTime dtVal ? dtVal : (DateTime?)null;
                SetDateValue(field, record, dateValue.GetValueOrDefault());
            }

            throw new NotSupportedException($"The type '{typeof(T).FullName}' is not supported. Field name: {field.Name}");
        }
    }


}