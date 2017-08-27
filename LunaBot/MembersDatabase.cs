using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Text;
using Anovase;

/// <summary>
/// Represents the main MS SQL database for ReAccess Account with all its dependencies and commands.
/// </summary>
public class MembersDatabase
{
	protected const string _ConnectionString =
#if true
	@"Data Source=192.168.7.5\FOUNDATIONONE;Initial Catalog=anovase_private;Persist Security Info=True;User ID=masterConf;Password=CallForAnother500/;Encrypt=True;TrustServerCertificate=True";
#else
	@"Data Source=CFS\FOUNDATIONONE;Initial Catalog=anovase_private;IntegratedSecurity=True";
#endif
	//protected SqlConnection new SqlConnection(_ConnectionString);

    /// <summary>
    /// Initializes the database connection and starts reading and writing from and to it.
    /// </summary>
	public MembersDatabase()
	{
        // Connect to MS SQL server
        //new SqlConnection(_ConnectionString) = new SqlConnection(_ConnectionString);
        //new SqlConnection(_ConnectionString).Open();
	}

    /// <summary>
    /// Inserts a new user into the database.
    /// </summary>
    /// <param name="User">The account data to insert from.</param>
    /*public void InsertUser(AccountDataCollection User)
    {
        SqlCommand InsertCmd = new SqlCommand();
        InsertCmd.CommandText = "INSERT INTO Accounts " + User.KeyChainString + " VALUES " + User.ParamChainString + ";";
        var con = (InsertCmd.Connection = new SqlConnection(_ConnectionString)).Open();
        User.ParseParams(ref InsertCmd);
        InsertCmd.ExecuteNonQuery();
		con.Close();
    }*/

    /// <summary>
    /// Checks if the user exists in the database.
    /// </summary>
    /// <param name="User">User credentials to check.</param>
    /// <returns>True is the user exists, False if not.</returns>
    public bool UserExists(string User, bool handle)
    {
        SqlCommand SelectCmd = new SqlCommand();
		SqlConnection con1 = new SqlConnection(_ConnectionString);
		SqlDataReader Reader = null;
		if (handle)
			SelectCmd.CommandText = "IF EXISTS(SELECT [UID] FROM [MembersHeadstore] WHERE [Handle] = @uid) BEGIN SELECT 1; END ELSE BEGIN SELECT 0; END";
		else
			SelectCmd.CommandText = "IF EXISTS(SELECT [UID] FROM [MembersHeadstore] WHERE [UID] = @uid) BEGIN SELECT 1; END ELSE BEGIN SELECT 0; END";
        (SelectCmd.Connection = con1).Open();
        SqlParameter UPara = new SqlParameter("@uid", User);
        if (handle)
			UPara.SqlDbType = System.Data.SqlDbType.VarChar;
		else
			UPara.SqlDbType = System.Data.SqlDbType.UniqueIdentifier;
		SelectCmd.Parameters.Add(UPara);
        Reader = SelectCmd.ExecuteReader();
        Reader.Read();
        object R = Reader[0];
        Reader.Close();
		con1.Close();
        return R.ToString() == "1";
    }

    /// <summary>
    /// Checks if the user's credentials are valid.
    /// </summary>
    /// <param name="User">User credentials to check.</param>
    /// <returns>True is the user's credentials are valid, False if not.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /*public bool ValidateCredentials(AccountCredentials User)
    {
        if (!UserExists(User)) throw new ArgumentOutOfRangeException("User", "The user credentials you're trying to validate, doesn't exist in the database.");
        SqlCommand SelectCmd = new SqlCommand();
        SqlDataReader Reader = null;
        SelectCmd.CommandText = "SELECT Password FROM Accounts WHERE Username = @Username;";
        (SelectCmd.Connection = new SqlConnection(_ConnectionString)).Open();
        SqlParameter UPara = new SqlParameter("@Username", User.Username);
        UPara.SqlDbType = System.Data.SqlDbType.VarChar;
        SelectCmd.Parameters.Add(UPara);
        Reader = SelectCmd.ExecuteReader();
        Reader.Read();
        object R = Reader[0];
        Reader.Close();
        return R.ToString().Substring(0, 40) == BitConverter.ToString(User.SafePassword).Replace("-","");
    }*/

    /// <summary>
    /// Updates a field over the database for a specific user.
    /// </summary>
    /// <param name="User">User data collection to load from.</param>
    /*public void UpdateUser(AccountDataCollection User)
    {
        if (!UserExists(User.Credentials)) throw new ArgumentOutOfRangeException("User", "The user credentials you're trying to validate, doesn't exist in the database.");
        SqlCommand UpdateCmd = new SqlCommand();
        User.ParseParams(ref UpdateCmd);
        (UpdateCmd.Connection = new SqlConnection(_ConnectionString)).Open();
        foreach (SqlParameter Par in User.AllKeys)
        {
            if (Par.SourceColumn == "Username") continue;
            UpdateCmd.CommandText = "UPDATE Accounts SET " + Par.SourceColumn + " = " + Par.ParameterName + " WHERE Username = @Username;";
            UpdateCmd.ExecuteNonQuery();
        }
    }*/

    /// <summary>
    /// Loads the user data from the database and stores it into an account data collection.
    /// </summary>
    /// <param name="User">Credentials to use as a selection key.</param>
    /// <returns></returns>
    public Dictionary<string, object> LoadMemberHeaderData(string User, bool handle = false)
    {
        //AccountDataCollection Collection = new AccountDataCollection(User);
        if (!UserExists(User, handle))
			throw new ArgumentOutOfRangeException("User", "The user you're trying to access, doesn't exist in the database.");
        SqlCommand SelectCmd = new SqlCommand();
		SqlConnection con1 = new SqlConnection(_ConnectionString);
		SqlDataReader Reader = null;
		if (handle)
			SelectCmd.CommandText = "SELECT * FROM [MembersHeadstore] WHERE [Handle] = @uid;";
		else
			SelectCmd.CommandText = "SELECT * FROM [MembersHeadstore] WHERE [UID] = @uid;";
        (SelectCmd.Connection = con1).Open();
        SelectCmd.Parameters.Add(new SqlParameter("@uid", User));
        Reader = SelectCmd.ExecuteReader();
        Reader.Read();
		var header = new Dictionary<string, object>();
		for (int i = 0; i < Reader.FieldCount; i++)
			if (!handle || i != 0)
				header.Add(Reader.GetName(i), Reader[i]);
        Reader.Close();
		con1.Close();
        return header;
    }

	public Dictionary<string, object> LoadMemberStatusData(string User, bool handle = false)
	{
		//AccountDataCollection Collection = new AccountDataCollection(User);
		if (!UserExists(User, handle))
			throw new ArgumentOutOfRangeException("User", "The user you're trying to access, doesn't exist in the database.");
		SqlCommand SelectCmd = new SqlCommand();
		SqlConnection con1 = new SqlConnection(_ConnectionString);
		SqlDataReader Reader = null;
		if (handle)
			SelectCmd.CommandText = "SELECT * FROM [MembersState] WHERE [UID] = (SELECT [UID] FROM [MembersHeadstore] WHERE [Handle] = @uid);";
		else
			SelectCmd.CommandText = "SELECT * FROM [MembersState] WHERE [UID] = @uid;";
		(SelectCmd.Connection = con1).Open();
		SelectCmd.Parameters.Add(new SqlParameter("@uid", User));
		Reader = SelectCmd.ExecuteReader();
		Reader.Read();
		var header = new Dictionary<string, object>();
		for (int i = 0; i < Reader.FieldCount; i++)
			if (!handle || i != 0)
				header.Add(Reader.GetName(i), Reader[i]);
		Reader.Close();
		con1.Close();
		return header;
	}

	public List<MemberDataSet> LoadAllMembers()
	{
		//AccountDataCollection Collection = new AccountDataCollection(User);

		SqlCommand HeaderCmd = new SqlCommand();
		SqlDataReader ReaderH = null;
		HeaderCmd.CommandText = "SELECT * FROM [MembersHeadstore];";
		SqlConnection con1 = new SqlConnection(_ConnectionString);
		(HeaderCmd.Connection = con1).Open();
		ReaderH = HeaderCmd.ExecuteReader();


		List<MemberDataSet> data = new List<MemberDataSet>();
		while (ReaderH.Read())
		{
			var header = new Dictionary<string, object>();
			for (int i = 0; i < ReaderH.FieldCount; i++)
				if (i != 0)
					header.Add(ReaderH.GetName(i), ReaderH[i]);

			SqlCommand StatusCmd = new SqlCommand();
			SqlConnection con2 = new SqlConnection(_ConnectionString);
			SqlDataReader ReaderS = null;
			StatusCmd.CommandText = "SELECT * FROM [MembersState] WHERE [UID] = '" + ReaderH["UID"] + "';";
			(StatusCmd.Connection = con2).Open();
			ReaderS = StatusCmd.ExecuteReader();

			var status = new Dictionary<string, object>();
			ReaderS.Read();
			for (int i = 0; i < ReaderS.FieldCount; i++)
				if (i != 0)
					status.Add(ReaderS.GetName(i), ReaderS[i]);
			data.Add(new MemberDataSet() { Header = header, Status = status });
			ReaderS.Close();
			con2.Close();
		}
		ReaderH.Close();
		con1.Close();
		data.Sort(new MemberDataSet.Comparer());
		return data;
	}

	public static readonly Dictionary<string, Dictionary<string, string>> LunaTransactFormat = new Dictionary<string, Dictionary<string, string>>()
	{
		{ "GET", new Dictionary<string, string>() { { "u", "SELECT * FROM [MembersOverview] WHERE [Handle] = @p1;" }, { "up", "SELECT {0} FROM [MembersOverview] WHERE [Handle] = @p1;" } } },
		{ "SET", new Dictionary<string, string>() { { "ue", "UPDATE [MembersOverview] SET {0} = @p2 WHERE [Handle] = @p1; SELECT {0} FROM [MembersOverview] WHERE [Handle] = @p1;" } } }
	};

	public static SqlCommand BuildTransactFromLunaQuery(LunaQuery query)
	{
		SqlCommandBuilder builder = new SqlCommandBuilder();
		List<string> paramList = new List<string>();
		foreach (var param in query.Params)
		{
			if (param is LunaPropertyParam prop)
				paramList.Add(builder.QuoteIdentifier(prop.PropertyName));
			else if (param is LunaEqualitySetter setter)
				paramList.Add(builder.QuoteIdentifier(setter.Property.PropertyName));
		}
		string baseTsql = LunaTransactFormat[query.Command][query.Demand];
		string com = string.Format(baseTsql, paramList.ToArray());
		SqlCommand command = new SqlCommand();

		{
			int i = 1;
			foreach (var param in query.Params)
				if (param as LunaParameter != null)
				{
					command.Parameters.Add(new SqlParameter($"@p{i}", ((LunaParameter)param).Value));
					i++;
				}
		}

		command.CommandText = com;

		return command;
	}


	public TsqlOutput ProcessTsqlQuery(SqlCommand command)
	{
		SqlConnection con1 = new SqlConnection(_ConnectionString);
		try
		{
			con1.Open();
			command.Connection = con1;
			SqlDataReader reader = command.ExecuteReader();

			if (!reader.HasRows)
				return new TsqlOutput("Query failed: Result data has no entry.", null);
			TsqlOutput output = new TsqlOutput();
			while (reader.Read())
			{
				var rowData = new Dictionary<string, object>();
				for (int i = 0; i < reader.FieldCount; i++)
					rowData.Add(reader.GetName(i), reader.GetFieldValue<object>(i));
				output.Data.Add(rowData);
			}

			return output;
		}
		catch (Exception ex)
		{
			return new TsqlOutput("Query failed: " + ex.Message, null);
		}
		finally
		{
			con1.Close();
		}
	}

	/// <summary>
	/// Closes the connection and stops the comminucation with the database.
	/// </summary>
	/*public void Dispose()
    {
        new SqlConnection(_ConnectionString).Close();
    }*/
}