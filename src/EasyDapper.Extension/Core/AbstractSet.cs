using System.Data;

namespace EasyDapper.Extension.Core
{
    public abstract class AbstractSet
    {
        public SqlProvider SqlProvider { get; protected set; }
        public IDbConnection DbCon { get; protected set; }
        public IDbTransaction DbTransaction { get; protected set; }

        protected AbstractSet(IDbConnection dbCon, SqlProvider sqlProvider, IDbTransaction dbTransaction)
        {
            SqlProvider = sqlProvider;
            DbCon = dbCon;
            DbTransaction = dbTransaction;
        }

        protected AbstractSet(IDbConnection dbCon, SqlProvider sqlProvider)
        {
            SqlProvider = sqlProvider;
            DbCon = dbCon;
        }
    }
}
