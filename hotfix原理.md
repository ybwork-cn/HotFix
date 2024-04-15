
#### 前置知识
- `Expression.Lamda`和`IL.Emit`都无法在IL2CPP环境下使用
- 思路1 : 能使用的仅剩`反射`，所以，维护一个自己的字节码虚拟机是必要的
- 思路2 : 预编译热更代码(C#转C++，然后编译，这个会很难)，然后提前制作一个C++dll，专门用于执行预编译的机器码

#### 思路1核心思路
- 1. 维护一个热更表，用于记录哪些类型的哪些方法要使用热更代码
- 2. 同样也允许C#直接调用热更脚本
- 3. 使用`BuildPipeline`进行项目打包
  - 在打包流程中加入dll分析
    - C#主dll每个`方法`、每个`属性`，都嵌入一条语句用于判断是否执行热更代码
  - 小版本发布时(热更新)，将所有热更代码转移为字节码（小版本发布时尝试不发布，而是仅仅编译dll并分析字节码）
  - 大版本发布前(冷更新)，先删除所有的热更标记，再进行打包
- 4. 如果要执行热更代码，则开辟一个虚拟机，执行相关代码
- 5. 尝试删除脚本，直接hotfix修改C#源码，然后用Attribute标记被修改的部分和新增的部分，在编译后将此部分源码生成的IL指令转译为自己的字节码
- 6. 每次大的项目发布(冷更新)，删除所有热更标记

#### 思路1数据结构
- 1. 方法`.method`
  - `.custom` Attribute[]
  - `.modifier` public|privite|protected,instance|static
  - `.name` string
  - `.params` Type[]
  - `.return` Type
  - `.locals` Type[]
  - `.maxstack` int
  - `.body` OpCodes[]
- 2. 字段`.field`
  - `.custom` Attribute[]
  - `.modifier` public|privite|protected,instance|static
  - `.type` Type
  - `.name` string
- 3. 属性`.property`
  - `.custom` Attribute[] 这个是针对于整个属性的，打在单个访问符的属性在编译时会被打到访问符对应的隐藏函数上
  - `.modifier` public|privite|protected,instance|static
  - `.type` Type
  - `.name` string
  - `.get` `.method`
  - `.set` `.method`
- 4. 类`.class`
  - `.custom` Attribute[]
  - `.name` string
  - `.baseType` Type
  - `.field`[]
  - `.property`[]
  - `.method`[]

#### Unity提供的API
- 1. 如果要修改Unity的LogException的StackTrace，可以继承重写Exception.StackTrace
- 2. 如果要删除Unity的Log的StackTrace，可以调用Application.SetStackTraceLogType
