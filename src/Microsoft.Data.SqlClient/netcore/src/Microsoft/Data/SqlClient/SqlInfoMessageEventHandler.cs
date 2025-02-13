// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Data.SqlClient
{
    /// <devdoc>
    ///    <para>
    ///       Represents the method that will handle the <see cref='Microsoft.Data.SqlClient.SqlConnection.InfoMessage'/> event of a <see cref='Microsoft.Data.SqlClient.SqlConnection'/>.
    ///    </para>
    /// </devdoc>
    public delegate void SqlInfoMessageEventHandler(object sender, SqlInfoMessageEventArgs e);
}
