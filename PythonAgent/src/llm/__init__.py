"""
llm 包 —— 大语言模型接入层

提供 LLMFactory 工厂类，根据配置创建不同厂商的 LLM 实例。
所有模型统一适配为 LangChain 的 BaseChatModel 接口，
上层代码无需关心具体模型实现细节。

支持的模型厂商：openai / chatglm / ollama / deepseek / qwen / siliconflow
"""
from .factory import LLMFactory

__all__ = ["LLMFactory"]