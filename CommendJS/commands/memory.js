/**
 * 记忆命令模块
 *
 * 管理记忆文件（清空、压缩等）
 */

const fs = require('fs');
const path = require('path');
const readline = require('readline');
const { getEnvVar, ENV_PREFIX } = require('../utils/env');

// 记忆文件夹名称
const MEMORY_FOLDER = 'memory';

/**
 * 处理记忆命令
 * @param {string[]} args - 命令参数
 */
function handleMemory(args) {
    if (args.length === 0) {
        console.error('❌ 缺少具体命令');
        console.log('使用 "dtp memory clean/zip" 进行记忆管理');
        process.exit(1);
    }

    const subCommand = args[0].toLowerCase();

    switch (subCommand) {
        case 'clean':
            handleMemoryClean();
            break;

        case 'zip':
            handleMemoryZip();
            break;

        default:
            console.error(`❌ 未知的记忆命令: ${subCommand}`);
            console.log('可用命令: clean, zip');
            process.exit(1);
    }
}

/**
 * 获取记忆文件夹路径
 * @returns {string|null} 记忆文件夹路径
 */
function getMemoryPath() {
    // 从环境变量获取项目路径
    const projectPathEnv = getEnvVar(`${ENV_PREFIX}PROJECT_PATH`);

    if (!projectPathEnv) {
        console.error('❌ 未设置项目路径');
        console.log('请先使用 "dtp config set path <路径>" 设置项目路径');
        return null;
    }

    const memoryPath = path.join(projectPathEnv, MEMORY_FOLDER);

    // 检查记忆文件夹是否存在
    if (!fs.existsSync(memoryPath)) {
        console.error(`❌ 记忆文件夹不存在: ${memoryPath}`);
        return null;
    }

    return memoryPath;
}

/**
 * 清空所有记忆文件
 */
function handleMemoryClean() {
    const memoryPath = getMemoryPath();

    if (!memoryPath) {
        process.exit(1);
    }

    // 检查文件夹是否为空
    const files = fs.readdirSync(memoryPath);
    if (files.length === 0) {
        console.log('⚠️  记忆文件夹已经是空的');
        return;
    }

    // 显示将要删除的文件
    console.log(`\n记忆文件夹: ${memoryPath}`);
    console.log('将要删除以下文件:');
    files.forEach(file => {
        const filePath = path.join(memoryPath, file);
        const stats = fs.statSync(filePath);
        const size = formatFileSize(stats.size);
        console.log(`  ${file} (${size})`);
    });

    // 创建命令行交互
    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout
    });

    rl.question(`\n确认清空所有记忆文件? (共 ${files.length} 个文件) (yes/no): `, (answer) => {
        rl.close();

        if (answer.toLowerCase() === 'yes' || answer.toLowerCase() === 'y') {
            try {
                files.forEach(file => {
                    const filePath = path.join(memoryPath, file);
                    fs.rmSync(filePath, { recursive: true, force: true });
                });
                console.log('✅ 已清空所有记忆文件');
            } catch (error) {
                console.error('❌ 清空记忆文件失败:', error.message);
                process.exit(1);
            }
        } else {
            console.log('❌ 已取消清空操作');
        }
    });
}

/**
 * 压缩所有记忆文件
 *
 * 此功能会调用 Python 接口进行实际的压缩操作
 * 这里只提供命令入口，具体实现由 Python 完成
 */
function handleMemoryZip() {
    const memoryPath = getMemoryPath();

    if (!memoryPath) {
        process.exit(1);
    }

    // 检查文件夹是否为空
    const files = fs.readdirSync(memoryPath);
    if (files.length === 0) {
        console.log('⚠️  记忆文件夹是空的，无需压缩');
        return;
    }

    console.log(`\n记忆文件夹: ${memoryPath}`);
    console.log(`发现 ${files.length} 个文件/文件夹`);
    console.log('\n正在准备压缩记忆文件...');

    // 调用 Python 接口（此处为接口预留）
    // 实际压缩逻辑由 Python 实现
    callPythonZipAPI(memoryPath);
}

/**
 * 调用 Python 压缩接口
 * @param {string} memoryPath - 记忆文件夹路径
 */
function callPythonZipAPI(memoryPath) {
    const { spawn } = require('child_process');

    // 获取项目根路径
    const projectPathEnv = getEnvVar(`${ENV_PREFIX}PROJECT_PATH`);
    const pythonScriptPath = path.join(projectPathEnv, 'PythonAgent', 'src', 'memory', 'zip_memory.py');

    // 检查 Python 脚本是否存在
    if (!fs.existsSync(pythonScriptPath)) {
        console.log('⚠️  Python 压缩脚本尚未实现');
        console.log('\n接口已预留，等待 Python 实现:');
        console.log(`  脚本路径: ${pythonScriptPath}`);
        console.log('\n实现说明:');
        console.log('  1. 在 PythonAgent/src/memory/ 下创建 zip_memory.py');
        console.log('  2. 接收记忆文件夹路径作为参数');
        console.log('  3. 压缩记忆文件并保存到指定位置');
        console.log('  4. 返回压缩结果');
        return;
    }

    // 调用 Python 脚本
    const pythonProcess = spawn('python', [pythonScriptPath, memoryPath]);

    pythonProcess.stdout.on('data', (data) => {
        console.log(data.toString());
    });

    pythonProcess.stderr.on('data', (data) => {
        console.error(data.toString());
    });

    pythonProcess.on('close', (code) => {
        if (code === 0) {
            console.log('✅ 记忆文件压缩完成');
        } else {
            console.error('❌ 记忆文件压缩失败');
            process.exit(1);
        }
    });
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

module.exports = { handleMemory };