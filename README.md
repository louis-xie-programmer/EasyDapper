# EasyDapper

EasyDapper ��һ������ Dapper �� .NET 6 ORM ��չ��רע�ڼ� SQL Server (MsSql) �����ݷ��ʣ�֧����ʽ���ʽ��������������ҳ������ȳ��ù��ܡ�

## Ŀ¼�ṹ

- `src/EasyDapper`��Dapper Դ�븱����������/��չ�ã�
- `src/EasyDapper.Extension`��EasyDapper ������չ���������ʽ��������ʽ API����ҳ���ۺϵ�
- `src/EasyDapper.Extension.MsSql`��MsSql ר��ʵ������չ
- `src/EasyDapper.Test`����Ԫ����������

## ��Ҫ����

- ���� Dapper��������Խ
- ֧�ֱ��ʽ��������ѯ����ʽ����
- ֧�ַ�ҳ���ۺϡ����顢����
- ֧���������루BulkCopy��
- ֧������
- ֧���첽����
- ֧�� MsSql ���ԣ��� WITH(NOLOCK)��

## ��װ

1. ��¡���ֿ�
2. �� Visual Studio 2022 �����ϰ汾�򿪽�����������뼴��
3. ��������
   - Dapper
   - Microsoft.Data.SqlClient
   - System.ComponentModel.Annotations

## ���ٿ�ʼ

### 1. �������ݿ�����

```csharp
using Microsoft.Data.SqlClient;
var conn = new SqlConnection("Server=...;Database=...;User Id=...;Password=...;Encrypt=False");
```

### 2. ��ѯʾ��

```csharp
using EasyDapper.Extension.MsSql;

// ��ѯ�����û�
var users = conn.QuerySet<UserInfo>().ToList();

// ������ѯ
var list = conn.QuerySet<UserInfo>()
    .Where(u => u.CreatedTime < DateTime.Now)
    .OrderBy(u => u.UserID)
    .ToList();

// ��ҳ��ѯ
var page = conn.QuerySet<UserInfo>().PageList(1, 20);

// �ۺ�
int count = conn.QuerySet<UserInfo>().Count();
bool exists = conn.QuerySet<UserInfo>().Where(u => u.Email == "test@test.com").Exists();
```

### 3. ����/��������

```csharp
// ��������
conn.CommandSet<UserInfo>().Insert(new UserInfo { ... });

// ��������
conn.CommandSet<UserInfo>().BatchInsert(listOfUserInfo);
```

### 4. ����

```csharp
conn.Transaction(ctx => {
    ctx.CommandSet<UserInfo>().Insert(new UserInfo { ... });
    ctx.CommandSet<UserInfo>().Delete("WHERE UserID = 1");
});
```

## �����÷�

- ֧���첽�������� `ToListAsync()`��`InsertAsync()`��
- ֧�ֱ��ʽ�� Select/GroupBy/Sum
- ֧���Զ��� Provider ��չ

## �����������

- .NET 6
- Dapper
- Microsoft.Data.SqlClient

## ����

��ӭ�ύ Issue �� PR��

## License

����Ŀ��ѭ Apache 2.0 Э�飬Dapper ��ش�����ѭ��ԭʼЭ�顣

---

������ϸ API ˵������ο�Դ��ע�������������
