"""
tools 包 —— 工具集

提供宠物可调用的系统级和文件级工具：
  - SystemTools: 系统命令执行、文件打开、环境变量管理、目录操作等
  - FileTools:   文件读写（文本/JSON）、列举、重命名、复制、删除等

这些工具通过 PetAgent._handle_tool_call() 被间接调用，
未来接入 LangChain Agent 后将由 LLM 自主选择调用。
"""
from .system_tools import SystemTools
from .file_tools import FileTools

__all__ = ["SystemTools", "FileTools"]