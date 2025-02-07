// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Diagnostics;
using Microsoft.Data.Common;

namespace Microsoft.Data.SqlClient
{
    public sealed class SqlBulkCopyColumnMappingCollection : CollectionBase
    {

        private enum MappingSchema
        {
            Undefined = 0,
            NamesNames = 1,
            NemesOrdinals = 2,
            OrdinalsNames = 3,
            OrdinalsOrdinals = 4,
        }

        private bool _readOnly;
        private MappingSchema _mappingSchema = MappingSchema.Undefined;

        internal SqlBulkCopyColumnMappingCollection()
        {
        }

        public SqlBulkCopyColumnMapping this[int index]
        {
            get
            {
                return (SqlBulkCopyColumnMapping)this.List[index];
            }
        }

        internal bool ReadOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                _readOnly = value;
            }
        }


        public SqlBulkCopyColumnMapping Add(SqlBulkCopyColumnMapping bulkCopyColumnMapping)
        {
            AssertWriteAccess();
            Debug.Assert(ADP.IsEmpty(bulkCopyColumnMapping.SourceColumn) || bulkCopyColumnMapping._internalSourceColumnOrdinal == -1, "BulkLoadAmbigousSourceColumn");
            if (((ADP.IsEmpty(bulkCopyColumnMapping.SourceColumn)) && (bulkCopyColumnMapping.SourceOrdinal == -1))
                || ((ADP.IsEmpty(bulkCopyColumnMapping.DestinationColumn)) && (bulkCopyColumnMapping.DestinationOrdinal == -1)))
            {
                throw SQL.BulkLoadNonMatchingColumnMapping();
            }
            InnerList.Add(bulkCopyColumnMapping);
            return bulkCopyColumnMapping;
        }

        public SqlBulkCopyColumnMapping Add(string sourceColumn, string destinationColumn)
        {
            AssertWriteAccess();
            SqlBulkCopyColumnMapping column = new SqlBulkCopyColumnMapping(sourceColumn, destinationColumn);
            return Add(column);
        }

        public SqlBulkCopyColumnMapping Add(int sourceColumnIndex, string destinationColumn)
        {
            AssertWriteAccess();
            SqlBulkCopyColumnMapping column = new SqlBulkCopyColumnMapping(sourceColumnIndex, destinationColumn);
            return Add(column);
        }

        public SqlBulkCopyColumnMapping Add(string sourceColumn, int destinationColumnIndex)
        {
            AssertWriteAccess();
            SqlBulkCopyColumnMapping column = new SqlBulkCopyColumnMapping(sourceColumn, destinationColumnIndex);
            return Add(column);
        }
        public SqlBulkCopyColumnMapping Add(int sourceColumnIndex, int destinationColumnIndex)
        {
            AssertWriteAccess();
            SqlBulkCopyColumnMapping column = new SqlBulkCopyColumnMapping(sourceColumnIndex, destinationColumnIndex);
            return Add(column);
        }

        private void AssertWriteAccess()
        {
            if (ReadOnly)
            {
                throw SQL.BulkLoadMappingInaccessible();
            }
        }

        new public void Clear()
        {
            AssertWriteAccess();
            base.Clear();
        }

        public bool Contains(SqlBulkCopyColumnMapping value)
        {
            return (-1 != InnerList.IndexOf(value));
        }

        public void CopyTo(SqlBulkCopyColumnMapping[] array, int index)
        {
            InnerList.CopyTo(array, index);
        }

        internal void CreateDefaultMapping(int columnCount)
        {
            for (int i = 0; i < columnCount; i++)
            {
                InnerList.Add(new SqlBulkCopyColumnMapping(i, i));
            }
        }

        public int IndexOf(SqlBulkCopyColumnMapping value)
        {
            return InnerList.IndexOf(value);
        }

        public void Insert(int index, SqlBulkCopyColumnMapping value)
        {
            AssertWriteAccess();
            InnerList.Insert(index, value);
        }

        public void Remove(SqlBulkCopyColumnMapping value)
        {
            AssertWriteAccess();
            InnerList.Remove(value);
        }

        new public void RemoveAt(int index)
        {
            AssertWriteAccess();
            base.RemoveAt(index);
        }

        internal void ValidateCollection()
        {
            MappingSchema mappingSchema;
            foreach (SqlBulkCopyColumnMapping a in this)
            {
                if (a.SourceOrdinal != -1)
                {
                    if (a.DestinationOrdinal != -1)
                    {
                        mappingSchema = MappingSchema.OrdinalsOrdinals;
                    }
                    else
                    {
                        mappingSchema = MappingSchema.OrdinalsNames;
                    }
                }
                else
                {
                    if (a.DestinationOrdinal != -1)
                    {
                        mappingSchema = MappingSchema.NemesOrdinals;
                    }
                    else
                    {
                        mappingSchema = MappingSchema.NamesNames;
                    }
                }

                if (_mappingSchema == MappingSchema.Undefined)
                {
                    _mappingSchema = mappingSchema;
                }
                else
                {
                    if (_mappingSchema != mappingSchema)
                    {
                        throw SQL.BulkLoadMappingsNamesOrOrdinalsOnly();
                    }
                }
            }
        }
    }
}

