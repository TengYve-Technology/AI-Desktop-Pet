#!/usr/bin/env node

/**
 * AI Desktop Pet 命令行工具入口
 *
 * 命令格式：dtp <命令选项> <具体命令> <参数一名称> <参数一数值> ...
 *
 * 示例：
 *   dtp config set model openai
 *   dtp config set api-key sk-xxxxx
 *   dtp config show
 *   dtp memory clean
 */

const path = require('path');
const fs = require('fs');
const { showHelp } = require('./commands/help');
const { handleConfig } = require('./commands/config');
const { handleMemory } = require('./commands/memory');
const { showVersion } = require('./commands/version');
const { showStatus } = require('./commands/status');

// 主函数
function main() {
    const args = process.argv.slice(2);

    // 如果没有参数，显示帮助信息
    if (args.length === 0) {
        showHelp();
        process.exit(0);
    }

    // 解析命令选项
    const commandOption = args[0].toLowerCase();

    // 处理简写和帮助命令
    switch (commandOption) {
        case '-h':
        case '--help':
        case 'help':
            showHelp();
            process.exit(0);
            break;

        case '-v':
        case '--version':
        case 'version':
            showVersion();
            process.exit(0);
            break;

        case 'status':
            showStatus();
            process.exit(0);
            break;

        case 'config':
            handleConfig(args.slice(1));
            break;

        case 'memory':
            handleMemory(args.slice(1));
            break;

        default:
            console.error(`❌ 未知命令选项: ${commandOption}`);
            console.log('使用 "dtp help" 查看可用命令');
            process.exit(1);
    }
}

// 执行主函数
main();