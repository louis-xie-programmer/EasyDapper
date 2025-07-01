# EasyDapper

EasyDapper 是一个基于 Dapper 的 .NET 6 ORM 扩展，专注于简化 SQL Server (MsSql) 的数据访问，支持链式表达式、批量操作、分页、事务等常用功能。

## 目录结构

- `src/EasyDapper`：Dapper 源码副本（兼容性/扩展用）
- `src/EasyDapper.Extension`：EasyDapper 核心扩展，包含表达式解析、链式 API、分页、聚合等
- `src/EasyDapper.Extension.MsSql`：MsSql 专用实现与扩展
- `src/EasyDapper.Test`：单元测试与用例

## 主要特性

- 基于 Dapper，性能优越
- 支持表达式树条件查询、链式调用
- 支持分页、聚合、分组、排序
- 支持批量插入（BulkCopy）
- 支持事务
- 支持异步操作
- 支持 MsSql 特性（如 WITH(NOLOCK)）

## 安装

1. 克隆本仓库
2. 用 Visual Studio 2022 或以上版本打开解决方案，编译即可
3. 依赖包：
   - Dapper
   - Microsoft.Data.SqlClient
   - System.ComponentModel.Annotations

## 快速开始

### 1. 配置数据库连接

```csharp
using Microsoft.Data.SqlClient;
var conn = new SqlConnection("Server=...;Database=...;User Id=...;Password=...;Encrypt=False");
```

### 2. 查询示例

```csharp
using EasyDapper.Extension.MsSql;

// 查询所有用户
var users = conn.QuerySet<UserInfo>().ToList();

// 条件查询
var list = conn.QuerySet<UserInfo>()
    .Where(u => u.CreatedTime < DateTime.Now)
    .OrderBy(u => u.UserID)
    .ToList();

// 分页查询
var page = conn.QuerySet<UserInfo>().PageList(1, 20);

// 聚合
int count = conn.QuerySet<UserInfo>().Count();
bool exists = conn.QuerySet<UserInfo>().Where(u => u.Email == "test@test.com").Exists();
```

### 3. 插入/批量插入

```csharp
// 单条插入
conn.CommandSet<UserInfo>().Insert(new UserInfo { ... });

// 批量插入
conn.CommandSet<UserInfo>().BatchInsert(listOfUserInfo);
```

### 4. 事务

```csharp
conn.Transaction(ctx => {
    ctx.CommandSet<UserInfo>().Insert(new UserInfo { ... });
    ctx.CommandSet<UserInfo>().Delete("WHERE UserID = 1");
});
```

## 进阶用法

- 支持异步方法（如 `ToListAsync()`、`InsertAsync()`）
- 支持表达式树 Select/GroupBy/Sum
- 支持自定义 Provider 扩展

## 依赖与兼容性

- .NET 6
- Dapper
- Microsoft.Data.SqlClient

## 贡献

欢迎提交 Issue 和 PR。

## License

本项目遵循 Apache 2.0 协议，Dapper 相关代码遵循其原始协议。

---

如需详细 API 说明，请参考源码注释与测试用例。
