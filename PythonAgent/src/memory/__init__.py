"""
memory 包 —— 记忆管理系统

管理宠物的对话历史和用户画像，支持持久化存储。
包括：
  - 短期对话记忆（滑动窗口，默认保留最近 50 条）
  - 用户画像（键值对，持久化为 JSON）
  - 对话历史持久化（按日期保存为 JSON 文件）
"""
from .memory_manager import MemoryManager

__all__ = ["MemoryManager"]