# EasyDapper

EasyDapper 是一个基于 Dapper 的 .NET 6 ORM 扩展库，专注于 SQL Server (MsSql) 和 MySQL 数据访问优化，支持表达式查询、分页聚合、批量操作等强大功能。

## 目录结构

- `src/EasyDapper`: Dapper 源码副本及基础扩展
- `src/EasyDapper.Extension`: EasyDapper 核心扩展，包含表达式 API 实现
- `src/EasyDapper.Extension.MsSql`: MsSql 专用实现的扩展
- `src/EasyDapper.Extension.MySql`: MySQL 专用实现的扩展
- `src/EasyDapper.Test`: 单元测试代码（包含 MsSQL 和 MySQL 测试案例）

## 主要特性

- 基于表达式的强类型查询
- 支持分页、聚合、排序
- 支持批量操作(BulkCopy)
- 支持事务
- 支持异步操作
- 提供 MsSql 特性支持，如 WITH(NOLOCK)
- 完整的单元测试覆盖

## 安装

当前最新版本：
- EasyDapper.Extension.MsSql 1.0.0
- EasyDapper.Extension.MySql 1.0.0

### 通过NuGet安装

推荐使用NuGet包管理器进行安装：

#### MsSQL 版本
```
Install-Package EasyDapper.Extension.MsSql -Version 1.0.0
```

#### MySQL 版本
```
Install-Package EasyDapper.Extension.MySql -Version 1.0.0
```

### 从源码编译
1. 下载源码
2. 使用 Visual Studio 2022 打开解决方案即可运行
3. 依赖项
   - Dapper
   - Microsoft.Data.SqlClient (MsSql)
   - MySql.Data (MySQL)
   - System.ComponentModel.Annotations

## 快速开始

### 1. 创建数据库连接

#### MsSQL 连接
```csharp
using Microsoft.Data.SqlClient;
var conn = new SqlConnection("Server=...;Database=...;User Id=...;Password=...;Encrypt=False");
```

#### MySQL 连接
```csharp
using MySql.Data.MySqlClient;
var conn = new MySqlConnection("Server=localhost;Port=13306;Database=test;User Id=root;Password=easymysql;");
```

### 2. 数据库操作示例

#### 表创建与删除
```csharp
// 判断表是否存在
bool exists = context.QuerySet<UserInfo>().ExistTable();

// 删除表
context.CommandSet<UserInfo>().DropTable().Wait();

// 创建表
context.CommandSet<UserInfo>().CreateTable().Wait();
```

#### 插入操作
```csharp
// 创建一个用户信息对象
var userInfo = new UserInfo()
{
    Email = "xxx@gmail.com",
    Password = "111111"
};

// 插入单个用户
context.CommandSet<UserInfo>().Insert(userInfo);

// 批量插入
var userInfoList = new List<UserInfo>();
for (int i = 0; i < 10; i++)
{
    userInfoList.Add(new UserInfo()
    {
        Email = $"x{i}@qq.com",
        Password = "111111"
    });
}
context.Context.CommandSet<UserInfo>().InsertAsyncList(userInfoList).Wait();

// 或者使用 BulkCopy 方法
context.Context.CommandSet<UserInfo>().BulkCopy(userInfoList, 100);
```

#### 查询操作
```csharp
// 查询所有用户
var users = conn.QuerySet<UserInfo>().ToList();

// 条件查询
var list = conn.QuerySet<UserInfo>()
    .Where(u => u.CreatedTime < DateTime.Now)
    .OrderBy(u => u.UserID)
    .Select(u => new UserInfo() { UserID = u.UserID, Email = u.Email + "mail", CreatedTime = DateTime.Now })
    .ToList();

// 分页查询
var page = conn.QuerySet<UserInfo>()
    .Where(u => u.CreatedTime < DateTime.Now)
    .OrderBy(u => u.UserID)
    .PageList(1, 10);

// 聚合查询
int count = conn.QuerySet<UserInfo>().Count();
int sum = conn.QuerySet<UserInfo>().Where(u => u.CreatedTime < DateTime.Now).Sum(u => u.UserID);
bool exists = conn.QuerySet<UserInfo>().Where(u => u.Email == "test@test.com").Exists();
```

#### 更新操作
```csharp
var userid = list.First().UserID;

// 更新指定ID用户的邮箱
var num = context.CommandSet<UserInfo>()
    .Where(n => n.UserID == userid)
    .Update(n => new UserInfo() { Email = n.Email + "fffffmail" });
```

#### 删除操作
```csharp
// 删除所有用户
context.CommandSet<UserInfo>().Where(u => u.UserID > 0).Delete();
```

### 3. 事务处理
```csharp
conn.Transaction(ctx => {
    ctx.CommandSet<UserInfo>().Insert(new UserInfo { ... });
    ctx.CommandSet<UserInfo>().Delete("WHERE UserID = 1");
});
```

## 高级用法

- 支持异步操作，如 `ToListAsync()`、`InsertAsync()`
- 支持表达式 Select/GroupBy/Sum
- 支持自定义 Provider 扩展
- 支持 WITH(NOLOCK) 查询（仅 MsSQL）

## 技术栈

- .NET 6
- Dapper
- Microsoft.Data.SqlClient (MsSql)
- MySql.Data (MySQL)

## 参与贡献

欢迎提交 Issue 和 PR。

## License

本项目遵循 Apache 2.0 协议，Dapper 相关代码遵循原始协议。