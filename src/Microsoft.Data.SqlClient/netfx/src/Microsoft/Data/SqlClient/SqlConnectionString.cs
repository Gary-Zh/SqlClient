// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Data.Common;

namespace Microsoft.Data.SqlClient
{
    internal sealed class SqlConnectionString : DbConnectionOptions
    {
        // instances of this class are intended to be immutable, i.e readonly
        // used by pooling classes so it is much easier to verify correctness
        // when not worried about the class being modified during execution

        internal static class DEFAULT
        {
            internal const ApplicationIntent ApplicationIntent = DbConnectionStringDefaults.ApplicationIntent;
            internal const string Application_Name = TdsEnums.SQL_PROVIDER_NAME;
            internal const bool Asynchronous = false;
            internal const string AttachDBFilename = "";
            internal const PoolBlockingPeriod PoolBlockingPeriod = DbConnectionStringDefaults.PoolBlockingPeriod;
            internal const int Connect_Timeout = ADP.DefaultConnectionTimeout;
            internal const bool Connection_Reset = true;
            internal const bool Context_Connection = false;
            internal const string Current_Language = "";
            internal const string Data_Source = "";
            internal const bool Encrypt = false;
            internal const bool Enlist = true;
            internal const string FailoverPartner = "";
            internal const string Initial_Catalog = "";
            internal const bool Integrated_Security = false;
            internal const int Load_Balance_Timeout = 0; // default of 0 means don't use
            internal const bool MARS = false;
            internal const int Max_Pool_Size = 100;
            internal const int Min_Pool_Size = 0;
            internal const bool MultiSubnetFailover = DbConnectionStringDefaults.MultiSubnetFailover;
            internal static readonly bool TransparentNetworkIPResolution = DbConnectionStringDefaults.TransparentNetworkIPResolution;
            internal const string Network_Library = "";
            internal const int Packet_Size = 8000;
            internal const string Password = "";
            internal const bool Persist_Security_Info = false;
            internal const bool Pooling = true;
            internal const bool TrustServerCertificate = false;
            internal const string Type_System_Version = "";
            internal const string User_ID = "";
            internal const bool User_Instance = false;
            internal const bool Replication = false;
            internal const int Connect_Retry_Count = 1;
            internal const int Connect_Retry_Interval = 10;
            internal static readonly SqlAuthenticationMethod Authentication = SqlAuthenticationMethod.NotSpecified;
            internal static readonly SqlConnectionColumnEncryptionSetting ColumnEncryptionSetting = SqlConnectionColumnEncryptionSetting.Disabled;
            internal const string EnclaveAttestationUrl = "";
#if ADONET_CERT_AUTH
            internal const  string Certificate            = "";
#endif
        }

        // SqlConnection ConnectionString Options
        // keys must be lowercase!
        internal static class KEY
        {
            internal const string ApplicationIntent = "applicationintent";
            internal const string Application_Name = "application name";
            internal const string AsynchronousProcessing = "asynchronous processing";
            internal const string AttachDBFilename = "attachdbfilename";
            internal const string PoolBlockingPeriod = "poolblockingperiod";
            internal const string ColumnEncryptionSetting = "column encryption setting";
            internal const string EnclaveAttestationUrl = "enclave attestation url";
            internal const string Connect_Timeout = "connect timeout";
            internal const string Connection_Reset = "connection reset";
            internal const string Context_Connection = "context connection";
            internal const string Current_Language = "current language";
            internal const string Data_Source = "data source";
            internal const string Encrypt = "encrypt";
            internal const string Enlist = "enlist";
            internal const string FailoverPartner = "failover partner";
            internal const string Initial_Catalog = "initial catalog";
            internal const string Integrated_Security = "integrated security";
            internal const string Load_Balance_Timeout = "load balance timeout";
            internal const string MARS = "multipleactiveresultsets";
            internal const string Max_Pool_Size = "max pool size";
            internal const string Min_Pool_Size = "min pool size";
            internal const string MultiSubnetFailover = "multisubnetfailover";
            internal const string TransparentNetworkIPResolution = "transparentnetworkipresolution";
            internal const string Network_Library = "network library";
            internal const string Packet_Size = "packet size";
            internal const string Password = "password";
            internal const string Persist_Security_Info = "persist security info";
            internal const string Pooling = "pooling";
            internal const string TransactionBinding = "transaction binding";
            internal const string TrustServerCertificate = "trustservercertificate";
            internal const string Type_System_Version = "type system version";
            internal const string User_ID = "user id";
            internal const string User_Instance = "user instance";
            internal const string Workstation_Id = "workstation id";
            internal const string Replication = "replication";
            internal const string Connect_Retry_Count = "connectretrycount";
            internal const string Connect_Retry_Interval = "connectretryinterval";
            internal const string Authentication = "authentication";
#if ADONET_CERT_AUTH            
            internal const string Certificate						= "certificate";
#endif
        }

        // Constant for the number of duplicate options in the connnection string

        private static class SYNONYM
        {
            // application name
            internal const string APP = "app";
            internal const string Async = "async";
            // attachDBFilename
            internal const string EXTENDED_PROPERTIES = "extended properties";
            internal const string INITIAL_FILE_NAME = "initial file name";
            // connect timeout
            internal const string CONNECTION_TIMEOUT = "connection timeout";
            internal const string TIMEOUT = "timeout";
            // current language
            internal const string LANGUAGE = "language";
            // data source
            internal const string ADDR = "addr";
            internal const string ADDRESS = "address";
            internal const string SERVER = "server";
            internal const string NETWORK_ADDRESS = "network address";
            // initial catalog
            internal const string DATABASE = "database";
            // integrated security
            internal const string TRUSTED_CONNECTION = "trusted_connection";
            // load balance timeout
            internal const string Connection_Lifetime = "connection lifetime";
            // network library
            internal const string NET = "net";
            internal const string NETWORK = "network";
            // password
            internal const string Pwd = "pwd";
            // persist security info
            internal const string PERSISTSECURITYINFO = "persistsecurityinfo";
            // user id
            internal const string UID = "uid";
            internal const string User = "user";
            // workstation id
            internal const string WSID = "wsid";

            // make sure to update SynonymCount value below when adding or removing synonyms
        }

        internal const int SynonymCount = 21;

        // the following are all inserted as keys into the _netlibMapping hash
        internal static class NETLIB
        {
            internal const string AppleTalk = "dbmsadsn";
            internal const string BanyanVines = "dbmsvinn";
            internal const string IPXSPX = "dbmsspxn";
            internal const string Multiprotocol = "dbmsrpcn";
            internal const string NamedPipes = "dbnmpntw";
            internal const string SharedMemory = "dbmslpcn";
            internal const string TCPIP = "dbmssocn";
            internal const string VIA = "dbmsgnet";
        }

        internal enum TypeSystem
        {
            Latest = 2008,
            SQLServer2000 = 2000,
            SQLServer2005 = 2005,
            SQLServer2008 = 2008,
            SQLServer2012 = 2012,
        }

        internal static class TYPESYSTEMVERSION
        {
            internal const string Latest = "Latest";
            internal const string SQL_Server_2000 = "SQL Server 2000";
            internal const string SQL_Server_2005 = "SQL Server 2005";
            internal const string SQL_Server_2008 = "SQL Server 2008";
            internal const string SQL_Server_2012 = "SQL Server 2012";
        }

        internal enum TransactionBindingEnum
        {
            ImplicitUnbind,
            ExplicitUnbind
        }

        internal static class TRANSACIONBINDING
        {
            internal const string ImplicitUnbind = "Implicit Unbind";
            internal const string ExplicitUnbind = "Explicit Unbind";
        }

        static private Hashtable _sqlClientSynonyms;
        static private Hashtable _netlibMapping;

        private readonly bool _integratedSecurity;

        private readonly PoolBlockingPeriod _poolBlockingPeriod;
        private readonly bool _connectionReset;
        private readonly bool _contextConnection;
        private readonly bool _encrypt;
        private readonly bool _trustServerCertificate;
        private readonly bool _enlist;
        private readonly bool _mars;
        private readonly bool _persistSecurityInfo;
        private readonly bool _pooling;
        private readonly bool _replication;
        private readonly bool _userInstance;
        private readonly bool _multiSubnetFailover;
        private readonly bool _transparentNetworkIPResolution;
        private readonly SqlAuthenticationMethod _authType;
        private readonly SqlConnectionColumnEncryptionSetting _columnEncryptionSetting;
        private readonly string _enclaveAttestationUrl;

        private readonly int _connectTimeout;
        private readonly int _loadBalanceTimeout;
        private readonly int _maxPoolSize;
        private readonly int _minPoolSize;
        private readonly int _packetSize;
        private readonly int _connectRetryCount;
        private readonly int _connectRetryInterval;

        private readonly ApplicationIntent _applicationIntent;
        private readonly string _applicationName;
        private readonly string _attachDBFileName;
        private readonly string _currentLanguage;
        private readonly string _dataSource;
        private readonly string _localDBInstance; // created based on datasource, set to NULL if datasource is not LocalDB 
        private readonly string _failoverPartner;
        private readonly string _initialCatalog;
        private readonly string _password;
        private readonly string _userID;
#if ADONET_CERT_AUTH        
        private readonly string _certificate;
#endif
        private readonly string _networkLibrary;
        private readonly string _workstationId;

        private readonly TypeSystem _typeSystemVersion;
        private readonly Version _typeSystemAssemblyVersion;
        private static readonly Version constTypeSystemAsmVersion10 = new Version("10.0.0.0");
        private static readonly Version constTypeSystemAsmVersion11 = new Version("11.0.0.0");

        private readonly TransactionBindingEnum _transactionBinding;

        private readonly string _expandedAttachDBFilename; // expanded during construction so that CreatePermissionSet & Expand are consistent


        // SxS: reading Software\\Microsoft\\MSSQLServer\\Client\\SuperSocketNetLib\Encrypt value from registry
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        internal SqlConnectionString(string connectionString) : base(connectionString, GetParseSynonyms(), false)
        {

            bool runningInProc = InOutOfProcHelper.InProc;

            _integratedSecurity = ConvertValueToIntegratedSecurity();

            ConvertValueToBoolean(KEY.AsynchronousProcessing, DEFAULT.Asynchronous); // while we don't use it anymore, we still need to verify it is true/false

            // SQLPT 41700: Ignore ResetConnection=False (still validate the keyword/value)
            _poolBlockingPeriod = ConvertValueToPoolBlockingPeriod();
            _connectionReset = ConvertValueToBoolean(KEY.Connection_Reset, DEFAULT.Connection_Reset);
            _contextConnection = ConvertValueToBoolean(KEY.Context_Connection, DEFAULT.Context_Connection);
            _encrypt = ConvertValueToEncrypt();
            _enlist = ConvertValueToBoolean(KEY.Enlist, ADP.IsWindowsNT);
            _mars = ConvertValueToBoolean(KEY.MARS, DEFAULT.MARS);
            _persistSecurityInfo = ConvertValueToBoolean(KEY.Persist_Security_Info, DEFAULT.Persist_Security_Info);
            _pooling = ConvertValueToBoolean(KEY.Pooling, DEFAULT.Pooling);
            _replication = ConvertValueToBoolean(KEY.Replication, DEFAULT.Replication);
            _userInstance = ConvertValueToBoolean(KEY.User_Instance, DEFAULT.User_Instance);
            _multiSubnetFailover = ConvertValueToBoolean(KEY.MultiSubnetFailover, DEFAULT.MultiSubnetFailover);
            _transparentNetworkIPResolution = ConvertValueToBoolean(KEY.TransparentNetworkIPResolution, DEFAULT.TransparentNetworkIPResolution);

            _connectTimeout = ConvertValueToInt32(KEY.Connect_Timeout, DEFAULT.Connect_Timeout);
            _loadBalanceTimeout = ConvertValueToInt32(KEY.Load_Balance_Timeout, DEFAULT.Load_Balance_Timeout);
            _maxPoolSize = ConvertValueToInt32(KEY.Max_Pool_Size, DEFAULT.Max_Pool_Size);
            _minPoolSize = ConvertValueToInt32(KEY.Min_Pool_Size, DEFAULT.Min_Pool_Size);
            _packetSize = ConvertValueToInt32(KEY.Packet_Size, DEFAULT.Packet_Size);
            _connectRetryCount = ConvertValueToInt32(KEY.Connect_Retry_Count, DEFAULT.Connect_Retry_Count);
            _connectRetryInterval = ConvertValueToInt32(KEY.Connect_Retry_Interval, DEFAULT.Connect_Retry_Interval);

            _applicationIntent = ConvertValueToApplicationIntent();
            _applicationName = ConvertValueToString(KEY.Application_Name, DEFAULT.Application_Name);
            _attachDBFileName = ConvertValueToString(KEY.AttachDBFilename, DEFAULT.AttachDBFilename);
            _currentLanguage = ConvertValueToString(KEY.Current_Language, DEFAULT.Current_Language);
            _dataSource = ConvertValueToString(KEY.Data_Source, DEFAULT.Data_Source);
            _localDBInstance = LocalDBAPI.GetLocalDbInstanceNameFromServerName(_dataSource);
            _failoverPartner = ConvertValueToString(KEY.FailoverPartner, DEFAULT.FailoverPartner);
            _initialCatalog = ConvertValueToString(KEY.Initial_Catalog, DEFAULT.Initial_Catalog);
            _networkLibrary = ConvertValueToString(KEY.Network_Library, null);
            _password = ConvertValueToString(KEY.Password, DEFAULT.Password);
            _trustServerCertificate = ConvertValueToBoolean(KEY.TrustServerCertificate, DEFAULT.TrustServerCertificate);
            _authType = ConvertValueToAuthenticationType();
            _columnEncryptionSetting = ConvertValueToColumnEncryptionSetting();
            _enclaveAttestationUrl = ConvertValueToString(KEY.EnclaveAttestationUrl, DEFAULT.EnclaveAttestationUrl);

#if ADONET_CERT_AUTH            
            _certificate = ConvertValueToString(KEY.Certificate,         DEFAULT.Certificate);
#endif

            // Temporary string - this value is stored internally as an enum.
            string typeSystemVersionString = ConvertValueToString(KEY.Type_System_Version, null);
            string transactionBindingString = ConvertValueToString(KEY.TransactionBinding, null);

            _userID = ConvertValueToString(KEY.User_ID, DEFAULT.User_ID);
            _workstationId = ConvertValueToString(KEY.Workstation_Id, null);

            if (_contextConnection)
            {
                // We have to be running in the engine for you to request a 
                // context connection.

                if (!runningInProc)
                {
                    throw SQL.ContextUnavailableOutOfProc();
                }

                // When using a context connection, we need to ensure that no 
                // other connection string keywords are specified.

                foreach (DictionaryEntry entry in Parsetable)
                {
                    if ((string)entry.Key != KEY.Context_Connection &&
                        (string)entry.Key != KEY.Type_System_Version)
                    {
                        throw SQL.ContextAllowsLimitedKeywords();
                    }
                }
            }

            if (!_encrypt)
            {    // Support legacy registry encryption settings
                const string folder = "Software\\Microsoft\\MSSQLServer\\Client\\SuperSocketNetLib";
                const string value = "Encrypt";

                Object obj = ADP.LocalMachineRegistryValue(folder, value);
                if ((obj is Int32) && (1 == (int)obj))
                {         // If the registry key exists
                    _encrypt = true;
                }
            }


            if (_loadBalanceTimeout < 0)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.Load_Balance_Timeout);
            }

            if (_connectTimeout < 0)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.Connect_Timeout);
            }

            if (_maxPoolSize < 1)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.Max_Pool_Size);
            }

            if (_minPoolSize < 0)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.Min_Pool_Size);
            }
            if (_maxPoolSize < _minPoolSize)
            {
                throw ADP.InvalidMinMaxPoolSizeValues();
            }

            if ((_packetSize < TdsEnums.MIN_PACKET_SIZE) || (TdsEnums.MAX_PACKET_SIZE < _packetSize))
            {
                throw SQL.InvalidPacketSizeValue();
            }

            if (null != _networkLibrary)
            { // MDAC 83525
                string networkLibrary = _networkLibrary.Trim().ToLower(CultureInfo.InvariantCulture);
                Hashtable netlib = NetlibMapping();
                if (!netlib.ContainsKey(networkLibrary))
                {
                    throw ADP.InvalidConnectionOptionValue(KEY.Network_Library);
                }
                _networkLibrary = (string)netlib[networkLibrary];
            }
            else
            {
                _networkLibrary = DEFAULT.Network_Library;
            }

            ValidateValueLength(_applicationName, TdsEnums.MAXLEN_APPNAME, KEY.Application_Name);
            ValidateValueLength(_currentLanguage, TdsEnums.MAXLEN_LANGUAGE, KEY.Current_Language);
            ValidateValueLength(_dataSource, TdsEnums.MAXLEN_SERVERNAME, KEY.Data_Source);
            ValidateValueLength(_failoverPartner, TdsEnums.MAXLEN_SERVERNAME, KEY.FailoverPartner);
            ValidateValueLength(_initialCatalog, TdsEnums.MAXLEN_DATABASE, KEY.Initial_Catalog);
            ValidateValueLength(_password, TdsEnums.MAXLEN_PASSWORD, KEY.Password);
            ValidateValueLength(_userID, TdsEnums.MAXLEN_USERNAME, KEY.User_ID);
            if (null != _workstationId)
            {
                ValidateValueLength(_workstationId, TdsEnums.MAXLEN_HOSTNAME, KEY.Workstation_Id);
            }

            if (!String.Equals(DEFAULT.FailoverPartner, _failoverPartner, StringComparison.OrdinalIgnoreCase))
            {
                // fail-over partner is set

                if (_multiSubnetFailover)
                {
                    throw SQL.MultiSubnetFailoverWithFailoverPartner(serverProvidedFailoverPartner: false, internalConnection: null);
                }

                if (String.Equals(DEFAULT.Initial_Catalog, _initialCatalog, StringComparison.OrdinalIgnoreCase))
                {
                    throw ADP.MissingConnectionOptionValue(KEY.FailoverPartner, KEY.Initial_Catalog);
                }
            }

            // expand during construction so that CreatePermissionSet and Expand are consistent
            string datadir = null;
            _expandedAttachDBFilename = ExpandDataDirectory(KEY.AttachDBFilename, _attachDBFileName, ref datadir);
            if (null != _expandedAttachDBFilename)
            {
                if (0 <= _expandedAttachDBFilename.IndexOf('|'))
                {
                    throw ADP.InvalidConnectionOptionValue(KEY.AttachDBFilename);
                }
                ValidateValueLength(_expandedAttachDBFilename, TdsEnums.MAXLEN_ATTACHDBFILE, KEY.AttachDBFilename);
                if (_localDBInstance == null)
                {
                    // fail fast to verify LocalHost when using |DataDirectory|
                    // still must check again at connect time
                    string host = _dataSource;
                    string protocol = _networkLibrary;
                    TdsParserStaticMethods.AliasRegistryLookup(ref host, ref protocol);
                    VerifyLocalHostAndFixup(ref host, true, false /*don't fix-up*/);
                }
            }
            else if (0 <= _attachDBFileName.IndexOf('|'))
            {
                throw ADP.InvalidConnectionOptionValue(KEY.AttachDBFilename);
            }
            else
            {
                ValidateValueLength(_attachDBFileName, TdsEnums.MAXLEN_ATTACHDBFILE, KEY.AttachDBFilename);
            }

            _typeSystemAssemblyVersion = constTypeSystemAsmVersion10;

            if (true == _userInstance && !ADP.IsEmpty(_failoverPartner))
            {
                throw SQL.UserInstanceFailoverNotCompatible();
            }

            if (ADP.IsEmpty(typeSystemVersionString))
            {
                typeSystemVersionString = DbConnectionStringDefaults.TypeSystemVersion;
            }

            if (typeSystemVersionString.Equals(TYPESYSTEMVERSION.Latest, StringComparison.OrdinalIgnoreCase))
            {
                _typeSystemVersion = TypeSystem.Latest;
            }
            else if (typeSystemVersionString.Equals(TYPESYSTEMVERSION.SQL_Server_2000, StringComparison.OrdinalIgnoreCase))
            {
                if (_contextConnection)
                {
                    throw SQL.ContextAllowsOnlyTypeSystem2005();
                }
                _typeSystemVersion = TypeSystem.SQLServer2000;
            }
            else if (typeSystemVersionString.Equals(TYPESYSTEMVERSION.SQL_Server_2005, StringComparison.OrdinalIgnoreCase))
            {
                _typeSystemVersion = TypeSystem.SQLServer2005;
            }
            else if (typeSystemVersionString.Equals(TYPESYSTEMVERSION.SQL_Server_2008, StringComparison.OrdinalIgnoreCase))
            {
                _typeSystemVersion = TypeSystem.SQLServer2008;
            }
            else if (typeSystemVersionString.Equals(TYPESYSTEMVERSION.SQL_Server_2012, StringComparison.OrdinalIgnoreCase))
            {
                _typeSystemVersion = TypeSystem.SQLServer2012;
                _typeSystemAssemblyVersion = constTypeSystemAsmVersion11;
            }
            else
            {
                throw ADP.InvalidConnectionOptionValue(KEY.Type_System_Version);
            }

            if (ADP.IsEmpty(transactionBindingString))
            {
                transactionBindingString = DbConnectionStringDefaults.TransactionBinding;
            }

            if (transactionBindingString.Equals(TRANSACIONBINDING.ImplicitUnbind, StringComparison.OrdinalIgnoreCase))
            {
                _transactionBinding = TransactionBindingEnum.ImplicitUnbind;
            }
            else if (transactionBindingString.Equals(TRANSACIONBINDING.ExplicitUnbind, StringComparison.OrdinalIgnoreCase))
            {
                _transactionBinding = TransactionBindingEnum.ExplicitUnbind;
            }
            else
            {
                throw ADP.InvalidConnectionOptionValue(KEY.TransactionBinding);
            }

            if ((_connectRetryCount < 0) || (_connectRetryCount > 255))
            {
                throw ADP.InvalidConnectRetryCountValue();
            }

            if ((_connectRetryInterval < 1) || (_connectRetryInterval > 60))
            {
                throw ADP.InvalidConnectRetryIntervalValue();
            }

            if (Authentication != SqlAuthenticationMethod.NotSpecified && _integratedSecurity == true)
            {
                throw SQL.AuthenticationAndIntegratedSecurity();
            }

            if (Authentication == SqlAuthenticationMethod.ActiveDirectoryIntegrated && (HasUserIdKeyword || HasPasswordKeyword))
            {
                throw SQL.IntegratedWithUserIDAndPassword();
            }

            if (Authentication == SqlAuthenticationMethod.ActiveDirectoryInteractive && (HasUserIdKeyword || HasPasswordKeyword))
            {
                throw SQL.InteractiveWithUserIDAndPassword();
            }

#if ADONET_CERT_AUTH
            
            if (!DbConnectionStringBuilderUtil.IsValidCertificateValue(_certificate)) {
                throw ADP.InvalidConnectionOptionValue(KEY.Certificate);
            }

            if (!string.IsNullOrEmpty(_certificate)) {
                
                if (Authentication == SqlClient.SqlAuthenticationMethod.NotSpecified && !_integratedSecurity) {
                    _authType = SqlClient.SqlAuthenticationMethod.SqlCertificate;
                }

                if (Authentication == SqlClient.SqlAuthenticationMethod.SqlCertificate && (HasUserIdKeyword || HasPasswordKeyword || _integratedSecurity)) { 
                    throw SQL.InvalidCertAuth();
                }
            }
            else if (Authentication == SqlClient.SqlAuthenticationMethod.SqlCertificate) {
                throw ADP.InvalidConnectionOptionValue(KEY.Authentication);
            }
#endif
        }

        // This c-tor is used to create SSE and user instance connection strings when user instance is set to true
        // BUG (VSTFDevDiv) 479687: Using TransactionScope with Linq2SQL against user instances fails with "connection has been broken" message
        internal SqlConnectionString(SqlConnectionString connectionOptions, string dataSource, bool userInstance, bool? setEnlistValue) : base(connectionOptions)
        {
            _integratedSecurity = connectionOptions._integratedSecurity;
            _connectionReset = connectionOptions._connectionReset;
            _contextConnection = connectionOptions._contextConnection;
            _encrypt = connectionOptions._encrypt;

            if (setEnlistValue.HasValue)
            {
                _enlist = setEnlistValue.Value;
            }
            else
            {
                _enlist = connectionOptions._enlist;
            }

            _mars = connectionOptions._mars;
            _persistSecurityInfo = connectionOptions._persistSecurityInfo;
            _pooling = connectionOptions._pooling;
            _replication = connectionOptions._replication;
            _userInstance = userInstance;
            _connectTimeout = connectionOptions._connectTimeout;
            _loadBalanceTimeout = connectionOptions._loadBalanceTimeout;
            _poolBlockingPeriod = connectionOptions._poolBlockingPeriod;
            _maxPoolSize = connectionOptions._maxPoolSize;
            _minPoolSize = connectionOptions._minPoolSize;
            _multiSubnetFailover = connectionOptions._multiSubnetFailover;
            _transparentNetworkIPResolution = connectionOptions._transparentNetworkIPResolution;
            _packetSize = connectionOptions._packetSize;
            _applicationName = connectionOptions._applicationName;
            _attachDBFileName = connectionOptions._attachDBFileName;
            _currentLanguage = connectionOptions._currentLanguage;
            _dataSource = dataSource;
            _localDBInstance = LocalDBAPI.GetLocalDbInstanceNameFromServerName(_dataSource);
            _failoverPartner = connectionOptions._failoverPartner;
            _initialCatalog = connectionOptions._initialCatalog;
            _password = connectionOptions._password;
            _userID = connectionOptions._userID;
            _networkLibrary = connectionOptions._networkLibrary;
            _workstationId = connectionOptions._workstationId;
            _expandedAttachDBFilename = connectionOptions._expandedAttachDBFilename;
            _typeSystemVersion = connectionOptions._typeSystemVersion;
            _typeSystemAssemblyVersion = connectionOptions._typeSystemAssemblyVersion;
            _transactionBinding = connectionOptions._transactionBinding;
            _applicationIntent = connectionOptions._applicationIntent;
            _connectRetryCount = connectionOptions._connectRetryCount;
            _connectRetryInterval = connectionOptions._connectRetryInterval;
            _authType = connectionOptions._authType;
            _columnEncryptionSetting = connectionOptions._columnEncryptionSetting;
            _enclaveAttestationUrl = connectionOptions._enclaveAttestationUrl;
#if ADONET_CERT_AUTH            
            _certificate              = connectionOptions._certificate;
#endif
            ValidateValueLength(_dataSource, TdsEnums.MAXLEN_SERVERNAME, KEY.Data_Source);
        }

        internal bool IntegratedSecurity { get { return _integratedSecurity; } }

        // We always initialize in Async mode so that both synchronous and asynchronous methods
        // will work.  In the future we can deprecate the keyword entirely.
        internal bool Asynchronous { get { return true; } }

        internal PoolBlockingPeriod PoolBlockingPeriod { get { return _poolBlockingPeriod; } }

        // SQLPT 41700: Ignore ResetConnection=False, always reset the connection for security
        internal bool ConnectionReset { get { return true; } }
        internal bool ContextConnection { get { return _contextConnection; } }
        //        internal bool EnableUdtDownload { get { return _enableUdtDownload;} }
        internal bool Encrypt { get { return _encrypt; } }
        internal bool TrustServerCertificate { get { return _trustServerCertificate; } }
        internal bool Enlist { get { return _enlist; } }
        internal bool MARS { get { return _mars; } }
        internal bool MultiSubnetFailover { get { return _multiSubnetFailover; } }
        internal bool TransparentNetworkIPResolution { get { return _transparentNetworkIPResolution; } }
        internal SqlAuthenticationMethod Authentication { get { return _authType; } }
        internal SqlConnectionColumnEncryptionSetting ColumnEncryptionSetting { get { return _columnEncryptionSetting; } }

        internal string EnclaveAttestationUrl { get { return _enclaveAttestationUrl; } }
#if ADONET_CERT_AUTH        
        internal string Certificate { get { return _certificate; } }
        internal bool UsesCertificate { get { return _authType == SqlClient.SqlAuthenticationMethod.SqlCertificate; } }
#else
        internal string Certificate { get { return null; } }
        internal bool UsesCertificate { get { return false; } }
#endif
        internal bool PersistSecurityInfo { get { return _persistSecurityInfo; } }
        internal bool Pooling { get { return _pooling; } }
        internal bool Replication { get { return _replication; } }
        internal bool UserInstance { get { return _userInstance; } }

        internal int ConnectTimeout { get { return _connectTimeout; } }
        internal int LoadBalanceTimeout { get { return _loadBalanceTimeout; } }
        internal int MaxPoolSize { get { return _maxPoolSize; } }
        internal int MinPoolSize { get { return _minPoolSize; } }
        internal int PacketSize { get { return _packetSize; } }
        internal int ConnectRetryCount { get { return _connectRetryCount; } }
        internal int ConnectRetryInterval { get { return _connectRetryInterval; } }

        internal ApplicationIntent ApplicationIntent { get { return _applicationIntent; } }
        internal string ApplicationName { get { return _applicationName; } }
        internal string AttachDBFilename { get { return _attachDBFileName; } }
        internal string CurrentLanguage { get { return _currentLanguage; } }
        internal string DataSource { get { return _dataSource; } }
        internal string LocalDBInstance { get { return _localDBInstance; } }
        internal string FailoverPartner { get { return _failoverPartner; } }
        internal string InitialCatalog { get { return _initialCatalog; } }
        internal string NetworkLibrary { get { return _networkLibrary; } }
        internal string Password { get { return _password; } }
        internal string UserID { get { return _userID; } }
        internal string WorkstationId { get { return _workstationId; } }

        internal TypeSystem TypeSystemVersion { get { return _typeSystemVersion; } }
        internal Version TypeSystemAssemblyVersion { get { return _typeSystemAssemblyVersion; } }
        internal TransactionBindingEnum TransactionBinding { get { return _transactionBinding; } }

        internal bool EnforceLocalHost
        {
            get
            {
                // so tdsparser.connect can determine if SqlConnection.UserConnectionOptions
                // needs to enfoce local host after datasource alias lookup
                return (null != _expandedAttachDBFilename) && (null == _localDBInstance);
            }
        }

        protected internal override System.Security.PermissionSet CreatePermissionSet()
        {
            System.Security.PermissionSet permissionSet = new System.Security.PermissionSet(System.Security.Permissions.PermissionState.None);
            permissionSet.AddPermission(new SqlClientPermission(this));
            return permissionSet;
        }

        protected internal override string Expand()
        {
            if (null != _expandedAttachDBFilename)
            {
                return ExpandKeyword(KEY.AttachDBFilename, _expandedAttachDBFilename);
            }
            else
            {
                return base.Expand();
            }
        }

        private static bool CompareHostName(ref string host, string name, bool fixup)
        {
            // same computer name or same computer name + "\named instance"
            bool equal = false;

            if (host.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                if (fixup)
                {
                    host = ".";
                }
                equal = true;
            }
            else if (host.StartsWith(name + @"\", StringComparison.OrdinalIgnoreCase))
            {
                if (fixup)
                {
                    host = "." + host.Substring(name.Length);
                }
                equal = true;
            }
            return equal;
        }

        // this hashtable is meant to be read-only translation of parsed string
        // keywords/synonyms to a known keyword string
        internal static Hashtable GetParseSynonyms()
        {

            Hashtable hash = _sqlClientSynonyms;
            if (null == hash)
            {
                hash = new Hashtable(SqlConnectionStringBuilder.KeywordsCount + SynonymCount);
                hash.Add(KEY.ApplicationIntent, KEY.ApplicationIntent);
                hash.Add(KEY.Application_Name, KEY.Application_Name);
                hash.Add(KEY.AsynchronousProcessing, KEY.AsynchronousProcessing);
                hash.Add(KEY.AttachDBFilename, KEY.AttachDBFilename);
                hash.Add(KEY.PoolBlockingPeriod, KEY.PoolBlockingPeriod);
                hash.Add(KEY.Connect_Timeout, KEY.Connect_Timeout);
                hash.Add(KEY.Connection_Reset, KEY.Connection_Reset);
                hash.Add(KEY.Context_Connection, KEY.Context_Connection);
                hash.Add(KEY.Current_Language, KEY.Current_Language);
                hash.Add(KEY.Data_Source, KEY.Data_Source);
                hash.Add(KEY.Encrypt, KEY.Encrypt);
                hash.Add(KEY.Enlist, KEY.Enlist);
                hash.Add(KEY.FailoverPartner, KEY.FailoverPartner);
                hash.Add(KEY.Initial_Catalog, KEY.Initial_Catalog);
                hash.Add(KEY.Integrated_Security, KEY.Integrated_Security);
                hash.Add(KEY.Load_Balance_Timeout, KEY.Load_Balance_Timeout);
                hash.Add(KEY.MARS, KEY.MARS);
                hash.Add(KEY.Max_Pool_Size, KEY.Max_Pool_Size);
                hash.Add(KEY.Min_Pool_Size, KEY.Min_Pool_Size);
                hash.Add(KEY.MultiSubnetFailover, KEY.MultiSubnetFailover);
                hash.Add(KEY.TransparentNetworkIPResolution, KEY.TransparentNetworkIPResolution);
                hash.Add(KEY.Network_Library, KEY.Network_Library);
                hash.Add(KEY.Packet_Size, KEY.Packet_Size);
                hash.Add(KEY.Password, KEY.Password);
                hash.Add(KEY.Persist_Security_Info, KEY.Persist_Security_Info);
                hash.Add(KEY.Pooling, KEY.Pooling);
                hash.Add(KEY.Replication, KEY.Replication);
                hash.Add(KEY.TrustServerCertificate, KEY.TrustServerCertificate);
                hash.Add(KEY.TransactionBinding, KEY.TransactionBinding);
                hash.Add(KEY.Type_System_Version, KEY.Type_System_Version);
                hash.Add(KEY.ColumnEncryptionSetting, KEY.ColumnEncryptionSetting);
                hash.Add(KEY.EnclaveAttestationUrl, KEY.EnclaveAttestationUrl);
                hash.Add(KEY.User_ID, KEY.User_ID);
                hash.Add(KEY.User_Instance, KEY.User_Instance);
                hash.Add(KEY.Workstation_Id, KEY.Workstation_Id);
                hash.Add(KEY.Connect_Retry_Count, KEY.Connect_Retry_Count);
                hash.Add(KEY.Connect_Retry_Interval, KEY.Connect_Retry_Interval);
                hash.Add(KEY.Authentication, KEY.Authentication);
#if ADONET_CERT_AUTH                
                hash.Add(KEY.Certificate,                    KEY.Certificate);
#endif
                hash.Add(SYNONYM.APP, KEY.Application_Name);
                hash.Add(SYNONYM.Async, KEY.AsynchronousProcessing);
                hash.Add(SYNONYM.EXTENDED_PROPERTIES, KEY.AttachDBFilename);
                hash.Add(SYNONYM.INITIAL_FILE_NAME, KEY.AttachDBFilename);
                hash.Add(SYNONYM.CONNECTION_TIMEOUT, KEY.Connect_Timeout);
                hash.Add(SYNONYM.TIMEOUT, KEY.Connect_Timeout);
                hash.Add(SYNONYM.LANGUAGE, KEY.Current_Language);
                hash.Add(SYNONYM.ADDR, KEY.Data_Source);
                hash.Add(SYNONYM.ADDRESS, KEY.Data_Source);
                hash.Add(SYNONYM.NETWORK_ADDRESS, KEY.Data_Source);
                hash.Add(SYNONYM.SERVER, KEY.Data_Source);
                hash.Add(SYNONYM.DATABASE, KEY.Initial_Catalog);
                hash.Add(SYNONYM.TRUSTED_CONNECTION, KEY.Integrated_Security);
                hash.Add(SYNONYM.Connection_Lifetime, KEY.Load_Balance_Timeout);
                hash.Add(SYNONYM.NET, KEY.Network_Library);
                hash.Add(SYNONYM.NETWORK, KEY.Network_Library);
                hash.Add(SYNONYM.Pwd, KEY.Password);
                hash.Add(SYNONYM.PERSISTSECURITYINFO, KEY.Persist_Security_Info);
                hash.Add(SYNONYM.UID, KEY.User_ID);
                hash.Add(SYNONYM.User, KEY.User_ID);
                hash.Add(SYNONYM.WSID, KEY.Workstation_Id);
                Debug.Assert(SqlConnectionStringBuilder.KeywordsCount + SynonymCount == hash.Count, "incorrect initial ParseSynonyms size");
                _sqlClientSynonyms = hash;
            }
            return hash;
        }

        internal string ObtainWorkstationId()
        {
            // If not supplied by the user, the default value is the MachineName
            // Note: In Longhorn you'll be able to rename a machine without
            // rebooting.  Therefore, don't cache this machine name.
            string result = WorkstationId;
            if (null == result)
            {
                // permission to obtain Environment.MachineName is Asserted
                // since permission to open the connection has been granted
                // the information is shared with the server, but not directly with the user
                result = ADP.MachineName();
                ValidateValueLength(result, TdsEnums.MAXLEN_HOSTNAME, KEY.Workstation_Id);
            }
            return result;
        }

        static internal Hashtable NetlibMapping()
        {
            const int NetLibCount = 8;

            Hashtable hash = _netlibMapping;
            if (null == hash)
            {
                hash = new Hashtable(NetLibCount);
                hash.Add(NETLIB.TCPIP, TdsEnums.TCP);
                hash.Add(NETLIB.NamedPipes, TdsEnums.NP);
                hash.Add(NETLIB.Multiprotocol, TdsEnums.RPC);
                hash.Add(NETLIB.BanyanVines, TdsEnums.BV);
                hash.Add(NETLIB.AppleTalk, TdsEnums.ADSP);
                hash.Add(NETLIB.IPXSPX, TdsEnums.SPX);
                hash.Add(NETLIB.VIA, TdsEnums.VIA);
                hash.Add(NETLIB.SharedMemory, TdsEnums.LPC);
                Debug.Assert(NetLibCount == hash.Count, "incorrect initial NetlibMapping size");
                _netlibMapping = hash;
            }
            return hash;
        }

        static internal bool ValidProtocal(string protocal)
        {
            switch (protocal)
            {
                case TdsEnums.TCP:
                case TdsEnums.NP:
                case TdsEnums.VIA:
                case TdsEnums.LPC:
                    return true;

                //              case TdsEnums.RPC  :  Invalid Protocals
                //              case TdsEnums.BV   :
                //              case TdsEnums.ADSP :
                //              case TdsEnums.SPX  :
                default:
                    return false;
            }
        }

        private void ValidateValueLength(string value, int limit, string key)
        {
            if (limit < value.Length)
            {
                throw ADP.InvalidConnectionOptionValueLength(key, limit);
            }
        }

        internal static void VerifyLocalHostAndFixup(ref string host, bool enforceLocalHost, bool fixup)
        {
            if (ADP.IsEmpty(host))
            {
                if (fixup)
                {
                    host = ".";
                }
            }
            else if (!CompareHostName(ref host, @".", fixup) &&
                     !CompareHostName(ref host, @"(local)", fixup))
            {
                // Fix-up completed in CompareHostName if return value true.
                string name = ADP.GetComputerNameDnsFullyQualified(); // i.e, machine.location.corp.company.com
                if (!CompareHostName(ref host, name, fixup))
                {
                    int separatorPos = name.IndexOf('.'); // to compare just 'machine' part
                    if ((separatorPos <= 0) || !CompareHostName(ref host, name.Substring(0, separatorPos), fixup))
                    {
                        if (enforceLocalHost)
                        {
                            throw ADP.InvalidConnectionOptionValue(KEY.AttachDBFilename);
                        }
                    }
                }
            }
        }

        internal Microsoft.Data.SqlClient.ApplicationIntent ConvertValueToApplicationIntent()
        {
            object value = base.Parsetable[KEY.ApplicationIntent];
            if (value == null)
            {
                return DEFAULT.ApplicationIntent;
            }

            // when wrong value is used in the connection string provided to SqlConnection.ConnectionString or c-tor,
            // wrap Format and Overflow exceptions with Argument one, to be consistent with rest of the keyword types (like int and bool)
            try
            {
                return DbConnectionStringBuilderUtil.ConvertToApplicationIntent(KEY.ApplicationIntent, value);
            }
            catch (FormatException e)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.ApplicationIntent, e);
            }
            catch (OverflowException e)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.ApplicationIntent, e);
            }
            // ArgumentException and other types are raised as is (no wrapping)
        }

        internal Microsoft.Data.SqlClient.PoolBlockingPeriod ConvertValueToPoolBlockingPeriod()
        {
            object value = base.Parsetable[KEY.PoolBlockingPeriod];
            if (value == null)
            {
                return DEFAULT.PoolBlockingPeriod;
            }

            try
            {
                return DbConnectionStringBuilderUtil.ConvertToPoolBlockingPeriod(KEY.PoolBlockingPeriod, value);
            }
            catch (FormatException e)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.PoolBlockingPeriod, e);
            }
            catch (OverflowException e)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.PoolBlockingPeriod, e);
            }
        }

        internal SqlAuthenticationMethod ConvertValueToAuthenticationType()
        {
            object value = base.Parsetable[KEY.Authentication];

            string valStr = value as string;
            if (valStr == null)
            {
                return DEFAULT.Authentication;
            }

            try
            {
                return DbConnectionStringBuilderUtil.ConvertToAuthenticationType(KEY.Authentication, valStr);
            }
            catch (FormatException e)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.Authentication, e);
            }
            catch (OverflowException e)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.Authentication, e);
            }
        }

        /// <summary>
        /// Convert the value to SqlConnectionColumnEncryptionSetting.
        /// </summary>
        /// <returns></returns>
        internal SqlConnectionColumnEncryptionSetting ConvertValueToColumnEncryptionSetting()
        {
            object value = base.Parsetable[KEY.ColumnEncryptionSetting];

            string valStr = value as string;
            if (valStr == null)
            {
                return DEFAULT.ColumnEncryptionSetting;
            }

            try
            {
                return DbConnectionStringBuilderUtil.ConvertToColumnEncryptionSetting(KEY.ColumnEncryptionSetting, valStr);
            }
            catch (FormatException e)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.ColumnEncryptionSetting, e);
            }
            catch (OverflowException e)
            {
                throw ADP.InvalidConnectionOptionValue(KEY.ColumnEncryptionSetting, e);
            }
        }

        internal bool ConvertValueToEncrypt()
        {
            // If the Authentication keyword is provided, default to Encrypt=true;
            // otherwise keep old default for backwards compatibility
            object authValue = base.Parsetable[KEY.Authentication];
            bool defaultEncryptValue = (authValue == null) ? DEFAULT.Encrypt : true;
            return ConvertValueToBoolean(KEY.Encrypt, defaultEncryptValue);
        }
    }
}

