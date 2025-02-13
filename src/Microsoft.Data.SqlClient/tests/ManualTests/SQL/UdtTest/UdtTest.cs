// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Data;
using System.Data.SqlTypes;
using Microsoft.Samples.SqlServer;
using Xunit;

namespace Microsoft.Data.SqlClient.ManualTesting.Tests
{
    public class UdtTest
    {
        private readonly string _connStr;

        public UdtTest()
        {
            _connStr = (new SqlConnectionStringBuilder(DataTestUtility.TcpConnStr) { InitialCatalog = DataTestUtility.UdtTestDbName }).ConnectionString;
        }

        [ConditionalFact(typeof(DataTestUtility), nameof(DataTestUtility.IsUdtTestDatabasePresent), nameof(DataTestUtility.AreConnStringsSetup), nameof(DataTestUtility.IsNotAzureServer))]
        public void ReaderTest()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                using (SqlCommand com = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = "select * from TestTable"
                })
                using (SqlDataReader reader = com.ExecuteReader())
                {

                    Utf8String[] expectedValues =
                        {
                        new Utf8String("a"),
                        new Utf8String("is"),
                        new Utf8String("test"),
                        new Utf8String("this")
                    };
                    int currentValue = 0;
                    do
                    {
                        while (reader.Read())
                        {
                            DataTestUtility.AssertEqualsWithDescription(1, reader.FieldCount, "Unexpected FieldCount.");
                            DataTestUtility.AssertEqualsWithDescription(expectedValues[currentValue], reader.GetValue(0), "Unexpected Value.");
                            DataTestUtility.AssertEqualsWithDescription(expectedValues[currentValue], reader.GetSqlValue(0), "Unexpected SQL Value.");

                            currentValue++;
                        }
                    }
                    while (reader.NextResult());

                    DataTestUtility.AssertEqualsWithDescription(expectedValues.Length, currentValue, "Received less values than expected.");
                }
            }
        }

        [ConditionalFact(typeof(DataTestUtility), nameof(DataTestUtility.IsUdtTestDatabasePresent), nameof(DataTestUtility.AreConnStringsSetup), nameof(DataTestUtility.IsNotAzureServer))]
        public void ExecuteScalarTest()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand com = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = "select * from TestTable"
                })
                {
                    DataTestUtility.AssertEqualsWithDescription(new Utf8String("a"), com.ExecuteScalar(), "Unexpected value.");
                }
            }
        }

        [ConditionalFact(typeof(DataTestUtility), nameof(DataTestUtility.IsUdtTestDatabasePresent), nameof(DataTestUtility.AreConnStringsSetup), nameof(DataTestUtility.IsNotAzureServer))]
        public void InputParameterTest()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                using (SqlCommand com = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = "insert into TestTable values (@p);" +
                                  "SELECT * FROM TestTable"
                })
                {
                    SqlParameter p = com.Parameters.Add("@p", SqlDbType.Udt);
                    p.UdtTypeName = "Utf8String";
                    p.Value = new Utf8String("this is an input param test");

                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        com.Transaction = trans;
                        using (SqlDataReader reader = com.ExecuteReader())
                        {

                            Utf8String[] expectedValues =
                            {
                                new Utf8String("a"),
                                new Utf8String("is"),
                                new Utf8String("test"),
                                new Utf8String("this"),
                                new Utf8String("this is an input param test")
                            };

                            int currentValue = 0;
                            do
                            {
                                while (reader.Read())
                                {
                                    DataTestUtility.AssertEqualsWithDescription(1, reader.FieldCount, "Unexpected FieldCount.");
                                    DataTestUtility.AssertEqualsWithDescription(expectedValues[currentValue], reader.GetValue(0), "Unexpected Value.");
                                    currentValue++;
                                }
                            }
                            while (reader.NextResult());
                            DataTestUtility.AssertEqualsWithDescription(expectedValues.Length, currentValue, "Received less values than expected.");
                        }
                    }
                }
            }
        }

        [ConditionalFact(typeof(DataTestUtility), nameof(DataTestUtility.IsUdtTestDatabasePresent), nameof(DataTestUtility.AreConnStringsSetup), nameof(DataTestUtility.IsNotAzureServer))]
        public void OutputParameterTest()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                using (SqlCommand com = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = "UDTTest",
                    CommandType = CommandType.StoredProcedure
                })
                {
                    SqlParameter p = com.Parameters.Add("@value", SqlDbType.Udt);
                    p.UdtTypeName = "Utf8String";
                    p.Direction = ParameterDirection.Output;

                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        do
                        {
                            while (reader.Read())
                            {
                                DataTestUtility.AssertEqualsWithDescription(0, reader.FieldCount, "Should not have any reader results.");
                            }
                        }
                        while (reader.NextResult());
                    }
                    DataTestUtility.AssertEqualsWithDescription(new Utf8String("this is an outparam test"), p.Value, "Unexpected parameter value.");
                }
            }
        }

        [ConditionalFact(typeof(DataTestUtility), nameof(DataTestUtility.IsUdtTestDatabasePresent), nameof(DataTestUtility.AreConnStringsSetup), nameof(DataTestUtility.IsNotAzureServer))]
        public void FillTest()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                DataSet ds = new DataSet();

                using (SqlDataAdapter adapter = new SqlDataAdapter("select * from TestTable", conn))
                {
                    adapter.Fill(ds);

                    Utf8String[] expectedValues =
                    {
                        new Utf8String("a"),
                        new Utf8String("is"),
                        new Utf8String("test"),
                        new Utf8String("this")
                    };
                    VerifyDataSet(ds, expectedValues);
                }
            }
        }

        [ConditionalFact(typeof(DataTestUtility), nameof(DataTestUtility.IsUdtTestDatabasePresent), nameof(DataTestUtility.AreConnStringsSetup), nameof(DataTestUtility.IsNotAzureServer))]
        public void UpdateTest()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                DataSet ds = new DataSet();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    SqlDataAdapter adapter = new SqlDataAdapter("select * from TestTable", conn);
                    using (SqlCommandBuilder builder = new SqlCommandBuilder(adapter))
                    {
                        adapter.SelectCommand.Transaction = trans;

                        adapter.Fill(ds);

                        ds.Tables[0].Rows[0][0] = new Utf8String("updated");

                        adapter.Update(ds);

                        ds.Reset();

                        adapter.Fill(ds);
                    }
                }

                Utf8String[] expectedValues =
                {
                    new Utf8String("is"),
                    new Utf8String("test"),
                    new Utf8String("this"),
                    new Utf8String("updated")
                };
                VerifyDataSet(ds, expectedValues);
            }
        }

        [ConditionalFact(typeof(DataTestUtility), nameof(DataTestUtility.IsUdtTestDatabasePresent), nameof(DataTestUtility.AreConnStringsSetup), nameof(DataTestUtility.IsNotAzureServer))]
        public void NullTest()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();

                using (SqlCommand com = new SqlCommand()
                {
                    Connection = conn,
                    CommandText = "insert into TestTableNull values (@p);" +
                                  "SELECT * FROM TestTableNull"
                })
                {
                    SqlParameter p = com.Parameters.Add("@p", SqlDbType.Udt);
                    p.UdtTypeName = "Utf8String";
                    p.Value = DBNull.Value;

                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        com.Transaction = trans;
                        using (SqlDataReader reader = com.ExecuteReader())
                        {

                            Utf8String[] expectedValues = {
                                new Utf8String("this"),
                                new Utf8String("is"),
                                new Utf8String("a"),
                                new Utf8String("test")
                            };

                            int currentValue = 0;
                            do
                            {
                                while (reader.Read())
                                {
                                    DataTestUtility.AssertEqualsWithDescription(1, reader.FieldCount, "Unexpected FieldCount.");
                                    if (currentValue < expectedValues.Length)
                                    {
                                        DataTestUtility.AssertEqualsWithDescription(expectedValues[currentValue], reader.GetValue(0), "Unexpected Value.");
                                        DataTestUtility.AssertEqualsWithDescription(expectedValues[currentValue], reader.GetSqlValue(0), "Unexpected SQL Value.");
                                    }
                                    else
                                    {
                                        DataTestUtility.AssertEqualsWithDescription(DBNull.Value, reader.GetValue(0), "Unexpected Value.");

                                        Utf8String sqlValue = (Utf8String)reader.GetSqlValue(0);
                                        INullable iface = sqlValue as INullable;
                                        Assert.True(iface != null, "Expected interface cast to return a non-null value.");
                                        Assert.True(iface.IsNull, "Expected interface cast to have IsNull==true.");
                                    }

                                    currentValue++;
                                    Assert.True(currentValue <= (expectedValues.Length + 1), "Expected to only hit one extra result.");
                                }
                            }
                            while (reader.NextResult());
                            DataTestUtility.AssertEqualsWithDescription(currentValue, (expectedValues.Length + 1), "Did not hit all expected values.");
                        }
                    }
                }
            }
        }

        private void VerifyDataSet(DataSet ds, Utf8String[] expectedValues)
        {
            DataTestUtility.AssertEqualsWithDescription(1, ds.Tables.Count, "Unexpected tables count.");
            DataTestUtility.AssertEqualsWithDescription(ds.Tables[0].Rows.Count, expectedValues.Length, "Unexpected rows count.");
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataTestUtility.AssertEqualsWithDescription(1, ds.Tables[0].Columns.Count, "Unexpected columns count.");
                DataTestUtility.AssertEqualsWithDescription(expectedValues[i], ds.Tables[0].Rows[i][0], "Unexpected value.");
            }
        }
    }
}
