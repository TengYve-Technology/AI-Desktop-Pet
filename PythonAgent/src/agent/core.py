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
    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.llm: Optional[BaseChatModel] = None
        self.memory = MemoryManager()
        self.screen_capture = ScreenCapture()
        self.process_monitor = ProcessMonitor()
        self.system_tools = SystemTools()
        self.file_tools = FileTools()
        self.is_initialized = False
        
        self._init_llm()

    def _init_llm(self):
        model_type = self.config.get("llm", {}).get("type", "ollama")
        model_config = self.config.get("llm", {}).get("config", {})
        
        fallback_models = ["chatglm", "deepseek", "qwen", "siliconflow", "openai"]
        
        try:
            self.llm = LLMFactory.create_llm(model_type, **model_config)
            self.is_initialized = True
            print(f"[PetAgent] LLM initialized: {model_type}")
            return
        except Exception as e:
            print(f"[PetAgent] Failed to initialize {model_type}: {e}")
        
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
        
        self.is_initialized = False
        print("[PetAgent] All LLM initialization attempts failed. Using mock mode.")

    def get_system_prompt(self) -> str:
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
回复不要太长，适合作为宠物对话。
"""
        return prompt.strip()

    def chat(self, user_input: str) -> str:
        if not self.is_initialized or self.llm is None:
            return "抱歉，我还没准备好呢，请稍后再试~"

        self.memory.add_human_message(user_input)
        
        messages = [SystemMessage(content=self.get_system_prompt())]
        messages.extend(self.memory.get_langchain_messages(limit=20))

        try:
            response = self.llm.invoke(messages)
            reply = response.content
            
            self.memory.add_ai_message(reply)
            self.memory.save_history()
            
            return reply
        except Exception as e:
            error_msg = f"聊天出错了: {str(e)}"
            print(f"[PetAgent] {error_msg}")
            return "哎呀，我好像出了点小问题，稍后再试吧~"

    def process_message(self, message_type: str, data: Dict[str, Any]) -> Dict[str, Any]:
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
        text = data.get("text", "")
        reply = self.chat(text)
        return {"success": True, "reply": reply}

    def _handle_system_info(self, data: Dict[str, Any]) -> Dict[str, Any]:
        info = self.process_monitor.get_system_info()
        return {"success": True, "data": info}

    def _handle_screenshot(self, data: Dict[str, Any]) -> Dict[str, Any]:
        filepath = self.screen_capture.save_screenshot()
        return {"success": True, "filepath": filepath}

    def _handle_process_list(self, data: Dict[str, Any]) -> Dict[str, Any]:
        count = data.get("count", 10)
        processes = self.process_monitor.get_top_processes(count)
        return {"success": True, "processes": processes}

    def _handle_tool_call(self, data: Dict[str, Any]) -> Dict[str, Any]:
        tool_name = data.get("tool")
        tool_args = data.get("args", {})

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
        key = data.get("key")
        value = data.get("value")
        
        if key and value is not None:
            self.memory.save_user_profile(key, value)
            return {"success": True}
        else:
            return {"success": False, "error": "Key and value are required"}

    def get_status(self) -> Dict[str, Any]:
        return {
            "initialized": self.is_initialized,
            "message_count": self.memory.get_message_count(),
            "llm_type": self.config.get("llm", {}).get("type", "unknown"),
        }