using System;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

class Password
{
    const string availableChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!@#$%^&*()-_=+<,>.";

    const string passRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{20,20}$";
    int recCount = 0;
    int count = 0;
    static string connectionString;
    static SqlConnection cnn;
    string  PasswordGenerator(ref Random random,ref StringBuilder result)
    {
        result.Clear();
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
        }
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
            recCount++;

            Console.WriteLine("Recursive call");
            Console.WriteLine(res.ToString());
            return PasswordGenerator(ref random, ref result);
        }
    }
    
    void DBConnect()
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
    void AddData(ref Random random,ref StringBuilder sb)
    {
        string query = "Insert Into PasswordTable (password) VALUES (@pass)";
        SqlCommand cmd = new SqlCommand(query, cnn);
        cmd.Parameters.AddWithValue("@pass", this.PasswordGenerator(ref random, ref sb));
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            if (ex.Number == 2627)
            {
                this.AddData(ref random,ref sb);
            }
        }
        finally
        {
            Console.WriteLine(count++);
        }
    }
    static void Main(string[] args)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        
        Random random = new Random();
        Password passobj = new Password();
        StringBuilder sb = new StringBuilder();

        passobj.DBConnect();

        cnn.Open() ;
        for(int i=0;i<3000000;i++)
        {
            passobj.AddData(ref random,ref sb);
            
        }
        cnn.Close() ;
        Console.WriteLine("Total number of Recursion needed: "+passobj.recCount);

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"{elapsedMs/1000} seconds");
    }
}