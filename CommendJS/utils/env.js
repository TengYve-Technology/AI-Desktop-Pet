/**
 * 环境变量操作模块
 *
 * 提供跨平台的环境变量读写功能
 *
 * 注意：在 Windows 和 Unix-like 系统上，环境变量的持久化方式不同
 * - Windows: 使用 setx 命令永久设置环境变量
 * - Unix-like: 需要修改 shell 配置文件（如 ~/.bashrc, ~/.zshrc）
 *
 * 本模块默认将环境变量保存到用户级别（永久）
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

// 环境变量前缀，避免与其他应用冲突
const ENV_PREFIX = 'DTP_';

// 本地配置文件路径（用于持久化存储）
const CONFIG_FILE_NAME = '.desktoppet-config.json';

/**
 * 获取配置文件路径
 * @returns {string} 配置文件路径
 */
function getConfigFilePath() {
    // 在用户主目录下存储配置
    const homeDir = process.env.HOME || process.env.USERPROFILE;
    return path.join(homeDir, CONFIG_FILE_NAME);
}

/**
 * 读取本地配置文件
 * @returns {object} 配置对象
 */
function readLocalConfig() {
    const configPath = getConfigFilePath();

    if (!fs.existsSync(configPath)) {
        return {};
    }

    try {
        const content = fs.readFileSync(configPath, 'utf8');
        return JSON.parse(content);
    } catch (error) {
        console.error('读取配置文件失败:', error.message);
        return {};
    }
}

/**
 * 写入本地配置文件
 * @param {object} config - 配置对象
 */
function writeLocalConfig(config) {
    const configPath = getConfigFilePath();

    try {
        fs.writeFileSync(configPath, JSON.stringify(config, null, 2), 'utf8');
    } catch (error) {
        console.error('写入配置文件失败:', error.message);
    }
}

/**
 * 设置环境变量
 * @param {string} key - 环境变量名
 * @param {string} value - 环境变量值
 */
function setEnvVar(key, value) {
    // 1. 设置当前进程的环境变量
    process.env[key] = value;

    // 2. 保存到本地配置文件（持久化）
    const config = readLocalConfig();
    config[key] = value;
    writeLocalConfig(config);

    // 3. 尝试设置系统环境变量（用户级别）
    try {
        if (process.platform === 'win32') {
            // Windows: 使用 setx 命令
            execSync(`setx ${key} "${value}"`, { stdio: 'pipe' });
        } else {
            // Unix-like: 需要手动添加到 shell 配置文件
            // 这里只做提示，不自动修改
            console.log(`\n提示: 请在 shell 配置文件中添加: export ${key}="${value}"`);
            console.log(`或者运行: export ${key}="${value}"\n`);
        }
    } catch (error) {
        // setx 命令可能失败（如权限不足），但不影响使用本地配置文件
        // 静默处理错误
    }
}

/**
 * 获取环境变量
 * @param {string} key - 环境变量名
 * @returns {string|null} 环境变量值
 */
function getEnvVar(key) {
    // 优先从当前进程环境变量获取
    if (process.env[key]) {
        return process.env[key];
    }

    // 从本地配置文件获取
    const config = readLocalConfig();
    return config[key] || null;
}

/**
 * 获取所有相关环境变量
 * @returns {Array} 环境变量列表 [{key, value}, ...]
 */
function getAllEnvVars() {
    const result = [];

    // 从本地配置文件获取
    const config = readLocalConfig();

    // 只返回带有前缀的环境变量
    Object.entries(config).forEach(([key, value]) => {
        if (key.startsWith(ENV_PREFIX)) {
            result.push({ key, value });
        }
    });

    return result;
}

/**
 * 清除所有环境变量
 */
function clearAllEnvVars() {
    const config = readLocalConfig();

    // 只清除带有前缀的环境变量
    Object.keys(config).forEach(key => {
        if (key.startsWith(ENV_PREFIX)) {
            delete config[key];

            // 尝试清除系统环境变量
            try {
                if (process.platform === 'win32') {
                    execSync(`setx ${key} ""`, { stdio: 'pipe' });
                }
            } catch (error) {
                // 静默处理错误
            }
        }
    });

    writeLocalConfig(config);
}

/**
 * 初始化环境变量（从本地配置文件加载到当前进程）
 */
function initEnvVars() {
    const config = readLocalConfig();

    Object.entries(config).forEach(([key, value]) => {
        if (!process.env[key]) {
            process.env[key] = value;
        }
    });
}

// 模块加载时初始化环境变量
initEnvVars();

module.exports = {
    ENV_PREFIX,
    setEnvVar,
    getEnvVar,
    getAllEnvVars,
    clearAllEnvVars,
    initEnvVars
};