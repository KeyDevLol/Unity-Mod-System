using UnityEngine;
using MoonSharp.Interpreter; // Используем библиотеку MoonSharp для работы с Lua.
using System.IO;
using System;

public class LuaMod
{
	public readonly string name; // Название мода (только для чтения).
	public readonly string modDir; // Путь к директории мода.
	public ModConfig config; // Конфигурация мода, загруженная из файла config.json.
	private LuaScript[] scripts; // Массив Lua-скриптов, принадлежащих этому моду.

	// Конструктор для инициализации Lua-мода.
	// Принимает имя мода, путь к директории мода и массив путей к Lua-скриптам.
	public LuaMod(string name, string modDir, string[] luaScriptsPath)
	{
		this.name = name; // Сохраняем имя мода.
		this.modDir = modDir; // Сохраняем путь к директории мода.

		// Настраиваем вывод отладочных сообщений Lua в Unity-лог.
		Script.DefaultOptions.DebugPrint = Debug.Log;

		// Инициализируем массив Lua-скриптов.
		scripts = new LuaScript[luaScriptsPath.Length];

		// Загружаем каждый Lua-скрипт.
		for (int i = 0; i < luaScriptsPath.Length; i++)
		{
			scripts[i] = new LuaScript(luaScriptsPath[i]);
		}
	}

	// Вызывает метод start для всех Lua-скриптов мода.
	public void CallStart()
	{
		foreach (LuaScript script in scripts)
		{
			script.CallStart();
		}
	}

	// Вызывает метод update для всех Lua-скриптов мода.
	public void CallUpdate()
	{
		foreach (LuaScript script in scripts)
		{
			script.CallUpdate();
		}
	}

	// Внутренний класс для работы с отдельным Lua-скриптом.
	private class LuaScript
	{
		public Script script; // Объект MoonSharp Script для выполнения Lua-кода.
		public string path; // Путь к файлу Lua-скрипта.
		private object startFunc; // Функция start из Lua-скрипта.
		private object updateFunc; // Функция update из Lua-скрипта.

		// Конструктор для инициализации Lua-скрипта.
		// Принимает путь к файлу Lua-скрипта.
		public LuaScript(string path)
		{
			this.path = path; // Сохраняем путь к файлу.

			// Создаем новый объект Script для выполнения Lua-кода.
			script = new Script();

			// Загружаем и выполняем Lua-скрипт из файла.
			script.DoString(File.ReadAllText(path, System.Text.Encoding.UTF8));

			// Получаем функции start и update из глобальной области видимости Lua-скрипта.
			startFunc = script.Globals["start"];
			updateFunc = script.Globals["update"];

			// Инициализируем API для Lua-скрипта.
			InitAPI();
		}

		// Инициализация API для Lua-скрипта.
		private void InitAPI()
		{
			// Здесь можно добавить свои методы и функции, которые будут доступны в Lua-скриптах.
			// Пример добавления метода changeColor, который изменяет цвет объекта "Square" на magenta.
			script.Globals["changeColor"] = (Action)delegate ()
			{
				GameObject.Find("Square")
				.GetComponent<SpriteRenderer>().color = Color.magenta;
			};
		}

		// Вызывает функцию start из Lua-скрипта, если она существует.
		public void CallStart()
		{
			if (startFunc != null) script.Call(startFunc);
		}

		// Вызывает функцию update из Lua-скрипта, если она существует.
		public void CallUpdate()
		{
			if (updateFunc != null) script.Call(updateFunc);
		}
	}
}