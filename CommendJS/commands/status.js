/**
 * 状态命令模块
 *
 * 显示当前配置、服务状态等信息
 */

const { getEnvVar, ENV_PREFIX } = require('../utils/env');
const { CONFIG_ITEMS } = require('./config');
const fs = require('fs');
const path = require('path');

/**
 * 显示当前状态
 */
function showStatus() {
    console.log('\n╔══════════════════════════════════════════════════════════════╗');
    console.log('║                     AI Desktop Pet 状态                      ║');
    console.log('╚══════════════════════════════════════════════════════════════╝\n');

    // 显示配置状态
    showConfigStatus();

    // 显示服务状态
    showServiceStatus();

    // 显示记忆状态
    showMemoryStatus();
}

/**
 * 显示配置状态
 */
function showConfigStatus() {
    console.log('━━━ 配置状态 ━━━\n');

    const modelType = getEnvVar(`${ENV_PREFIX}MODEL_TYPE`);
    const apiUrl = getEnvVar(`${ENV_PREFIX}API_URL`);
    const apiSecret = getEnvVar(`${ENV_PREFIX}API_SECRET`);
    const projectPath = getEnvVar(`${ENV_PREFIX}PROJECT_PATH`);

    // 模型类型
    if (modelType) {
        console.log(`✅ AI 模型: ${modelType}`);
    } else {
        console.log('⚠️  AI 模型: 未设置 (使用 "dtp config set model <模型类型>" 设置)');
    }

    // API 地址
    if (apiUrl) {
        console.log(`✅ API 地址: ${apiUrl}`);
    } else {
        console.log('⚠️  API 地址: 未设置 (使用 "dtp config set api-url <地址>" 设置)');
    }

    // API 密钥
    if (apiSecret) {
        const maskedSecret = apiSecret.substring(0, 4) + '****' + apiSecret.substring(apiSecret.length - 4);
        console.log(`✅ API 密钥: ${maskedSecret}`);
    } else {
        console.log('⚠️  API 密钥: 未设置 (使用 "dtp config set api-secret <密钥>" 设置)');
    }

    // 项目路径
    if (projectPath) {
        if (fs.existsSync(projectPath)) {
            console.log(`✅ 项目路径: ${projectPath}`);
        } else {
            console.log(`⚠️  项目路径: ${projectPath} (路径不存在)`);
        }
    } else {
        console.log('⚠️  项目路径: 未设置 (使用 "dtp config set path <路径>" 设置)');
    }

    console.log('');
}

/**
 * 显示服务状态
 */
function showServiceStatus() {
    console.log('━━━ 服务状态 ━━━\n');

    const projectPath = getEnvVar(`${ENV_PREFIX}PROJECT_PATH`);

    if (!projectPath) {
        console.log('⚠️  无法检查服务状态（项目路径未设置）\n');
        return;
    }

    // 检查 Python 服务配置文件
    const configPath = path.join(projectPath, 'PythonAgent', 'config.yaml');
    if (fs.existsSync(configPath)) {
        console.log('✅ Python 服务配置文件: 存在');

        // 读取并显示部分配置
        try {
            const configContent = fs.readFileSync(configPath, 'utf8');
            const hostMatch = configContent.match(/host:\s*"([^"]+)"/);
            const portMatch = configContent.match(/port:\s*(\d+)/);

            if (hostMatch && portMatch) {
                console.log(`   服务地址: ws://${hostMatch[1]}:${portMatch[1]}`);
            }
        } catch (error) {
            console.log('   无法读取配置文件');
        }
    } else {
        console.log('⚠️  Python 服务配置文件: 不存在');
    }

    // 检查虚拟环境
    const venvPath = path.join(projectPath, 'PythonAgent', 'venv');
    const venvPathAlt = path.join(projectPath, 'PythonAgent', '.venv');
    if (fs.existsSync(venvPath)) {
        console.log('✅ Python 虚拟环境: 存在');
    } else if (fs.existsSync(venvPathAlt)) {
        console.log('✅ Python 虚拟环境: 存在');
    } else {
        console.log('⚠️  Python 虚拟环境: 不存在');
    }

    console.log('');
}

/**
 * 显示记忆状态
 */
function showMemoryStatus() {
    console.log('━━━ 记忆状态 ━━━\n');

    const projectPath = getEnvVar(`${ENV_PREFIX}PROJECT_PATH`);

    if (!projectPath) {
        console.log('⚠️  无法检查记忆状态（项目路径未设置）\n');
        return;
    }

    const memoryPath = path.join(projectPath, 'memory');

    if (!fs.existsSync(memoryPath)) {
        console.log('⚠️  记忆文件夹: 不存在\n');
        return;
    }

    console.log('✅ 记忆文件夹: 存在');

    // 统计记忆文件
    try {
        const files = fs.readdirSync(memoryPath);
        if (files.length === 0) {
            console.log('   状态: 空');
        } else {
            let totalSize = 0;
            let fileCount = 0;

            files.forEach(file => {
                const filePath = path.join(memoryPath, file);
                const stats = fs.statSync(filePath);
                totalSize += stats.size;
                fileCount++;
            });

            console.log(`   文件数量: ${fileCount}`);
            console.log(`   总大小: ${formatFileSize(totalSize)}`);
        }
    } catch (error) {
        console.log('   无法读取记忆文件夹');
    }

    console.log('');
}

/**
 * 格式化文件大小
 * @param {number} bytes - 字节数
 * @returns {string} 格式化后的大小
 */
function formatFileSize(bytes) {
    if (bytes === 0) return '0 B';

    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

module.exports = { showStatus };