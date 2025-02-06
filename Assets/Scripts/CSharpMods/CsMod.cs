using System.Reflection;
using UnityEngine;

public class CsMod
{
	public Assembly modAssembly = null; // Сборка (DLL), содержащая код мода. Используется для доступа к классам и методам мода.
	public ModConfig config; // Конфигурация мода, загруженная из файла config.json. Содержит имя, описание, путь к иконке и автора мода.
	public string modDir; // Путь к директории мода, где находятся его файлы (DLL, конфигурация, ресурсы и т.д.).

	// Конструктор для инициализации мода.
	// Принимает сборку (DLL) и путь к директории мода.
	public CsMod(Assembly assembly, string modDir)
	{
		modAssembly = assembly; // Сохраняем сборку мода.
		this.modDir = modDir; // Сохраняем путь к директории мода.
	}

	// Метод для активации мода.
	// Вызывает метод OnStart из класса MainClass мода, если он существует.
	public void Activate()
	{
		try
		{
			// Получаем метод OnStart из класса MainClass мода с помощью рефлексии.
			MethodInfo method = modAssembly.GetType("Mod.MainClass").GetMethod("OnStart");

			if (method != null)
				method.Invoke(null, null); // Вызываем метод OnStart, если он найден.
		}
		catch (System.Exception exc)
		{
			// Логируем ошибку, если метод не найден или произошла ошибка при вызове.
			Debug.LogError($"Mod - {System.IO.Path.GetFileNameWithoutExtension(modAssembly.Location)} Error: {exc}");
		}
	}
}