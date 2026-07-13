/**
 * 验证器模块
 *
 * 提供各种格式验证功能
 */

// 支持的模型类型列表（从 Python factory.py 中提取）
const SUPPORTED_MODELS = ['openai', 'chatglm', 'ollama', 'deepseek', 'qwen', 'siliconflow'];

// API 密钥格式正则表达式
const API_KEY_PATTERNS = {
    openai: /^sk-[a-zA-Z0-9]{20,}$/,                    // OpenAI: sk- 开头，至少 20 个字符
    chatglm: /^[a-zA-Z0-9]{32,}$/,                      // 智谱 AI: 至少 32 个字符
    deepseek: /^sk-[a-zA-Z0-9]{20,}$/,                  // DeepSeek: sk- 开头，至少 20 个字符
    qwen: /^sk-[a-zA-Z0-9]{20,}$/,                      // 通义千问: sk- 开头，至少 20 个字符
    siliconflow: /^sk-[a-zA-Z0-9]{20,}$/,               // 硅基流动: sk- 开头，至少 20 个字符
    ollama: null                                        // Ollama 不需要 API 密钥
};

/**
 * 验证模型类型
 * @param {string} modelType - 模型类型
 * @returns {object} 验证结果 { valid: boolean, message?: string, warning?: string }
 */
function validateModelType(modelType) {
    if (!modelType || modelType.trim() === '') {
        return { valid: false, message: '模型类型不能为空' };
    }

    const normalizedType = modelType.toLowerCase().trim();

    if (!SUPPORTED_MODELS.includes(normalizedType)) {
        return {
            valid: false,
            message: `不支持的模型类型: ${modelType}`,
            warning: `支持的模型类型: ${SUPPORTED_MODELS.join(', ')}`
        };
    }

    // 如果是 ollama，提醒用户确保本地服务已启动
    if (normalizedType === 'ollama') {
        return {
            valid: true,
            warning: 'Ollama 是本地模型，请确保本地 Ollama 服务已启动 (默认端口: 11434)'
        };
    }

    return { valid: true };
}

/**
 * 验证 API 密钥格式
 * @param {string} apiKey - API 密钥
 * @param {string} modelType - 模型类型（可选，用于特定格式验证）
 * @returns {object} 验证结果
 */
function validateApiKey(apiKey, modelType = null) {
    if (!apiKey || apiKey.trim() === '') {
        return { valid: false, message: 'API 密钥不能为空' };
    }

    // 如果没有指定模型类型，只做基本验证
    if (!modelType) {
        // 至少要有一定的长度
        if (apiKey.length < 10) {
            return { valid: false, message: 'API 密钥长度不足，请检查密钥格式' };
        }
        return { valid: true };
    }

    const normalizedModelType = modelType.toLowerCase().trim();

    // Ollama 不需要 API 密钥
    if (normalizedModelType === 'ollama') {
        return {
            valid: true,
            warning: 'Ollama 模型不需要 API 密钥，此设置将被忽略'
        };
    }

    // 获取对应的验证规则
    const pattern = API_KEY_PATTERNS[normalizedModelType];

    if (!pattern) {
        // 没有特定验证规则，只做基本验证
        if (apiKey.length < 10) {
            return { valid: false, message: 'API 密钥长度不足，请检查密钥格式' };
        }
        return { valid: true };
    }

    // 使用正则表达式验证
    if (!pattern.test(apiKey)) {
        return {
            valid: false,
            message: `${modelType} 的 API 密钥格式不正确`,
            warning: `期望格式: ${getApiKeyFormatHint(normalizedModelType)}`
        };
    }

    return { valid: true };
}

/**
 * 验证 URL 格式
 * @param {string} url - URL 地址
 * @returns {object} 验证结果
 */
function validateUrl(url) {
    if (!url || url.trim() === '') {
        return { valid: false, message: 'URL 不能为空' };
    }

    try {
        const urlObj = new URL(url);

        // 检查协议
        if (!['http:', 'https:'].includes(urlObj.protocol)) {
            return { valid: false, message: 'URL 协议必须为 http 或 https' };
        }

        // 检查主机名
        if (!urlObj.hostname) {
            return { valid: false, message: 'URL 缺少主机名' };
        }

        return { valid: true };
    } catch (error) {
        return { valid: false, message: `无效的 URL 格式: ${error.message}` };
    }
}

/**
 * 获取 API 密钥格式提示
 * @param {string} modelType - 模型类型
 * @returns {string} 格式提示
 */
function getApiKeyFormatHint(modelType) {
    const hints = {
        openai: 'sk- 开头，后跟至少 20 个字母数字字符',
        chatglm: '至少 32 个字母数字字符',
        deepseek: 'sk- 开头，后跟至少 20 个字母数字字符',
        qwen: 'sk- 开头，后跟至少 20 个字母数字字符',
        siliconflow: 'sk- 开头，后跟至少 20 个字母数字字符',
        ollama: 'Ollama 不需要 API 密钥'
    };

    return hints[modelType] || '无特定格式要求';
}

module.exports = {
    validateModelType,
    validateApiKey,
    validateUrl,
    SUPPORTED_MODELS,
    API_KEY_PATTERNS
};