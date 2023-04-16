# 微信小程序RSS订阅器后端
本项目使用 .NET 框架和 MySQL 数据库，采用 SQLSugar ORM 框架进行数据库操作。

## 项目结构

```
 RSS.Repository         --
 RSS.RepositoryTests    --
 RSS.Util               --
 RSS.Web                -- 启动项目
 UpdateFeed             -- 刷新订阅源
 ```

## 开发环境

```
- .net core 3.1
- MySQL 8.0
- Visual Studio 2022

```



## 如何运行
克隆本项目到本地
```git clone https://github.com/fanfpy/RSSServer.git```

- 在 MySQL 中创建数据库，`sql-script.sql` 并将数据库连接字符串添加到 appsettings.json 文件中。

- 在 Visual Studio 中打开项目，并运行。

