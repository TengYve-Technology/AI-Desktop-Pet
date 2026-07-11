from typing import Optional, Dict, Any
from langchain_openai import ChatOpenAI
from langchain_ollama import ChatOllama
from langchain_core.language_models import BaseChatModel
import os

class LLMFactory:
    @staticmethod
    def create_llm(model_type: str, **kwargs) -> BaseChatModel:
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
        return ["openai", "chatglm", "ollama", "deepseek", "qwen", "siliconflow"]

    @staticmethod
    def validate_config(model_type: str, config: Dict[str, Any]) -> bool:
        model_type = model_type.lower().strip()
        required_keys = []
        
        if model_type in ["openai", "chatglm", "deepseek", "qwen", "siliconflow"]:
            if not config.get("api_key") and not os.getenv(f"{model_type.upper()}_API_KEY"):
                return False
        
        return True