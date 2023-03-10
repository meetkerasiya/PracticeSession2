using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

class Password
{
    const string availableChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz~!@#$%^&*()-_=+<,>.";
    //static List<string> passList= new List<string>();
    static ConcurrentDictionary<int, string> passList= new ConcurrentDictionary<int, string>();
    static int dictIndex = 0;
    const string smalls = "abcdefghijklmnopqrstuvwxyz";
    const string capitals = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string numbers = "0123456789";
    const string specials = "~&*()-_=+<!@#$%^,>.";
    const string passRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{20,20}$";
    
    int count = 0;
    static string connectionString;
    static SqlConnection cnn;
    static int current = 0;
    static int total = 100000;
    static string  PasswordGenerator(ref Random random,ref StringBuilder result)
    {
        result.Clear();
        int total_special=random.Next(2,5);
        int total_number = random.Next(2, 5);
        int total_smalls = random.Next(2, 5);
        int total_capital = 20 - total_smalls - total_number - total_special;

        for (int i = 0; i < total_special; i++)
        {
            result.Append(specials[random.Next(specials.Length)]);
        }
        for (int i=0;i<total_capital;i++)
        {
            result.Append(capitals[random.Next(capitals.Length)]);
        }
      
        for(int i=0;i<total_number;i++)
        {
            result.Append(numbers[random.Next(numbers.Length)]);
        }
        for(int i=0;i<total_smalls;i++)
        {
            result.Append(smalls[random.Next(smalls.Length)]);
        }
        return result.ToString();
        /*
        for(int i=0;i<20;i++)
        {
            result.Append(availableChars[random.Next(availableChars.Length)]);
        }
        if (Regex.IsMatch(result.ToString() , passRegex))
        {

               //Console.WriteLine(result.ToString());
               return result.ToString();
        }
        else
        {
            recCount++;

            //Console.WriteLine("Recursive call");
            //Console.WriteLine(result.ToString());
            return PasswordGenerator(ref random,ref result);
        }*/
    }
   
    string PasswordGenerate(ref Random random,ref StringBuilder result)
    {
        result.Clear();
        for (int i = 0; i < 5; i++)
        {
            result.Append(availableChars[random.Next(10)]);
        }
        for (int i = 5; i < 10; i++)
        {
            result.Append(availableChars[random.Next(10, 36)]);
        }
        for (int i = 10; i < 15; i++)
        {
            result.Append(availableChars[random.Next(36, 62)]);
        }
        for (int i = 15; i < 20; i++)
        {
            result.Append(availableChars[random.Next(62, availableChars.Length)]);
        }
        Random random1 = new Random();
        string temp=result.ToString();
       // Console.WriteLine(result.ToString());
        char[] res = new char[20];
        int j = 0;
        var num= Enumerable.Range(0, 20).OrderBy(c => Guid.NewGuid()).ToArray();
        foreach(var i in num)
        {
            res[i] = temp[j++];
        }
        //var res = result.ToString().ToCharArray().OrderBy(c=>random1.Next(2)).ToArray();
        //Console.WriteLine("Before if else:");
        //Console.WriteLine(res);
        string temp1=new string(res);
        if (Regex.IsMatch(temp1, passRegex))
        {
            //Console.WriteLine(result.ToString());
            return temp1;
        }
        else
        {
            

            Console.WriteLine("Recursive call");
            Console.WriteLine(res.ToString());
            return PasswordGenerator(ref random, ref result);
        }
    }
    
    static void DBConnect()
    {
        connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""C:\Users\Meet  Kerasiya\source\repos\PracticeSession2\PracticeSession2\Database1.mdf"";Integrated Security=True";
        cnn = new SqlConnection(connectionString);
        try
        {
            cnn.Open();

            Console.WriteLine("Conncetion established");
            SqlCommand cmd = new SqlCommand("TRUNCATE TABLE PasswordTable ",cnn);
            cmd.ExecuteNonQuery();
            //SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM PasswordTable ", cnn);
            //SqlDataReader reader= command.ExecuteReader();
            //reader.Read();
            //Console.WriteLine(reader.GetInt32(0));
            cnn.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Could not establish sql connection");
            Console.WriteLine(e.ToString());
            Environment.Exit(1);
        }
    }
    static async Task<int> AddDataToDB()
    {
        string password;
        while(current==passList.Count || passList.Count==0)
        {

        }
        while(current<passList.Count)
        {
            string query = "Insert Into PasswordTable (password) VALUES (@pass)";
            SqlCommand cmd = new SqlCommand(query, cnn);
            //cmd.Parameters.AddWithValue("@pass", passList[current++]);
            passList.TryGetValue(current, out password);
            cmd.Parameters.AddWithValue("@pass", password);
            //cmd.Parameters.AddWithValue("@pass", this.PasswordGenerator(ref random, ref sb));
            try
            {
                await cnn.OpenAsync();
                cmd.ExecuteNonQuery();

            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    AddDataToDB();
                }
            }
            finally
            {
                current++;
                await cnn.CloseAsync();
                Console.WriteLine(current);
            }
        }
        
        return 0;
    }
    static async Task<int> AddDataToDB2()
    {
        string password;
        DataTable dataTable= new DataTable();
        dataTable.Columns.Add(new DataColumn("password",typeof(string)));
        SqlBulkCopy copy = new SqlBulkCopy(cnn);
        copy.DestinationTableName = "[dbo].[PasswordTable]";
        copy.ColumnMappings.Add("password", "password");
        await Task.Delay(100);
        while(current==passList.Count || passList.Count==0)
        {

        }
        while(current<passList.Count)
        {
           
            DataRow row= dataTable.NewRow();
            //Console.WriteLine(passList.TryGetValue(current++, out password));
            //row[0] = password;
            row["password"] = passList[current++];
            dataTable.Rows.Add(row);
            try
            {
                await cnn.OpenAsync();
                copy.WriteToServer(dataTable);
                
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    AddDataToDB2();
                }
            }
            finally
            {
                Console.WriteLine(current);
                await cnn.CloseAsync();
            }
        }
        
        return 0;
    }

    static async Task<int> AddDataToDB3()
    {
        await Task.Delay(100);
        Random random= new Random();
        StringBuilder sb= new StringBuilder();
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add(new DataColumn("password", typeof(string)));
        string password;
        while (current == passList.Count || passList.Count == 0)
        {

        }
        while (current < passList.Count)
        {
            dataTable.Clear();
            DataRow row = dataTable.NewRow();
            passList.TryGetValue(current++, out password);
            row[0] = password;
            dataTable.Rows.Add(row);
            //row[0] = PasswordGenerator(ref random, ref sb);
            try
            {
                cnn.OpenAsync();
                using(SqlCommand cmd = new SqlCommand("AddPassword", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    
                    cmd.Parameters.Add(new SqlParameter("@data", SqlDbType.Structured ) { Value = dataTable });   
                    cmd.ExecuteNonQuery();
                    current++;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    total += 1;
                    AddDataToDB3();
                }
            }
            finally
            {
                cnn.CloseAsync();
                Console.WriteLine(current);
            }
        }

        return 0;
    }
    static async Task<int> AddDataToDB4()
    {
        await Task.Delay(100);
        Random random = new Random();
        StringBuilder sb = new StringBuilder();
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add(new DataColumn("password", typeof(string)));
        string password;
        SqlCommand cmd=cnn.CreateCommand();
        cmd.CommandText = "Execute PasswordTable @pass";
        while (current == passList.Count || passList.Count == 0)
        {

        }
        while (current < passList.Count)
        {
            dataTable.Clear();
            DataRow row = dataTable.NewRow();
            passList.TryGetValue(current++, out password);
            //row[0] = password;
            //dataTable.Rows.Add(row);
            //row[0] = PasswordGenerator(ref random, ref sb);
            try
            {
                await cnn.OpenAsync();
                cmd.Parameters.Add("@pass",SqlDbType.VarChar,50).Value = password;
                cmd.ExecuteNonQuery();
                //using (SqlCommand cmd = new SqlCommand("AddPass", cnn))
                //{
                //    cmd.CommandType = CommandType.StoredProcedure;


                //    cmd.Parameters.Add(new SqlParameter("@pass", SqlDbType.VarChar,50) { Value = password });
                //    cmd.ExecuteNonQuery();
                //    current++;
                //}
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    total += 1;
                    AddDataToDB3();
                }
            }
            finally
            {
                await cnn.CloseAsync();
                Console.WriteLine(current);
            }
        }

        return 0;
    }

    static async Task<int> AddDataToList()
    {
        Random random= new Random();
        StringBuilder sb = new StringBuilder();
        for(int i=0;i<total;i++)
        {
            passList.TryAdd(dictIndex++,PasswordGenerator(ref random, ref sb));
        }
        return 0;
    }
    static async Task Main(string[] args)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        DBConnect();
        //cnn.Open();
        Random random = new Random();
        Password passobj = new Password();
        StringBuilder sb = new StringBuilder();
        Task listTask=AddDataToList();
        Task DBTask = AddDataToDB3();

        //Task listTask = new Task(async()=>await AddDataToList());
        //Task DBTask = new Task(async() =>
        //{
        //    DBConnect();
        //    cnn.Open();
        //    await AddDataToDB();
        //});
        //listTask.Start();
        //DBTask.Start();
        Console.WriteLine(passList.Count);
        await Task.WhenAll(listTask, DBTask);
        //cnn.Close() ;
        
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"{elapsedMs/1000} seconds");
    }
}