"""
memory_manager.py —— 记忆管理器

负责维护对话历史和用户画像，提供以下能力：
  1. 对话历史管理：添加/查询/清除消息，滑动窗口控制长度
  2. 用户画像管理：保存/读取用户偏好信息（姓名、喜好等）
  3. LangChain 消息格式转换：将内部历史转换为 LangChain 的 HumanMessage/AIMessage
  4. 持久化存储：对话历史按日期保存为 JSON，用户画像保存为独立 JSON

当前为简单实现，计划书中的长期记忆、记忆压缩、重要性评估等功能待后续实现。
"""
from typing import List, Dict, Optional, Any
from datetime import datetime
import json
import os
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage

class MemoryManager:
    """
    记忆管理器

    管理对话历史（短期记忆）和用户画像，支持持久化到磁盘。

    Attributes:
        memory_dir:          记忆文件的存储目录
        max_history:         对话历史的最大保留条数（滑动窗口）
        conversation_history: 当前会话的对话历史列表
        user_profile:        用户画像字典
    """

    def __init__(self, memory_dir: str = "data/memory", max_history: int = 50):
        """
        初始化记忆管理器

        Args:
            memory_dir:  记忆文件存储目录，不存在会自动创建
            max_history: 对话历史最大保留条数，超出后自动裁剪最早的消息
        """
        self.memory_dir = memory_dir
        self.max_history = max_history
        self.conversation_history: List[Dict[str, Any]] = []
        self.user_profile: Dict[str, Any] = {}

        os.makedirs(memory_dir, exist_ok=True)
        self._load_user_profile()

    def add_message(self, role: str, content: str, timestamp: Optional[datetime] = None):
        """
        添加一条消息到对话历史

        Args:
            role:      消息角色（"user" / "assistant" / "system"）
            content:   消息内容
            timestamp: 消息时间戳，默认为当前时间
        """
        if timestamp is None:
            timestamp = datetime.now()
        
        message = {
            "role": role,
            "content": content,
            "timestamp": timestamp.isoformat()
        }
        
        self.conversation_history.append(message)
        
        # 滑动窗口：超出最大条数时裁剪最早的消息
        if len(self.conversation_history) > self.max_history:
            self.conversation_history = self.conversation_history[-self.max_history:]

    def add_human_message(self, content: str):
        """添加用户消息到对话历史"""
        self.add_message("user", content)

    def add_ai_message(self, content: str):
        """添加 AI 回复到对话历史"""
        self.add_message("assistant", content)

    def get_history(self, limit: Optional[int] = None) -> List[Dict[str, Any]]:
        """
        获取对话历史

        Args:
            limit: 返回最近 N 条消息，None 表示返回全部

        Returns:
            对话历史列表，每条包含 role / content / timestamp
        """
        if limit is not None:
            return self.conversation_history[-limit:]
        return self.conversation_history

    def get_langchain_messages(self, limit: int = 20) -> List:
        """
        将对话历史转换为 LangChain 消息列表

        用于传给 LLM 的 invoke() 方法，仅包含 user → HumanMessage 和
        assistant → AIMessage，system 消息由 PetAgent 单独注入。

        Args:
            limit: 获取最近 N 条消息用于构造上下文

        Returns:
            LangChain Message 对象列表
        """
        messages = []
        history = self.get_history(limit)
        
        for msg in history:
            if msg["role"] == "user":
                messages.append(HumanMessage(content=msg["content"]))
            elif msg["role"] == "assistant":
                messages.append(AIMessage(content=msg["content"]))
        
        return messages

    def clear_history(self):
        """清空当前会话的对话历史"""
        self.conversation_history = []

    def save_user_profile(self, key: str, value: Any):
        """
        保存用户画像中的某个键值对

        Args:
            key:   画像键名（如 "name"、"hobby"）
            value: 画像值
        """
        self.user_profile[key] = value
        self._save_user_profile()

    def get_user_profile(self, key: Optional[str] = None) -> Any:
        """
        获取用户画像

        Args:
            key: 画像键名，None 表示返回完整画像

        Returns:
            对应键的值，或完整画像字典
        """
        if key is None:
            return self.user_profile
        return self.user_profile.get(key)

    def _load_user_profile(self):
        """从磁盘加载用户画像（user_profile.json）"""
        profile_path = os.path.join(self.memory_dir, "user_profile.json")
        if os.path.exists(profile_path):
            try:
                with open(profile_path, "r", encoding="utf-8") as f:
                    self.user_profile = json.load(f)
            except Exception:
                self.user_profile = {}

    def _save_user_profile(self):
        """将用户画像持久化到磁盘（user_profile.json）"""
        profile_path = os.path.join(self.memory_dir, "user_profile.json")
        try:
            with open(profile_path, "w", encoding="utf-8") as f:
                json.dump(self.user_profile, f, ensure_ascii=False, indent=2)
        except Exception:
            pass

    def save_history(self, filename: Optional[str] = None):
        """
        将对话历史持久化到磁盘

        Args:
            filename: 保存文件名，默认按当天日期命名（conversation_YYYYMMDD.json）
        """
        if filename is None:
            filename = f"conversation_{datetime.now().strftime('%Y%m%d')}.json"
        
        history_path = os.path.join(self.memory_dir, filename)
        try:
            with open(history_path, "w", encoding="utf-8") as f:
                json.dump(self.conversation_history, f, ensure_ascii=False, indent=2)
        except Exception:
            pass

    def load_history(self, filename: str):
        """
        从磁盘加载对话历史

        Args:
            filename: 历史文件名（如 conversation_20260713.json）
        """
        history_path = os.path.join(self.memory_dir, filename)
        if os.path.exists(history_path):
            try:
                with open(history_path, "r", encoding="utf-8") as f:
                    self.conversation_history = json.load(f)
            except Exception:
                pass

    def get_history_summary(self) -> str:
        """获取最近 5 条对话的摘要文本"""
        if len(self.conversation_history) == 0:
            return "No conversation history."
        
        recent = self.conversation_history[-5:]
        summary = "Recent conversation:\n"
        for msg in recent:
            summary += f"{msg['role']}: {msg['content'][:50]}...\n"
        
        return summary

    def get_message_count(self) -> int:
        """获取当前对话历史的消息总数"""
        return len(self.conversation_history)

    def get_last_message(self) -> Optional[Dict[str, Any]]:
        """获取最近一条消息，无消息时返回 None"""
        if self.conversation_history:
            return self.conversation_history[-1]
        return None