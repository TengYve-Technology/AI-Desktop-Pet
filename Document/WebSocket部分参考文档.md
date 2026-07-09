# WebSocket 通信模块开发文档

---

## 1. 模块概述

本文档描述 AI 桌面宠物项目中 Unity 客户端与 Python 服务端之间的 WebSocket 通信模块设计与实现。

通信模块承担以下职责：
- 建立和维护 Unity 与 Python 之间的 WebSocket 连接
- 定义统一的消息格式
- 实现请求-响应匹配机制
- 处理连接异常与自动重连
- 提供心跳保活功能

---

## 2. 技术选型

| 组件 | 技术方案 | 说明 |
|------|----------|------|
| Unity WebSocket 客户端 | `nl.elraccoone.web-sockets` | 通过 OpenUPM 安装，版本 2.0.2 |
| Python WebSocket 服务端 | `websockets` | Python 异步 WebSocket 库 |
| 消息序列化 | JSON | 双方统一使用 UTF-8 编码 |
| Python 异步框架 | asyncio | 原生异步支持 |

---

## 3. 消息协议

### 3.1 基础消息结构

所有消息均为 JSON 格式，包含以下字段：

```typescript
{
  id: string;           // 消息唯一标识，UUID 格式
  type: string;         // 消息类型
  protocol: string;     // 协议版本，固定 "v1"
  timestamp: string;    // ISO 8601 时间戳
  data?: object;        // 消息数据（可选）
  in_reply_to?: string; // 响应时关联的请求 ID
  error?: string;       // 错误时包含错误信息
}
```

### 3.2 消息类型定义

| type 值 | 方向 | 说明 |
|---------|------|------|
| `hello` | C → S | 连接建立后的握手消息 |
| `chat` | C → S | 用户聊天消息 |
| `ping` | C → S | 心跳检测 |
| `response` | S → C | 对请求的响应 |
| `error` | S → C | 错误响应 |
| `notification` | S → C | 服务器主动推送（预留） |

### 3.3 请求-响应匹配

- 请求消息包含 `id` 字段
- 响应消息包含 `in_reply_to` 字段，值与对应的请求 `id` 一致
- 客户端维护 `_pendingRequests` 字典，存储待回应的请求及其回调

---

## 4. Unity 客户端实现

### 4.1 目录结构

```
Assets/_Project/Scripts/Communication/
├── WebSocketManager.cs              # 核心管理器（单例）
├── MessageRouter.cs                  # 消息路由器
├── MessageHandlerBase.cs             # 处理器基类
├── Handlers/
│   ├── ResponseHandler.cs            # 处理 response 类型消息
│   ├── ErrorHandler.cs               # 处理 error 类型消息
│   └── NotificationHandler.cs        # 处理通知类消息
├── Models/
│   ├── WebSocketMessage.cs           # 消息数据模型
│   └── RequestContext.cs             # 请求上下文
└── Utils/
    └── WebSocketLogger.cs            # 日志工具
```

### 4.2 核心类说明

#### WebSocketManager

单例组件，挂载于场景中的持久化 GameObject。

**公共方法：**

| 方法 | 参数 | 说明 |
|------|------|------|
| `Connect()` | 无 | 建立 WebSocket 连接 |
| `SendMessage(msg)` | `WebSocketMessage` | 发送无需回应的消息 |
| `SendRequest(msg, onSuccess, onError, timeout)` | `WebSocketMessage`, `Action<WebSocketMessage>`, `Action<string>`, `int` | 发送需要回应的请求 |
| `IsConnected` | 无（属性） | 返回当前连接状态 |

**事件：**

| 事件 | 参数 | 说明 |
|------|------|------|
| `OnConnected` | 无 | 连接建立时触发 |
| `OnDisconnected` | `string` | 连接断开时触发，参数为断开原因 |
| `OnError` | `string` | 发生错误时触发 |
| `OnNotification` | `WebSocketMessage` | 收到通知消息时触发 |

**内部机制：**

- **断线重连**：使用指数退避策略（1s, 2s, 4s, 8s...），最大间隔 30 秒，最多尝试 10 次
- **心跳保活**：连接建立后每 30 秒发送一次 `ping` 消息
- **请求超时**：默认 10 秒超时，超时后触发 `OnTimeout` 回调并移除请求
- **离线队列**：断线期间发送的消息存入队列，重连成功后按顺序补发

#### MessageRouter

维护 `MessageHandlerBase` 列表，根据消息类型分发到对应处理器。

#### MessageHandlerBase

抽象基类，所有处理器继承此类：

```csharp
public abstract class MessageHandlerBase
{
    public abstract bool CanHandle(WebSocketMessage message);
    public abstract void Handle(WebSocketMessage message);
}
```

### 4.3 使用示例

**发送请求并处理响应：**

```csharp
var msg = new WebSocketMessage
{
    type = "chat",
    data = new ChatData { text = "Hello" }
};

WebSocketManager.Instance.SendRequest(
    msg,
    onSuccess: (response) => {
        Debug.Log($"收到回复: {response.data}");
    },
    onError: (error) => {
        Debug.LogError($"请求失败: {error}");
    }
);
```

**订阅通知：**

```csharp
WebSocketManager.Instance.OnNotification += (msg) => {
    Debug.Log($"收到通知: {msg.type}");
};
```

---

## 5. Python 服务端实现

### 5.1 目录结构

```
PythonAgent/src/
├── server.py                    # 主入口
├── models/
│   └── message.py               # 消息模型
├── handlers/
│   ├── base.py                  # 处理器基类
│   ├── hello_handler.py         # 握手处理器
│   ├── chat_handler.py          # 聊天处理器（含 LLM 调用）
│   └── ping_handler.py          # 心跳处理器
├── llm/
│   ├── client.py                # LLM API 客户端
│   └── manager.py               # LLM 管理器（多模型配置）
└── utils/
    └── logger.py                # 日志工具
```

### 5.2 核心类说明

#### Message

消息数据类，提供序列化/反序列化方法。

```python
@dataclass
class Message:
    id: str
    type: str
    protocol: str = "v1"
    timestamp: str = None
    data: Optional[Dict] = None

    def to_response(self, data: Any) -> dict:
        """生成响应消息"""
        
    def to_error(self, error: str) -> dict:
        """生成错误消息"""
```

#### BaseHandler

处理器抽象基类：

```python
class BaseHandler(ABC):
    @abstractmethod
    def can_handle(self, msg: Message) -> bool:
        pass
    
    @abstractmethod
    def handle(self, msg: Message) -> dict:
        pass
```

#### LLMClient

封装 OpenAI 兼容 API 调用，支持流式输出。

```python
class LLMClient:
    async def chat(
        self,
        messages: list[dict],
        temperature: float = 0.7,
        max_tokens: int = 512,
        stream: bool = False
    ) -> str | AsyncGenerator
```

### 5.3 配置方式

通过 `.env` 文件配置 LLM 参数：

```env
LLM_API_KEY=your_api_key
LLM_BASE_URL=https://api.openai.com/v1
LLM_MODEL=gpt-3.5-turbo

# 本地备用模型
LLM_FALLBACK_MODEL=qwen2.5:7b
LLM_FALLBACK_BASE_URL=http://localhost:11434/v1
LLM_FALLBACK_API_KEY=ollama
```

---

## 6. 通信流程

### 6.1 连接建立流程

```
Unity                          Python
  |                              |
  |---- Connect() --------------->|
  |                              | (WebSocket 握手)
  |<--- OnConnected -------------|
  |                              |
  |---- hello message ---------->|
  |                              | (返回 hello response)
  |<--- hello response ----------|
```

### 6.2 聊天消息流程

```
Unity                          Python
  |                              |
  |---- chat message ----------->|
  |                              | (LLMManager 调用 LLM API)
  |                              | (LLMClient.chat() 请求外部 API)
  |                              | (等待响应)
  |<--- response message --------|
```

### 6.3 断线重连流程

```
Unity                          Python
  |                              |
  | (连接中断)                    | (连接中断)
  |                              |
  | (开始重连计时)               |
  | (等待 1s)                    |
  |---- Connect() --------------->|
  | (连接失败)                    |
  | (等待 2s)                    |
  |---- Connect() --------------->|
  | (连接成功)                    |
  |---- hello message ----------->|
  |<--- hello response -----------|
  | (补发离线消息)               |
```

---

## 7. 错误处理

### 7.1 错误类型

| 错误场景 | 处理方式 |
|----------|----------|
| 连接失败 | 触发重连流程，最多 10 次 |
| 请求超时 | 触发 `OnTimeout` 回调，移除请求 |
| 服务器返回 error | 触发 `OnError` 回调 |
| 消息解析失败 | 记录日志，丢弃该消息 |
| LLM API 调用失败 | 返回 error 消息给 Unity |

### 7.2 日志级别

- **Info**：连接状态、消息收发
- **Warning**：孤立响应、重连尝试
- **Error**：连接错误、API 错误

---

## 8. 依赖安装

### Unity 端

通过 OpenUPM 安装：

```
openupm add nl.elraccoone.web-sockets
```

或在 `manifest.json` 中添加：

```json
"nl.elraccoone.web-sockets": "2.0.2"
```

### Python 端

```bash
pip install websockets openai httpx python-dotenv
```

---

## 9. 版本记录

| 日期 | 版本 | 变更内容 |
|------|------|----------|
| 2026-07-09 | v1.0 | 初始版本，实现基础通信、消息路由、重连机制、LLM 集成 |

---

## 10. 文件清单

### Unity 端文件

| 文件路径 | 行数 | 职责 |
|----------|------|------|
| `Communication/WebSocketManager.cs` | 280 | 核心连接管理 |
| `Communication/MessageRouter.cs` | 30 | 消息路由 |
| `Communication/MessageHandlerBase.cs` | 10 | 处理器基类 |
| `Communication/Handlers/ResponseHandler.cs` | 35 | 响应处理 |
| `Communication/Handlers/ErrorHandler.cs` | 35 | 错误处理 |
| `Communication/Handlers/NotificationHandler.cs` | 30 | 通知处理 |
| `Communication/Models/WebSocketMessage.cs` | 30 | 消息模型 |
| `Communication/Models/RequestContext.cs` | 25 | 请求上下文 |
| `Communication/Utils/WebSocketLogger.cs` | 30 | 日志工具 |

### Python 端文件

| 文件路径 | 行数 | 职责 |
|----------|------|------|
| `src/server.py` | 40 | 主入口 |
| `src/models/message.py` | 50 | 消息模型 |
| `src/handlers/base.py` | 15 | 处理器基类 |
| `src/handlers/hello_handler.py` | 20 | 握手处理 |
| `src/handlers/chat_handler.py` | 50 | 聊天处理 + LLM |
| `src/handlers/ping_handler.py` | 20 | 心跳处理 |
| `src/llm/client.py` | 60 | LLM API 客户端 |
| `src/llm/manager.py` | 50 | LLM 管理器 |