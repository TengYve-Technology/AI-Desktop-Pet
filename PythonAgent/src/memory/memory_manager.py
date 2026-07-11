from typing import List, Dict, Optional, Any
from datetime import datetime
import json
import os
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage

class MemoryManager:
    def __init__(self, memory_dir: str = "data/memory", max_history: int = 50):
        self.memory_dir = memory_dir
        self.max_history = max_history
        self.conversation_history: List[Dict[str, Any]] = []
        self.user_profile: Dict[str, Any] = {}
        
        os.makedirs(memory_dir, exist_ok=True)
        self._load_user_profile()

    def add_message(self, role: str, content: str, timestamp: Optional[datetime] = None):
        if timestamp is None:
            timestamp = datetime.now()
        
        message = {
            "role": role,
            "content": content,
            "timestamp": timestamp.isoformat()
        }
        
        self.conversation_history.append(message)
        
        if len(self.conversation_history) > self.max_history:
            self.conversation_history = self.conversation_history[-self.max_history:]

    def add_human_message(self, content: str):
        self.add_message("user", content)

    def add_ai_message(self, content: str):
        self.add_message("assistant", content)

    def get_history(self, limit: Optional[int] = None) -> List[Dict[str, Any]]:
        if limit is not None:
            return self.conversation_history[-limit:]
        return self.conversation_history

    def get_langchain_messages(self, limit: int = 20) -> List:
        messages = []
        history = self.get_history(limit)
        
        for msg in history:
            if msg["role"] == "user":
                messages.append(HumanMessage(content=msg["content"]))
            elif msg["role"] == "assistant":
                messages.append(AIMessage(content=msg["content"]))
        
        return messages

    def clear_history(self):
        self.conversation_history = []

    def save_user_profile(self, key: str, value: Any):
        self.user_profile[key] = value
        self._save_user_profile()

    def get_user_profile(self, key: Optional[str] = None) -> Any:
        if key is None:
            return self.user_profile
        return self.user_profile.get(key)

    def _load_user_profile(self):
        profile_path = os.path.join(self.memory_dir, "user_profile.json")
        if os.path.exists(profile_path):
            try:
                with open(profile_path, "r", encoding="utf-8") as f:
                    self.user_profile = json.load(f)
            except Exception:
                self.user_profile = {}

    def _save_user_profile(self):
        profile_path = os.path.join(self.memory_dir, "user_profile.json")
        try:
            with open(profile_path, "w", encoding="utf-8") as f:
                json.dump(self.user_profile, f, ensure_ascii=False, indent=2)
        except Exception:
            pass

    def save_history(self, filename: Optional[str] = None):
        if filename is None:
            filename = f"conversation_{datetime.now().strftime('%Y%m%d')}.json"
        
        history_path = os.path.join(self.memory_dir, filename)
        try:
            with open(history_path, "w", encoding="utf-8") as f:
                json.dump(self.conversation_history, f, ensure_ascii=False, indent=2)
        except Exception:
            pass

    def load_history(self, filename: str):
        history_path = os.path.join(self.memory_dir, filename)
        if os.path.exists(history_path):
            try:
                with open(history_path, "r", encoding="utf-8") as f:
                    self.conversation_history = json.load(f)
            except Exception:
                pass

    def get_history_summary(self) -> str:
        if len(self.conversation_history) == 0:
            return "No conversation history."
        
        recent = self.conversation_history[-5:]
        summary = "Recent conversation:\n"
        for msg in recent:
            summary += f"{msg['role']}: {msg['content'][:50]}...\n"
        
        return summary

    def get_message_count(self) -> int:
        return len(self.conversation_history)

    def get_last_message(self) -> Optional[Dict[str, Any]]:
        if self.conversation_history:
            return self.conversation_history[-1]
        return None