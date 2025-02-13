﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Data.SqlClient.Server
{

    [Serializable]
    public enum DataAccessKind
    {
        None = 0,
        Read = 1,
    }

    [Serializable]
    public enum SystemDataAccessKind
    {
        None = 0,
        Read = 1,
    }

    // sql specific attribute
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false), Serializable]
    public class SqlFunctionAttribute : System.Attribute
    {
        private bool m_fDeterministic;
        private DataAccessKind m_eDataAccess;
        private SystemDataAccessKind m_eSystemDataAccess;
        private bool m_fPrecise;
        private string m_fName;
        private string m_fTableDefinition;
        private string m_FillRowMethodName;


        public SqlFunctionAttribute()
        {
            // default values
            m_fDeterministic = false;
            m_eDataAccess = DataAccessKind.None;
            m_eSystemDataAccess = SystemDataAccessKind.None;
            m_fPrecise = false;
            m_fName = null;
            m_fTableDefinition = null;
            m_FillRowMethodName = null;
        } // SqlFunctionAttribute

        public bool IsDeterministic
        {
            get
            {
                return m_fDeterministic;
            }
            set
            {
                m_fDeterministic = value;
            }
        } // Deterministic

        public DataAccessKind DataAccess
        {
            get
            {
                return m_eDataAccess;
            }
            set
            {
                m_eDataAccess = value;
            }
        } // public bool DataAccessKind

        public SystemDataAccessKind SystemDataAccess
        {
            get
            {
                return m_eSystemDataAccess;
            }
            set
            {
                m_eSystemDataAccess = value;
            }
        } // public bool SystemDataAccessKind

        public bool IsPrecise
        {
            get
            {
                return m_fPrecise;
            }
            set
            {
                m_fPrecise = value;
            }
        } // Precise

        public string Name
        {
            get
            {
                return m_fName;
            }
            set
            {
                m_fName = value;
            }
        }

        public string TableDefinition
        {
            get
            {
                return m_fTableDefinition;
            }
            set
            {
                m_fTableDefinition = value;
            }
        }
        public string FillRowMethodName
        {
            get
            {
                return m_FillRowMethodName;
            }
            set
            {
                m_FillRowMethodName = value;
            }
        }

    } // class SqlFunctionAttribute
}
