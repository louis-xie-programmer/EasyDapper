using EasyDapper.Extension.MsSql;
using System.Text.RegularExpressions;

namespace EasyDapper.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            var context = new TestMsSqlContext();
            var count = context.Context.QuerySet<UserInfo>().Count();

            var list = context.Context.QuerySet<UserInfo>().WithNoLock().Where(n=>n.CreatedTime< DateTime.Now).Select(n=> new UserInfo() { UserID = n.UserID + count, Email = n.Email +  "mail", CreatedTime = DateTime.Now }).ToList();

            if (count > 0)
            {

            }
        }
    }
}