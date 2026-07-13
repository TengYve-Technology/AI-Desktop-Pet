"""
factory.py —— LLM 工厂

根据 model_type 字符串创建对应的 LangChain ChatModel 实例。
所有模型统一适配为 BaseChatModel 接口，上层代码无需关心具体厂商差异。

设计思路：
  - 云端模型（openai / chatglm / deepseek / qwen / siliconflow）：
    统一使用 ChatOpenAI，通过 base_url + api_key 切换不同厂商的 OpenAI 兼容端点
  - 本地模型（ollama）：
    使用 ChatOllama，连接本地 Ollama 服务

配置优先级：
  1. 代码中显式传入的参数
  2. .env 中的环境变量
  3. 各厂商的默认值
"""
from typing import Optional, Dict, Any
from langchain_openai import ChatOpenAI
from langchain_ollama import ChatOllama
from langchain_core.language_models import BaseChatModel
import os

class LLMFactory:
    """
    LLM 工厂类

    提供统一的 create_llm() 方法，根据模型类型创建对应的 LLM 实例。
    也提供 validate_config() 用于检查配置完整性，以及 get_supported_models() 查询支持的模型列表。
    """

    @staticmethod
    def create_llm(model_type: str, **kwargs) -> BaseChatModel:
        """
        根据模型类型创建 LLM 实例

        Args:
            model_type: 模型类型（openai / chatglm / ollama / deepseek / qwen / siliconflow）
            **kwargs: 传递给对应模型构造器的参数（api_key / base_url / model_name / temperature 等）

        Returns:
            适配为 BaseChatModel 接口的 LLM 实例

        Raises:
            ValueError: 不支持的模型类型
        """
        model_type = model_type.lower().strip()

        if model_type == "openai":
            return LLMFactory._create_openai(**kwargs)
        elif model_type == "chatglm":
            return LLMFactory._create_chatglm(**kwargs)
        elif model_type == "ollama":
            return LLMFactory._create_ollama(**kwargs)
        elif model_type == "deepseek":
            return LLMFactory._create_deepseek(**kwargs)
        elif model_type == "qwen":
            return LLMFactory._create_qwen(**kwargs)
        elif model_type == "siliconflow":
            return LLMFactory._create_siliconflow(**kwargs)
        else:
            raise ValueError(f"Unsupported model type: {model_type}")

    @staticmethod
    def _create_openai(**kwargs) -> ChatOpenAI:
        """创建 OpenAI 模型实例（也作为其他云端模型的基类）"""
        api_key = kwargs.get("api_key") or os.getenv("OPENAI_API_KEY")
        model_name = kwargs.get("model_name") or kwargs.get("model") or "gpt-4o-mini"
        base_url = kwargs.get("base_url") or os.getenv("OPENAI_BASE_URL")
        
        params = {
            "model_name": model_name,
            "api_key": api_key,
            "temperature": kwargs.get("temperature", 0.7),
            "max_tokens": kwargs.get("max_tokens", 4096),
        }
        
        if base_url:
            params["openai_api_base"] = base_url
        
        return ChatOpenAI(**params)

    @staticmethod
    def _create_chatglm(**kwargs) -> ChatOpenAI:
        """创建智谱 ChatGLM 模型实例（通过 OpenAI 兼容端点）"""
        api_key = kwargs.get("api_key") or os.getenv("CHATGLM_API_KEY")
        model_name = kwargs.get("model_name") or kwargs.get("model") or "glm-4"
        base_url = kwargs.get("base_url") or os.getenv("CHATGLM_BASE_URL") or "https://open.bigmodel.cn/api/paas/v4/"
        
        return ChatOpenAI(
            model_name=model_name,
            api_key=api_key,
            openai_api_base=base_url,
            temperature=kwargs.get("temperature", 0.7),
            max_tokens=kwargs.get("max_tokens", 4096),
        )

    @staticmethod
    def _create_ollama(**kwargs) -> ChatOllama:
        """创建 Ollama 本地模型实例（连接本地 Ollama 服务）"""
        model_name = kwargs.get("model_name") or kwargs.get("model") or "qwen2:7b"
        base_url = kwargs.get("base_url") or os.getenv("OLLAMA_BASE_URL") or "http://localhost:11434"
        
        return ChatOllama(
            model=model_name,
            base_url=base_url,
            temperature=kwargs.get("temperature", 0.7),
            num_ctx=kwargs.get("num_ctx", 8192),
        )

    @staticmethod
    def _create_deepseek(**kwargs) -> ChatOpenAI:
        """创建 DeepSeek 模型实例（通过 OpenAI 兼容端点）"""
        api_key = kwargs.get("api_key") or os.getenv("DEEPSEEK_API_KEY")
        model_name = kwargs.get("model_name") or kwargs.get("model") or "deepseek-chat"
        base_url = kwargs.get("base_url") or os.getenv("DEEPSEEK_BASE_URL") or "https://api.deepseek.com/v1"
        
        return ChatOpenAI(
            model_name=model_name,
            api_key=api_key,
            openai_api_base=base_url,
            temperature=kwargs.get("temperature", 0.7),
            max_tokens=kwargs.get("max_tokens", 4096),
        )

    @staticmethod
    def _create_qwen(**kwargs) -> ChatOpenAI:
        """创建通义千问模型实例（通过 OpenAI 兼容端点）"""
        api_key = kwargs.get("api_key") or os.getenv("QWEN_API_KEY")
        model_name = kwargs.get("model_name") or kwargs.get("model") or "qwen-plus"
        base_url = kwargs.get("base_url") or os.getenv("QWEN_BASE_URL") or "https://dashscope.aliyuncs.com/compatible-mode/v1"
        
        return ChatOpenAI(
            model_name=model_name,
            api_key=api_key,
            openai_api_base=base_url,
            temperature=kwargs.get("temperature", 0.7),
            max_tokens=kwargs.get("max_tokens", 4096),
        )

    @staticmethod
    def _create_siliconflow(**kwargs) -> ChatOpenAI:
        """创建硅基流动模型实例（通过 OpenAI 兼容端点）"""
        api_key = kwargs.get("api_key") or os.getenv("SILICONFLOW_API_KEY")
        model_name = kwargs.get("model_name") or kwargs.get("model") or "Qwen/Qwen2-7B-Instruct"
        base_url = kwargs.get("base_url") or os.getenv("SILICONFLOW_BASE_URL") or "https://api.siliconflow.cn/v1"
        
        return ChatOpenAI(
            model_name=model_name,
            api_key=api_key,
            openai_api_base=base_url,
            temperature=kwargs.get("temperature", 0.7),
            max_tokens=kwargs.get("max_tokens", 4096),
        )

    @staticmethod
    def get_supported_models() -> list:
        """获取当前支持的模型类型列表"""
        return ["openai", "chatglm", "ollama", "deepseek", "qwen", "siliconflow"]

    @staticmethod
    def validate_config(model_type: str, config: Dict[str, Any]) -> bool:
        """
        验证 LLM 配置是否完整

        对于云端模型，检查是否提供了 api_key（直接传入或通过环境变量）。
        对于本地模型（ollama），不需要 api_key，直接返回 True。

        Args:
            model_type: 模型类型
            config: 配置字典

        Returns:
            True 表示配置有效，False 表示缺少必要配置
        """
        model_type = model_type.lower().strip()
        required_keys = []  # 预留字段，当前未使用

        # 云端模型必须有 api_key（配置中或环境变量中）
        if model_type in ["openai", "chatglm", "deepseek", "qwen", "siliconflow"]:
            if not config.get("api_key") and not os.getenv(f"{model_type.upper()}_API_KEY"):
                return False
        
        return True