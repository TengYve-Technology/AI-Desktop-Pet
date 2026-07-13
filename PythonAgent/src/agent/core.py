"""
core.py —— 宠物 AI Agent 核心

PetAgent 是整个 Python 后端的中枢，负责：
  1. 初始化并管理 LLM（支持多模型降级）
  2. 处理各类消息（chat、system_info、screenshot、tool_call 等）
  3. 维护对话记忆和用户画像
  4. 调用工具（系统命令、文件操作、屏幕截图等）
  5. 生成系统提示词（注入角色设定和当前系统状态）
"""

from typing import Optional, Dict, Any, List
from langchain_core.messages import SystemMessage
from llm.factory import LLMFactory
from memory.memory_manager import MemoryManager
from perception.screen_capture import ScreenCapture
from perception.process_monitor import ProcessMonitor
from tools.system_tools import SystemTools
from tools.file_tools import FileTools
from langchain_core.language_models import BaseChatModel

class PetAgent:
    """
    宠物 AI Agent 核心

    统筹 LLM 对话、记忆、感知和工具子系统，
    对外暴露 process_message() 作为消息处理的统一入口。
    """

    def __init__(self, config: Dict[str, Any]):
        """
        初始化 Agent 及所有子系统

        Args:
            config: 从 config.yaml 加载的完整配置字典
        """
        self.config = config
        self.llm: Optional[BaseChatModel] = None
        self.memory = MemoryManager()          # 对话记忆管理器
        self.screen_capture = ScreenCapture()  # 屏幕截图工具
        self.process_monitor = ProcessMonitor()  # 进程监控器
        self.system_tools = SystemTools()      # 系统工具集（命令执行、文件打开等）
        self.file_tools = FileTools()          # 文件工具集（读写、列举等）
        self.is_initialized = False

        self._init_llm()

    def _init_llm(self):
        """
        初始化 LLM，支持降级链

        先尝试配置中指定的主模型，失败后依次尝试：
        chatglm → deepseek → qwen → siliconflow → openai
        全部失败则进入 mock 模式（is_initialized=False）。
        """
        model_type = self.config.get("llm", {}).get("type", "ollama")
        model_config = self.config.get("llm", {}).get("config", {})

        # 降级模型列表：主模型失败后按此顺序依次尝试
        fallback_models = ["chatglm", "deepseek", "qwen", "siliconflow", "openai"]
        
        try:
            # 尝试创建主模型
            self.llm = LLMFactory.create_llm(model_type, **model_config)
            self.is_initialized = True
            print(f"[PetAgent] LLM initialized: {model_type}")
            return
        except Exception as e:
            print(f"[PetAgent] Failed to initialize {model_type}: {e}")

        # 主模型失败，依次尝试降级模型
        for fallback_type in fallback_models:
            if fallback_type == model_type:
                continue
            
            try:
                self.llm = LLMFactory.create_llm(fallback_type, **model_config)
                self.is_initialized = True
                print(f"[PetAgent] Fallback to {fallback_type} succeeded")
                return
            except Exception as e:
                print(f"[PetAgent] Fallback {fallback_type} failed: {e}")
        
        # 全部模型初始化失败，进入 mock 模式
        self.is_initialized = False
        print("[PetAgent] All LLM initialization attempts failed. Using mock mode.")

    def get_system_prompt(self) -> str:
        """
        生成系统提示词

        将宠物角色设定和当前系统状态（CPU/内存/进程数）注入到 LLM 的 system prompt 中，
        让 LLM 了解自身角色和运行环境，从而生成符合设定的回复。
        """
        system_info = self.process_monitor.get_system_info()
        prompt = f"""
你是一个可爱的AI桌面宠物，名字叫"小宠"。

角色设定：
- 你是一个活泼可爱的虚拟宠物，生活在用户的桌面上
- 你的性格友好、热情，喜欢和用户聊天
- 你可以感知用户的电脑状态（CPU、内存、进程等）
- 你可以执行一些系统操作

当前系统信息：
- CPU使用率: {system_info['cpu_percent']}%
- 内存使用率: {system_info['memory_percent']}%
- 运行进程数: {system_info['cpu_count']}

可用工具：
1. screen_capture - 截图当前屏幕
2. get_process_info - 获取进程信息
3. run_command - 执行系统命令
4. read_file - 读取文件内容
5. write_file - 写入文件

请用简短、友好、可爱的语气回复用户。
根据用户的问题确定回答的长短以及专业性或者趣味性，适合作为宠物对话。
"""
        return prompt.strip()

    def chat(self, user_input: str) -> str:
        """
        与 LLM 进行一轮对话

        Args:
            user_input: 用户输入的文本

        Returns:
            LLM 生成的回复文本；如果 LLM 未初始化或调用失败，返回友好提示
        """
        if not self.is_initialized or self.llm is None:
            return "抱歉，我还没准备好呢，请稍后再试~"

        self.memory.add_human_message(user_input)

        # 构建消息列表：系统提示词 + 近期对话历史
        messages = [SystemMessage(content=self.get_system_prompt())]
        messages.extend(self.memory.get_langchain_messages(limit=20))

        try:
            response = self.llm.invoke(messages)
            reply = response.content

            # 将 AI 回复写入记忆并持久化
            self.memory.add_ai_message(reply)
            self.memory.save_history()
            
            return reply
        except Exception as e:
            error_msg = f"聊天出错了: {str(e)}"
            print(f"[PetAgent] {error_msg}")
            return "哎呀，我好像出了点小问题，稍后再试吧~"

    def process_message(self, message_type: str, data: Dict[str, Any]) -> Dict[str, Any]:
        """
        消息处理统一入口

        根据 message_type 分发到对应的内部处理方法，
        所有处理方法返回统一格式 {"success": bool, ...}。

        Args:
            message_type: 消息类型（chat / system_info / screenshot / process_list / tool_call / memory_query / profile_update）
            data: 消息携带的数据字典

        Returns:
            处理结果字典，至少包含 success 字段
        """
        # 消息类型 → 处理方法的映射表
        handlers = {
            "chat": self._handle_chat,
            "system_info": self._handle_system_info,
            "screenshot": self._handle_screenshot,
            "process_list": self._handle_process_list,
            "tool_call": self._handle_tool_call,
            "memory_query": self._handle_memory_query,
            "profile_update": self._handle_profile_update,
        }

        handler = handlers.get(message_type)
        if handler:
            try:
                return handler(data)
            except Exception as e:
                return {"success": False, "error": str(e)}
        else:
            return {"success": False, "error": f"Unknown message type: {message_type}"}

    def _handle_chat(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """处理聊天消息，调用 LLM 生成回复"""
        text = data.get("text", "")
        reply = self.chat(text)
        return {"success": True, "reply": reply}

    def _handle_system_info(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """获取当前系统信息（CPU、内存、磁盘等）"""
        info = self.process_monitor.get_system_info()
        return {"success": True, "data": info}

    def _handle_screenshot(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """截取当前屏幕并保存，返回文件路径"""
        filepath = self.screen_capture.save_screenshot()
        return {"success": True, "filepath": filepath}

    def _handle_process_list(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """获取 CPU 占用最高的 N 个进程列表"""
        count = data.get("count", 10)
        processes = self.process_monitor.get_top_processes(count)
        return {"success": True, "processes": processes}

    def _handle_tool_call(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """
        执行工具调用

        根据工具名称和参数，调用对应的系统/文件工具并返回结果。
        支持的工具：run_command / read_file / write_file / list_files / open_file / get_system_info / capture_screen
        """
        tool_name = data.get("tool")
        tool_args = data.get("args", {})

        # 工具名称 → 执行函数的映射表
        tools = {
            "run_command": lambda args: self.system_tools.run_command(**args),
            "read_file": lambda args: self.file_tools.read_file(**args),
            "write_file": lambda args: self.file_tools.write_file(**args),
            "list_files": lambda args: self.file_tools.list_files(**args),
            "open_file": lambda args: {"success": self.system_tools.open_file(**args)},
            "get_system_info": lambda args: {"success": True, "data": self.process_monitor.get_system_info()},
            "capture_screen": lambda args: {"success": True, "filepath": self.screen_capture.save_screenshot()},
        }

        if tool_name in tools:
            try:
                result = tools[tool_name](tool_args)
                return {"success": True, "result": result}
            except Exception as e:
                return {"success": False, "error": str(e)}
        else:
            return {"success": False, "error": f"Unknown tool: {tool_name}"}

    def _handle_memory_query(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """
        查询记忆数据

        支持的查询类型：
          - history: 获取最近的对话历史
          - summary: 获取对话摘要
          - profile: 获取用户画像中的某个键值
          - count: 获取消息总数
        """
        query_type = data.get("type", "history")
        
        if query_type == "history":
            limit = data.get("limit", 10)
            return {"success": True, "history": self.memory.get_history(limit)}
        elif query_type == "summary":
            return {"success": True, "summary": self.memory.get_history_summary()}
        elif query_type == "profile":
            key = data.get("key")
            return {"success": True, "profile": self.memory.get_user_profile(key)}
        elif query_type == "count":
            return {"success": True, "count": self.memory.get_message_count()}
        else:
            return {"success": False, "error": f"Unknown query type: {query_type}"}

    def _handle_profile_update(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """更新用户画像中的某个键值对"""
        key = data.get("key")
        value = data.get("value")
        
        if key and value is not None:
            self.memory.save_user_profile(key, value)
            return {"success": True}
        else:
            return {"success": False, "error": "Key and value are required"}

    def get_status(self) -> Dict[str, Any]:
        """获取 Agent 当前状态（初始化状态、消息数、LLM 类型）"""
        return {
            "initialized": self.is_initialized,
            "message_count": self.memory.get_message_count(),
            "llm_type": self.config.get("llm", {}).get("type", "unknown"),
        }