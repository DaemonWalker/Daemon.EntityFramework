# 没有说明的说明文档
## 这玩意是干啥的
跟名字差不多，一个比较基本的ORM框架？
## 用途
单纯是个人摸鱼无聊的产物
## 各种类的功能
* DBContext：数据库上下文？
* DBTable，DBView：表和视图
* DataBase：数据库底层的各种对象Connection、Command、Transaction、DataReader
* DataOperator：执行sql部分，返回返回行数或者实体、实体List
* DEFSettings：框架的设置，比如反射什么DataBase，什么DataOperator的。。。
* EntityDBConvert：将lambda表达式翻译为sql的部分
* ExpressionAnalyze：表达式解析
* Query、QueryProvider：分析Linq用
* EntityTypeAttribute(v0.3.3新增)：表示Entity对象是表还是视图，不加默认为表
* PrimaryKeyAttribute：Entity中有该标记的实体为主键。**暂时不支持复合主键，没测试过:(**
* EntityEntry：用于标记对数据的操作，增删改查什么的
## 各个文件夹的功能
* SqlFormatter：格式化Sql用的，这个是抄NHibernate的
* Demo：主要是抽象类的实现，完全没有任何重写
* AbstractClass：所有的抽象类都在这个文件夹里面
* Attribute：看名字就明白了吧
* Utils：工具类
## 怎么用
1. 有两个东西是必须要继承然后重写方法的
   1. `Daemon.EntityFramework.Core.AbstractClasses.DataBase`：这个所有方法都要重写。因为各个数据库的对象，对象生成的方法都不一样(比如样例的Sqlite，谁能想到这么费劲。。。)所以我提供默认实现没有太大的意义
   2. `Daemon.EntityFramework.Core.AbstractClasses.EntityDBConvert`：这里面有个Insert方法，我在尽量简化这个方法的重写，但是因为每种数据库获取ID的方法都不一样，然后是先获取ID(Oracle)还是插入完再说(MSSql,Sqlite,Mysql)这个之后的版本再说。。。
2. 随意创建一个类，继承`Daemon.EntityFramework.Core.DbContext`类，然后在构造函数把刚刚继承的两个类加进去，以Test里面的SqliteDbContext举例：
```
    public SqliteDbContext() : base(
        new DefSettings()
        {
            DataBaseType = typeof(SqliteDataBase),
            EntityDBConvertType = typeof(SqliteEntityDBConvert),
            OutputSql = true
        }){ }
```
3. 创建各个表、视图的属性，可以无脑用DBTable，但是记得在实体上面加标签。
```
    public DBTable<SCORE> Score { get; set; }
    public DBTable<CLASS> Class { get; set; }
    public DBTable<STUDENT> Student { get; set; }
    public DBTable<SUBJECT> Subject { get; set; }
    public DBTable<V_STATS> VStats { get; set; }
```
