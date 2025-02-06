using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LuaModsManager : MonoBehaviour
{
	private static List<LuaMod> mods; // Список для хранения Lua-модов.
	private string modFolderPath; // Путь к папке с модами.

	[SerializeField]
	private ModListItem modListPrefab; // Префаб элемента списка модов для отображения в панельке модов.
	[SerializeField]
	private Transform modScrollContent; // Объект Content в Scroll View для размещения элементов списка модов.
	[SerializeField]
	private FilterMode iconFilterMode = FilterMode.Bilinear; // Режим фильтрации для иконок модов.
	[SerializeField]
	private string authorPrefix = "Автор"; // Префикс для отображения автора мода.

	private void Awake()
	{
		if (mods != null)
		{
			// Если моды уже загружены, отображаем их в списке.
			foreach (LuaMod luaMod in mods)
			{
				ModConfig config = luaMod.config;
				string iconPath = Path.Combine(luaMod.modDir, config.iconPath); // Получаем путь к иконке мода.

				SpawnModListItem(config, iconPath); // Создаем элемент списка для мода.
			}

			return;
		}

		LoadMods(); // Загружаем моды, если они еще не загружены.
	}

	// Перезагружает все Lua-моды.
	public void ReloadMods()
	{
		mods.Clear(); // Очищаем список модов.
		mods = null; // Сбрасываем список модов.

		LoadMods(); // Загружаем моды заново.

		// Перезагружаем сцену для применения изменений.
		UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
	}

	// Загружает все Lua-моды из папки "mods".
	private void LoadMods()
	{
		mods = new List<LuaMod>();
		modFolderPath = Path.Combine(Environment.CurrentDirectory, "mods"); // Путь к папке с модами.

		// Создаем папку "mods", если она не существует.
		if (Directory.Exists(modFolderPath) == false)
			Directory.CreateDirectory(modFolderPath);

		// Проходим по всем поддиректориям в папке "mods".
		foreach (string modDir in Directory.GetDirectories(modFolderPath))
		{
			string configPath = Path.Combine(modDir, "config.json"); // Путь к файлу конфигурации мода.

			if (File.Exists(configPath) == false)
				return; // Пропускаем мод, если конфигурация отсутствует.

			string[] scriptFiles = Directory.GetFiles(modDir, "*.lua*", SearchOption.AllDirectories); // Ищем Lua-файлы в директории мода.

			if (scriptFiles.Length > 0)
			{
				// Создаем объект LuaMod для мода.
				LuaMod mod = new LuaMod(Path.GetDirectoryName(modDir), modDir, scriptFiles);

				// Загружаем конфигурацию мода из файла config.json.
				ModConfig config = JsonUtility.FromJson<ModConfig>(File.ReadAllText(configPath));
				string iconPath = Path.Combine(modDir, config.iconPath); // Путь к иконке мода.

				mod.config = config; // Сохраняем конфигурацию в объекте мода.

				mods.Add(mod); // Добавляем мод в список.

				SpawnModListItem(config, iconPath); // Создаем элемент списка для мода.
			}
		}
	}

	// Создает элемент списка модов в панельке модов.
	public void SpawnModListItem(ModConfig config, string iconPath)
	{
		// Создаем объект элемента списка из префаба.
		GameObject listObj = GameObject.Instantiate(modListPrefab.gameObject, modScrollContent);

		// Получаем компонент ModListItem для настройки.
		ModListItem listItem = listObj.GetComponent<ModListItem>();

		// Загружаем иконку мода и настраиваем элемент списка.
		listItem.icon.sprite = LoadSpriteFull(iconPath, iconFilterMode);
		listItem.modName.text = config.name;
		listItem.description.text = config.description;
		listItem.author.text = $"{authorPrefix} {config.author}";
	}


	// Загружает спрайт по полному пути.
	private Sprite LoadSpriteFull(string fullPath, FilterMode mode = FilterMode.Bilinear, int pixelsPerUnit = 32)
	{
		Texture2D texture = LoadTextureFull(fullPath, mode); // Загружаем текстуру.

		if (texture == null)
			return null;

		// Создаем спрайт из текстуры.
		return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), 0.5f * Vector2.one, pixelsPerUnit);
	}

	// Загружает текстуру по полному пути.
	public Texture2D LoadTextureFull(string fullPath, FilterMode mode = FilterMode.Bilinear)
	{
		byte[] data;

		try
		{
			data = File.ReadAllBytes(fullPath); // Читаем байты из файла.
		}
		catch { return null; } // Если произошла ошибка, возвращаем null.

		Texture2D texture = new Texture2D(0, 0);

		if (!texture.LoadImage(data))
		{
			throw new InvalidDataException("Texture load failed"); // Если загрузка не удалась, выбрасываем исключение.
		}

		// Настраиваем фильтрацию и режим обертывания текстуры.
		texture.filterMode = mode;
		texture.wrapMode = TextureWrapMode.Clamp;

		return texture;
	}

	private void Start()
	{
		// Вызываем метод CallStart для всех Lua-модов после загрузки сцены.
		foreach (LuaMod mod in mods)
		{
			mod.CallStart();
		}
	}

	private void Update()
	{
		// Вызываем метод CallUpdate для всех Lua-модов каждый кадр.
		foreach (LuaMod mod in mods)
		{
			mod.CallUpdate();
		}
	}
}
