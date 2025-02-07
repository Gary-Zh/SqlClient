// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Common;

namespace Microsoft.Data.SqlClient
{
    public sealed partial class SqlParameter : DbParameter
    { // V1.2.3300
        private object _value;

        private object _parent;

        private ParameterDirection _direction;
        private int _size;
#if USEOFFSET   // USEOFFSET is set in makefile.inc for SqlParameter class only
        private int _offset;
#endif
        private string _sourceColumn;
        private DataRowVersion _sourceVersion;
        private bool _sourceColumnNullMapping;

        private bool _isNullable;

        private object _coercedValue;

        private SqlParameter(SqlParameter source) : this()
        { // V1.2.3300, Clone
            ADP.CheckArgumentNull(source, "source");

            source.CloneHelper(this);

            ICloneable cloneable = (_value as ICloneable);
            if (null != cloneable)
            { // MDAC 49322
                _value = cloneable.Clone();
            }
        }

        private object CoercedValue
        { // V1.2.3300
            get
            {
                return _coercedValue;
            }
            set
            {
                _coercedValue = value;
            }
        }

        [
        RefreshProperties(RefreshProperties.All),
        ResCategoryAttribute(StringsHelper.ResourceNames.DataCategory_Data),
        ResDescriptionAttribute(StringsHelper.ResourceNames.DbParameter_Direction),
        ]
        override public ParameterDirection Direction
        { // V1.2.3300, XXXParameter V1.0.3300
            get
            {
                ParameterDirection direction = _direction;
                return ((0 != direction) ? direction : ParameterDirection.Input);
            }
            set
            {
                if (_direction != value)
                {
                    switch (value)
                    { // @perfnote: Enum.IsDefined
                        case ParameterDirection.Input:
                        case ParameterDirection.Output:
                        case ParameterDirection.InputOutput:
                        case ParameterDirection.ReturnValue:
                            PropertyChanging();
                            _direction = value;
                            break;
                        default:
                            throw ADP.InvalidParameterDirection(value);
                    }
                }
            }
        }

        override public bool IsNullable
        { // V1.2.3300, XXXParameter V1.0.3300
            get
            {
                return _isNullable;
            }
            set
            {
                _isNullable = value;
            }
        }

#if USEOFFSET
        [
        Browsable(false),
        EditorBrowsableAttribute(EditorBrowsableState.Advanced), // MDAC 69508
        ResCategoryAttribute(StringsHelper.ResourceNames.DataCategory_Data),
        ResDescriptionAttribute(StringsHelper.ResourceNames.DbParameter_Offset),
        ]
        public int Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                if (value < 0)
                {
                    throw ADP.InvalidOffsetValue(value);
                }
                _offset = value;
            }
        }
#else
        internal int Offset {
            get {
                return 0;
            }
        }
#endif

        [
        ResCategoryAttribute(StringsHelper.ResourceNames.DataCategory_Data),
        ResDescriptionAttribute(StringsHelper.ResourceNames.DbParameter_Size),
        ]
        override public int Size
        { // V1.2.3300, XXXParameter V1.0.3300
            get
            {
                int size = _size;
                if (0 == size)
                {
                    size = ValueSize(Value);
                }
                return size;
            }
            set
            {
                if (_size != value)
                {
                    if (value < -1)
                    {
                        throw ADP.InvalidSizeValue(value);
                    }
                    PropertyChanging();
                    _size = value;
                }
            }
        }

        private void ResetSize()
        {
            if (0 != _size)
            {
                PropertyChanging();
                _size = 0;
            }
        }

        private bool ShouldSerializeSize()
        { // V1.2.3300
            return (0 != _size);
        }

        [
        ResCategoryAttribute(StringsHelper.ResourceNames.DataCategory_Update),
        ResDescriptionAttribute(StringsHelper.ResourceNames.DbParameter_SourceColumn),
        ]
        override public string SourceColumn
        { // V1.2.3300, XXXParameter V1.0.3300
            get
            {
                string sourceColumn = _sourceColumn;
                return ((null != sourceColumn) ? sourceColumn : ADP.StrEmpty);
            }
            set
            {
                _sourceColumn = value;
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                return _sourceColumnNullMapping;
            }
            set
            {
                _sourceColumnNullMapping = value;
            }
        }

        [
        ResCategoryAttribute(StringsHelper.ResourceNames.DataCategory_Update),
        ResDescriptionAttribute(StringsHelper.ResourceNames.DbParameter_SourceVersion),
        ]
        override public DataRowVersion SourceVersion
        { // V1.2.3300, XXXParameter V1.0.3300
            get
            {
                DataRowVersion sourceVersion = _sourceVersion;
                return ((0 != sourceVersion) ? sourceVersion : DataRowVersion.Current);
            }
            set
            {
                switch (value)
                { // @perfnote: Enum.IsDefined
                    case DataRowVersion.Original:
                    case DataRowVersion.Current:
                    case DataRowVersion.Proposed:
                    case DataRowVersion.Default:
                        _sourceVersion = value;
                        break;
                    default:
                        throw ADP.InvalidDataRowVersion(value);
                }
            }
        }

        private void CloneHelperCore(SqlParameter destination)
        {
            destination._value = _value;
            // NOTE: _parent is not cloned
            destination._direction = _direction;
            destination._size = _size;
#if USEOFFSET
            destination._offset = _offset;
#endif
            destination._sourceColumn = _sourceColumn;
            destination._sourceVersion = _sourceVersion;
            destination._sourceColumnNullMapping = _sourceColumnNullMapping;
            destination._isNullable = _isNullable;
        }

        internal void CopyTo(DbParameter destination)
        {
            ADP.CheckArgumentNull(destination, "destination");
            CloneHelper((SqlParameter)destination);
        }

        internal object CompareExchangeParent(object value, object comparand)
        {
            // the interlock guarantees same parameter won't belong to multiple collections
            // at the same time, but to actually occur the user must really try
            // since we never declared thread safety, we don't care at this time
            //return System.Threading.Interlocked.CompareExchange(ref _parent, value, comparand);
            object parent = _parent;
            if (comparand == parent)
            {
                _parent = value;
            }
            return parent;
        }

        internal void ResetParent()
        {
            _parent = null;
        }

        override public string ToString()
        { // V1.2.3300, XXXParameter V1.0.3300
            return ParameterName;
        }

        private byte ValuePrecisionCore(object value)
        { // V1.2.3300
            if (value is Decimal)
            {
                return ((System.Data.SqlTypes.SqlDecimal)(Decimal)value).Precision; // WebData 102913
            }
            return 0;
        }

        private byte ValueScaleCore(object value)
        { // V1.2.3300
            if (value is Decimal)
            {
                return (byte)((Decimal.GetBits((Decimal)value)[3] & 0x00ff0000) >> 0x10);
            }
            return 0;
        }

        private int ValueSizeCore(object value)
        { // V1.2.3300
            if (!ADP.IsNull(value))
            {
                string svalue = (value as string);
                if (null != svalue)
                {
                    return svalue.Length;
                }
                byte[] bvalue = (value as byte[]);
                if (null != bvalue)
                {
                    return bvalue.Length;
                }
                char[] cvalue = (value as char[]);
                if (null != cvalue)
                {
                    return cvalue.Length;
                }
                if ((value is byte) || (value is char))
                {
                    return 1;
                }
            }
            return 0;
        }
    }
}

