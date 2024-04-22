#### 2024.04.19
- 支持自动根据函数全名判断是否有热更，并自动调用对应的热更数据

#### 2024.04.20
- 更换热更数据保存位置
- 嵌入的脚本支持传入当前参数
- 判断文件是否存在的逻辑改为:增加文件目录，读取该目录以查询该文件是否存在
- 读取文件的逻辑改为:通过UnityWebRequest读取(异步读取)
- 修复bug：虚拟机调用具有返回值的函数时，未将返回值存至栈
- 修复bug：虚拟机执行时，获取参数值时的序号
- 修复bug：虚拟机执行时，指令的偏移量
- 增加对sbyte类型的支持
- 增加更多IL指令(值比较大小)
- 增加更多IL指令(循环)
- 增加更多IL指令(位运算)
- 方法内调用其他方法时，自动判断是否为热更方法

#### 注
- 方法内支持调用或定义表达式
  - 支持捕获变量
  - 支持不捕获变量
  - 一个函数只要标记了Hotfix，则将其中的所有表达式转码为热更IL码(一个独立的Function)，不关心该表达式是否发生了变化
- `?` 方法支持异步、协程
- 位运算应支持所有整数类型，参考https://learn.microsoft.com/zh-cn/dotnet/api/system.reflection.emit.opcodes.shr?view=net-8.0&viewFallbackFrom=net-4.8
- 支持sizeof
- 增加对更多IL指令的支持
- 支持现有类型的热更
  - 允许增删方法
  - 允许增删字段
  - 允许增删改属性
  - 热更代码中，如果要代用一个类型的对象的成员(`Call`等指令)，则在线判断是否存在该成员
    - 如果该成员存在则正常调用
    - 如果该成员不存在，则用热更的方式调用
  - 类型成员的热更方式
    - 只记录增加和修改的成员，被删除的成员因为不会访问到，所以不需要记录
    - 方法:直接将热更方法通过虚拟机加载并Invoke
    - 字段:每个类型增加一个字段`public Dictionary<string,object> __HotfixFields__;`，用于存储所有的字段
    - 属性:每个类型增加一个字段`public Dictionary<string,HotfixProperty> __HotfixProperties__;`，用于存储所有的字段
      - `HotfixProperty`需要存储该属性的已被修改的访问符，以及该访问符所绑定的方法
- 支持新类型的创建
- class和struct实现方式不同
- `HotfixAttribute`自动实现`MethodImpl(MethodImplOptions.NoInlining)`
