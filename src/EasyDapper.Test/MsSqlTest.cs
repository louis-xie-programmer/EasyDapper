using EasyDapper.Extension.MsSql;
using EasyDapper.Extension.MsSql.Extension;

namespace EasyDapper.Test
{
    public class MsSqlTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            var context = new TestMsSqlContext();

            // Create the UserInfo table in the database
            if (context.Context.QuerySet<UserInfo>().ExistTable())
            {
                // If the table already exists, drop it
                context.Context.CommandSet<UserInfo>().DropTable().Wait();
            }

            context.Context.CommandSet<UserInfo>().CreateTable().Wait();

            // Create a new user info object
            var userInfo = new UserInfo()
            {
                Email = "xxx@gmail.com",
                Password = "111111"
            };

            // Insert the user info into the database
            context.Context.CommandSet<UserInfo>().Insert(userInfo);

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
            context.Context.CommandSet<UserInfo>().InsertAsyncList(userInfoList).Wait();

            // Delete user info where UserID is greater than 0
            context.Context.CommandSet<UserInfo>().Where(n => n.UserID > 0).Delete();

            // Alternatively, you can use BulkCopy method
            context.Context.CommandSet<UserInfo>().BulkCopy(userInfoList, 100);

            var count = context.Context.QuerySet<UserInfo>().Count();

            var list = context.Context.QuerySet<UserInfo>().WithNoLock()
                .Where(n=>n.CreatedTime< DateTime.Now)
                .OrderBy(n => n.UserID)
                .Select(n=> new UserInfo() { UserID = n.UserID, Email = n.Email +  "mail", CreatedTime = DateTime.Now })
                .ToList();

            var sum = context.Context.QuerySet<UserInfo>()
                .Where(n => n.CreatedTime < DateTime.Now)
                .Sum(n => n.UserID);

            var userid = list.First().UserID;

            // Update the email of the user with the specified UserID
            var num = context.Context.CommandSet<UserInfo>()
                .Where(n => n.UserID == userid)
                .Update(n => new UserInfo() { Email = n.Email + "fffffmail"});

            if (count > 0)
            {

            }
        }
    }
}