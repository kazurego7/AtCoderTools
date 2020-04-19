﻿using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
// Entrypoint, create from the .NET Core Console App.
class Program : ConsoleAppBase // inherit ConsoleAppBase
{
    static async Task Main(string[] args)
    {
        await Task.Run(() =>
        {
            var templateConfig = new TemplateConfig();
            templateConfig.Init();
        });
        // target T as ConsoleAppBase.
        await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
    }

    [Command(new[] { "ushitapunikiakun", "uk", }, "????????????")]
    public void UshitapunikiakunCommand()
    {
        Console.Error.WriteLine("う　し　た　ぷ　に　き　あ　く　ん　（笑）");
    }

    [Command(new[] { "install", "i", }, "テンプレートのインストール")]
    public void InstallCommand([Option(0, "テンプレート名")]string templateName, [Option(1, "テンプレートへのパス")]string templatePath)
    {
        var templateConfig = new TemplateConfig();
        var invalidPathString = new string(Path.GetInvalidPathChars());
        if (invalidPathString.Any(invalidChar => templatePath.Contains(invalidChar)))
        {
            Console.Error.WriteLine($"WA! 以下の文字は、テンプレートへのパスに含むことができません。");
            Console.Error.WriteLine($"    {String.Join(" ", invalidPathString)}");
            return;
        }

        if (!Directory.Exists(templatePath))
        {
            Console.Error.WriteLine($"WA! {templatePath} は存在しません。");
            Console.Error.WriteLine($"    存在するテンプレートへのパスを指定してください。");
            return;
        }

        var template = new Template { name = templateName, path = Path.GetFullPath(templatePath) };
        if (!templateConfig.IsInstalled(template.name))
        {
            templateConfig.Add(template);
            templateConfig.Write();
            Console.WriteLine($"AC! テンプレート名 {template.name} に、{template.path} をインストールしました。");
        }
        else
        {
            Console.WriteLine($"テンプレート名 {template.name} には、テンプレートへのパス {templateConfig.Get(template.name).path} が既にインストールされています。");
            Console.WriteLine("テンプレートへのパスを上書きしますか？ (yes/no)");
            var input = Console.ReadLine();
            if (input == "yes")
            {
                templateConfig.Add(template);
                templateConfig.Write();
                Console.WriteLine($"AC! テンプレート名 {template.name} に、{template.path} を上書きインストールしました。");
            }
            else
            {
                Console.WriteLine($"テンプレート名 {template.name} を上書きせず終了しました。");
            }
        }
    }

    [Command(new[] { "uninstall", "un", "remove", "rm" }, "テンプレートのアンインストール")]
    public void RemoveCommand([Option(0, "テンプレート名")]string templateName)
    {
        var templateConfig = new TemplateConfig();
        if (!templateConfig.IsInstalled(templateName))
        {
            Console.Error.WriteLine($"WA! {templateName} がテンプレート名に存在しません。");
            Console.Error.WriteLine($"    テンプレート一覧を確認してください。");

            Console.WriteLine($"テンプレート名 : テンプレートへのパス");
            foreach (var template in templateConfig.ListInstalledTemplate())
            {
                Console.WriteLine($"{template.name} : {template.path}");
            }
            return;
        }

        templateConfig.Remove(templateName);
        templateConfig.Write();
        Console.WriteLine($"AC! テンプレート名 {templateName} をアンインストールしました。");
    }

    [Command(new[] { "list", "ls", }, "テンプレートの一覧")]
    public void ListCommand([Option("r", "アンインストールしたテンプレートを表示する")]bool removed = false)
    {
        var templateConfig = new TemplateConfig();
        if (!removed)
        {
            Console.WriteLine($"テンプレート名 : テンプレートへのパス");
            foreach (var template in templateConfig.ListInstalledTemplate())
            {
                Console.WriteLine($"{template.name} : {template.path}");
            }
        }
        else
        {
            Console.WriteLine($"アンインストールしたテンプレート名 : テンプレートへのパス");
            foreach (var template in templateConfig.ListRemovedTemplate())
            {
                Console.WriteLine($"{template.name} : {template.path}");
            }
        }
    }

    [Command(new[] { "restore", "rs", }, "アンインストールしたテンプレートの復元")]
    public void RestoreCommand([Option(0, "テンプレート名")]string templateName)
    {
        var templateConfig = new TemplateConfig();

        if (!templateConfig.IsRemoved(templateName))
        {
            Console.Error.WriteLine($"WA! {templateName} がアンインストールしたテンプレート名に存在しません。");
            Console.Error.WriteLine($"    テンプレート一覧を確認してください。");

            Console.WriteLine($"アンインストールしたテンプレート名 : テンプレートへのパス");
            foreach (var template in templateConfig.ListRemovedTemplate())
            {
                Console.WriteLine($"{template.name} : {template.path}");
            }
            return;
        }

        templateConfig.Restore(templateName);
        var restoredTemplate = templateConfig.Get(templateName);
        Console.WriteLine($"AC! テンプレート {restoredTemplate.name} : {restoredTemplate.path} を復元しました");
    }

    [Command(new[] { "new", "n", }, "コンテスト用のプロジェクト作成")]
    public void CreateCommand([Option(0, "利用するテンプレート名")]string templateName, [Option(1, "作成するコンテスト名")]string contestName)
    {
        var templateConfig = new TemplateConfig();
        if (!templateConfig.IsInstalled(templateName))
        {
            Console.Error.WriteLine($"WA! {templateName} がテンプレート名に存在しません。");
            Console.Error.WriteLine($"    テンプレート一覧を確認してください。");

            Console.WriteLine($"テンプレート名 : テンプレートへのパス");
            foreach (var template in templateConfig.ListInstalledTemplate())
            {
                Console.WriteLine($"{template.name} : {template.path}");
            }
            return;
        }

        var invalidFileNameString = new string(Path.GetInvalidFileNameChars());
        if (invalidFileNameString.Any(invalidChar => contestName.Contains(invalidChar)))
        {
            Console.Error.WriteLine($"WA! 以下の文字は、コンテスト名に含むことができません。");
            Console.Error.WriteLine($"    {String.Join(" ", invalidFileNameString)}");
            return;
        }
        if (Directory.Exists(contestName) || File.Exists(contestName))
        {
            Console.Error.WriteLine($"WA! {contestName} はすでに存在しています。別のコンテスト名を使用してください。");
            return;
        }

        if (!templateConfig.IsInstalled(templateName))
        {
            var template = templateConfig.Get(templateName);
            Console.Error.WriteLine($"CE! テンプレート名 {template.name} のパス {template.path} に、テンプレートが存在しません。");
            Console.Error.WriteLine($"    install コマンドで、テンプレートへのパスを修正してください。");
            return;
        }

        Console.WriteLine("WJ... コンテスト名のディレクトリを作成します。");

        Directory.CreateDirectory(contestName);
        foreach (var problemName in new List<string> { "A", "B", "C", "D", "E", "F" })
        {
            Console.WriteLine($"WJ... {problemName} ディレクトリを作成します。");
            var template = templateConfig.Get(templateName);
            var problemPath = Path.Combine(contestName, problemName);
            DirectoryEx.Copy(template.path, problemPath);
        }

        Console.WriteLine("AC! コンテスト用の各問題プロジェクトの作成が完了しました。");
    }
}