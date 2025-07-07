using EasyDapper.Extension.MySql;

namespace EasyDapper.Test;

public class MySqlTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        using (var connection = new MySql.Data.MySqlClient.MySqlConnection("Server=localhost;Port=13306;Database=test;User Id=root;Password=easymysql;"))
        {
            connection.Open();
            // Create the UserInfo table in the database
            if (connection.QuerySet<UserInfo>().ExistTable())
            {
                // If the table already exists, drop it
                connection.CommandSet<UserInfo>().DropTable().Wait();
            }
            connection.CommandSet<UserInfo>().CreateTable().Wait();

            // Create a new user info object
            var userInfo = new UserInfo()
            {
                Email = "xxx@gmail.com",
                Password = "111111"
            };

            // Insert the user info into the database
            connection.CommandSet<UserInfo>().Insert(userInfo);

            // Delete user info where UserID is greater than 0
            connection.CommandSet<UserInfo>().Where(n => n.UserID > 0).Delete();

            var userInfoList = new List<UserInfo>();
            for (int i = 0; i < 10; i++)
            {
                userInfoList.Add(new UserInfo()
                {
                    Email = $"x{i}@qq.com",
                    Password = "111111"
                });
            }
            // Bulk copy the list of user info into the database
            connection.CommandSet<UserInfo>().InsertAsyncList(userInfoList).Wait();

            var count = connection.QuerySet<UserInfo>().Count();

            var list = connection.QuerySet<UserInfo>().WithNoLock()
                .Where(n => n.CreatedTime < DateTime.Now)
                .OrderBy(n => n.UserID)
                .Select(n => new UserInfo() { UserID = n.UserID, Email = n.Email + "mail", CreatedTime = DateTime.Now })
                .ToList();

            var sum = connection.QuerySet<UserInfo>()
                .Where(n => n.CreatedTime < DateTime.Now)
                .Sum(n => n.UserID);

            var userid = list.First().UserID;

            // Update the email of the user with the specified UserID
            var num = connection.CommandSet<UserInfo>()
                .Where(n => n.UserID == userid)
                .Update(n => new UserInfo() { Email = n.Email + "fffffmail" });

            if (count > 0)
            {

            }
            // Create a new user info object
        }
    }
}
