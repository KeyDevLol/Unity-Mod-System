using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Media;

namespace ModCompiler
{
    public static class Program
    {
        // Параметры компиляции для сборки мода.
        private static CompilerParameters parameters = new CompilerParameters();
        private static string folderPath; // Путь к папке мода.
        private static string lastFileName; // Имя последнего измененного файла.

        // Время последнего изменения файла, отслеживаемое FileSystemWatcher.
        public static DateTime watcherLastTime { get; private set; }

        // Точка входа в программу.
        private static void Main(string[] args)
        {
            // Проверка аргументов командной строки.
            if (args.Length == 0)
            {
                // Если аргументы не переданы, выводим сообщение об ошибке.
                Console.WriteLine("You need to specify the mod folder! To do this, drag the main mod folder onto the compiler exe file.");
            }
            else if (File.Exists(args[0]))
            {
                // Если передан файл, а не папка, выводим сообщение об ошибке.
                Console.WriteLine("You need to specify the mod folder, not the file! To do this, drag the main mod folder onto the compiler exe file.");
            }
            else
            {
                // Сохраняем путь к папке мода.
                folderPath = args[0];

                // Настраиваем параметры компиляции.
                parameters.GenerateExecutable = false; // Генерируем DLL, а не EXE.

                // Запрашиваем имя мода и задаем имя выходной сборки.
                Console.WriteLine("Enter the name of the mod: ");
                parameters.OutputAssembly = Path.Combine(folderPath, Console.ReadLine() + ".dll");

                // Читаем путь к папке с данными игры из файла dataPath.txt.
                string dataFolder = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "dataPath.txt"));
                // Получаем путь к папке Managed, где находятся сборки игры.
                string managedFolder = Path.Combine(new DirectoryInfo(AppContext.BaseDirectory).Parent.FullName, $"{dataFolder}\\Managed");

                // Добавляем все сборки из папки Managed в качестве ссылок для компиляции.
                foreach (string file in Directory.GetFiles(managedFolder, "*.dll"))
                {
                    if (Path.GetFileName(file).Contains("mscorlib"))
                        continue; // Пропускаем mscorlib, так как она уже включена по умолчанию.

                    parameters.ReferencedAssemblies.Add(file);
                }

                // Создаем FileSystemWatcher для отслеживания изменений в папке мода.
                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(args[0]);
                fileSystemWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess; // Отслеживаем изменения размера, записи и доступа.
                fileSystemWatcher.Filter = "*.cs"; // Отслеживаем только CS-файлы.
                fileSystemWatcher.Changed += new FileSystemEventHandler(Program.OnModChanged); // Обработчик изменения файлов.
                fileSystemWatcher.Error += new ErrorEventHandler(Program.OnWatcherError); // Обработчик ошибок.

                // Компилируем мод при старте.
                Compile();

                // Сообщаем, что компилятор готов к работе.
                Console.WriteLine("Ready! You can start making a mod");

                // Включаем отслеживание изменений.
                fileSystemWatcher.EnableRaisingEvents = true;
                watcherLastTime = DateTime.Now;

                // Ожидаем ввода пользователя, чтобы программа не завершилась сразу.
                Console.ReadLine();
                fileSystemWatcher.Dispose(); // Освобождаем ресурсы FileSystemWatcher.
                fileSystemWatcher.Changed -= new FileSystemEventHandler(Program.OnModChanged); // Отписываемся от события.
            }
        }

        // Обработчик ошибок FileSystemWatcher.
        private static void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.ToString()); // Выводим ошибку в консоль.
        }

        // Обработчик изменения файлов в папке мода.
        private static void OnModChanged(object sender, FileSystemEventArgs e)
        {
            // Проверяем, чтобы не обрабатывать одно и то же изменение несколько раз.
            if (DateTime.Now.Subtract(Program.watcherLastTime).TotalMilliseconds < 500.0 && Program.lastFileName == e.Name)
                return;

            watcherLastTime = DateTime.Now; // Обновляем время последнего изменения.
            try
            {
                Console.WriteLine("Mod changed! compiling.."); // Сообщаем о начале компиляции.
                Compile(); // Компилируем мод.
            }
            catch (Exception ex)
            {
                Console.WriteLine((object)ex); // Выводим ошибку, если компиляция не удалась.
            }
            lastFileName = e.Name; // Сохраняем имя измененного файла.
        }

        // Метод для компиляции мода.
        private static void Compile()
        {
            // Создаем провайдер для компиляции C# кода.
            CodeDomProvider codeDomProvider = (CodeDomProvider)new CSharpCodeProvider();
            // Получаем все CS-файлы в папке мода.
            string[] files = Directory.GetFiles(Program.folderPath, "*.cs");

            if (files.Length == 0)
                return; // Если файлов нет, завершаем компиляцию.

            // Ожидаем, пока файлы станут доступны для чтения.
            for (int index = 0; index < files.Length; ++index)
            {
                do;
                while (!Program.IsFileReady(files[index]));
            }

            // Компилируем сборку из CS-файлов.
            CompilerResults compilerResults = codeDomProvider.CompileAssemblyFromFile(Program.parameters, files);

            if (compilerResults.Errors.HasErrors)
            {
                // Если есть ошибки компиляции, выводим их в консоль.
                Console.WriteLine("Compilation error:");
                foreach (CompilerError error in (CollectionBase)compilerResults.Errors)
                    Console.WriteLine(string.Format("{0} > {1} - {2} {3}", error.FileName, error.Line, error.ErrorText, error.ErrorNumber));
            }
            else
            {
                // Если компиляция прошла успешно, выводим сообщение и воспроизводим звук.
                Console.WriteLine("Mod compilation completed successfully " + DateTime.Now.ToString("HH:mm:ss"));
                SystemSounds.Beep.Play();
            }
        }

        // Метод для проверки, доступен ли файл для чтения.
        public static bool IsFileReady(string filename)
        {
            try
            {
                // Пытаемся открыть файл для чтения.
                using (FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return fileStream.Length > 0; // Проверяем, что файл не пустой.
            }
            catch
            {
                return false; // Если файл недоступен, возвращаем false.
            }
        }
    }
}