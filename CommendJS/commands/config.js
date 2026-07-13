/**
 * 配置命令模块
 *
 * 管理应用配置（通过环境变量实现）
 */

const { setEnvVar, getEnvVar, getAllEnvVars, clearAllEnvVars, ENV_PREFIX } = require('../utils/env');
const { validateModelType, validateApiKey, validateUrl } = require('../utils/validator');
const readline = require('readline');

// 支持的配置项及其验证函数
const CONFIG_ITEMS = {
    model: {
        name: 'AI 模型类型',
        validate: validateModelType,
        envKey: 'MODEL_TYPE'
    },
    'api-url': {
        name: 'API 地址',
        validate: validateUrl,
        envKey: 'API_URL'
    },
    'api-secret': {
        name: 'API 密钥',
        validate: validateApiKey,
        envKey: 'API_SECRET'
    },
    path: {
        name: '项目路径',
        validate: validatePath,
        envKey: 'PROJECT_PATH'
    }
};

/**
 * 处理配置命令
 * @param {string[]} args - 命令参数
 */
function handleConfig(args) {
    if (args.length === 0) {
        console.error('❌ 缺少具体命令');
        console.log('使用 "dtp config set/show/clean" 进行配置管理');
        process.exit(1);
    }

    const subCommand = args[0].toLowerCase();

    switch (subCommand) {
        case 'set':
            handleConfigSet(args.slice(1));
            break;

        case 'show':
            handleConfigShow();
            break;

        case 'clean':
            handleConfigClean();
            break;

        default:
            console.error(`❌ 未知的配置命令: ${subCommand}`);
            console.log('可用命令: set, show, clean');
            process.exit(1);
    }
}

/**
 * 处理设置命令
 * @param {string[]} args - 参数数组
 */
function handleConfigSet(args) {
    if (args.length < 2) {
        console.error('❌ 参数不足');
        console.log('用法: dtp config set <配置项> <值>');
        console.log('\n支持的配置项:');
        Object.entries(CONFIG_ITEMS).forEach(([key, item]) => {
            console.log(`  ${key.padEnd(12)} - ${item.name}`);
        });
        process.exit(1);
    }

    const configKey = args[0].toLowerCase();
    const configValue = args[1];

    // 检查配置项是否支持
    if (!CONFIG_ITEMS[configKey]) {
        console.error(`❌ 不支持的配置项: ${configKey}`);
        console.log('支持的配置项:', Object.keys(CONFIG_ITEMS).join(', '));
        process.exit(1);
    }

    const configItem = CONFIG_ITEMS[configKey];

    // 获取当前模型类型（如果设置的是 api-secret，需要验证密钥格式）
    let currentModel = null;
    if (configKey === 'api-secret') {
        currentModel = getEnvVar('MODEL_TYPE') || 'openai';
    }

    // 执行验证
    const validationResult = configItem.validate(configValue, currentModel);

    if (!validationResult.valid) {
        console.error(`❌ ${validationResult.message}`);
        process.exit(1);
    }

    // 设置环境变量
    const envKey = `${ENV_PREFIX}${configItem.envKey}`;
    setEnvVar(envKey, configValue);

    console.log(`✅ 已设置 ${configItem.name}: ${configValue}`);
    if (validationResult.warning) {
        console.log(`⚠️  ${validationResult.warning}`);
    }
}

/**
 * 显示所有配置
 */
function handleConfigShow() {
    console.log('\n━━━ 当前配置 ━━━\n');

    const envVars = getAllEnvVars();

    if (envVars.length === 0) {
        console.log('暂无配置');
        console.log('\n使用 "dtp config set <配置项> <值>" 进行设置');
        return;
    }

    // 按配置项顺序显示
    const configuredItems = [];
    const notConfiguredItems = [];

    Object.entries(CONFIG_ITEMS).forEach(([key, item]) => {
        const envKey = `${ENV_PREFIX}${item.envKey}`;
        const value = getEnvVar(envKey);

        if (value) {
            configuredItems.push({ key, name: item.name, value });
        } else {
            notConfiguredItems.push({ key, name: item.name });
        }
    });

    // 显示已配置项
    if (configuredItems.length > 0) {
        console.log('已配置:');
        configuredItems.forEach(item => {
            // 如果是 API 密钥，只显示部分内容
            const displayValue = item.key === 'api-secret'
                ? maskApiKey(item.value)
                : item.value;
            console.log(`  ${item.name.padEnd(12)} ${displayValue}`);
        });
    }

    // 显示未配置项
    if (notConfiguredItems.length > 0) {
        console.log('\n未配置:');
        notConfiguredItems.forEach(item => {
            console.log(`  ${item.name.padEnd(12)} (未设置)`);
        });
    }

    console.log('');
}

/**
 * 清除所有配置
 */
function handleConfigClean() {
    const envVars = getAllEnvVars();

    if (envVars.length === 0) {
        console.log('⚠️  没有需要清除的配置');
        return;
    }

    // 创建命令行交互
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout
    });

    console.log('\n即将清除以下配置:');
    envVars.forEach(envVar => {
        const keyWithoutPrefix = envVar.key.replace(ENV_PREFIX, '');
        const configItem = Object.values(CONFIG_ITEMS).find(item => item.envKey === keyWithoutPrefix);
        const displayName = configItem ? configItem.name : envVar.key;
        console.log(`  ${displayName}: ${envVar.value}`);
    });

    rl.question('\n确认清除所有配置? (yes/no): ', (answer) => {
        rl.close();

        if (answer.toLowerCase() === 'yes' || answer.toLowerCase() === 'y') {
            clearAllEnvVars();
            console.log('✅ 已清除所有配置');
        } else {
            console.log('❌ 已取消清除操作');
        }
    });
}

/**
 * 遮蔽 API 密钥显示
 * @param {string} key - API 密钥
 * @returns {string} 遮蔽后的密钥
 */
function maskApiKey(key) {
    if (!key || key.length < 8) {
        return '****';
    }
    return key.substring(0, 4) + '****' + key.substring(key.length - 4);
}

/**
 * 验证项目路径
 * @param {string} path - 路径值
 * @returns {object} 验证结果
 */
function validatePath(path) {
    const fs = require('fs');

    if (!path || path.trim() === '') {
        return { valid: false, message: '路径不能为空' };
    }

    // 检查路径是否存在
    if (!fs.existsSync(path)) {
        return {
            valid: true,
            warning: `路径 "${path}" 不存在，请确保路径正确`
        };
    }

    // 检查是否为目录
    const stats = fs.statSync(path);
    if (!stats.isDirectory()) {
        return { valid: false, message: '路径必须是一个目录' };
    }

    return { valid: true };
}

module.exports = { handleConfig, CONFIG_ITEMS };