using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class CsModsManager : MonoBehaviour
{
	public static Dictionary<Assembly, CsMod> mods; // Словарь для хранения всех загруженных модов. Ключ — сборка мода, значение — объект CsMod.
	private string modFolderPath; // Путь к папке, где хранятся моды.

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
			foreach (CsMod mod in mods.Values)
			{
				ModConfig config = mod.config;
				string iconPath = Path.Combine(mod.modDir, config.iconPath); // Получаем путь к иконке мода.

				SpawnModListItem(config, iconPath); // Создаем элемент списка для мода.
			}

			return;
		}

		LoadMods(); // Загружаем моды, если они еще не загружены.
	}

	// Загружает все моды из папки "mods" в корне игры (Или проекта, если запускается из редактора).
	public void LoadMods()
	{
		mods = new Dictionary<Assembly, CsMod>();

		// Определяем путь к папке с модами.
		modFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "mods");

		// Создаем папку "mods", если она не существует.
		if (Directory.Exists(modFolderPath) == false)
			Directory.CreateDirectory(modFolderPath);

		// Проходим по всем поддиректориям в папке "mods".
		foreach (string modDir in Directory.GetDirectories(modFolderPath))
		{
			string configPath = Path.Combine(modDir, "config.json"); // Путь к файлу конфигурации мода.

			if (File.Exists(configPath) == false)
				return; // Пропускаем мод, если конфигурация отсутствует.

			string[] files = Directory.GetFiles(modDir, "*.dll"); // Ищем DLL-файлы в директории мода.

			if (files.Length > 0)
			{
				// Загружаем сборку мода.
				Assembly asm = Assembly.Load(File.ReadAllBytes(files[0]));

				if (mods.ContainsKey(asm))
					continue; // Пропускаем мод, если он уже загружен.

				// Создаем объект CsMod для мода.
				CsMod mod = new CsMod(asm, modDir);
				mods.Add(asm, mod);

				// Загружаем конфигурацию мода из файла config.json.
				ModConfig config = JsonUtility.FromJson<ModConfig>(File.ReadAllText(configPath));
				string iconPath = Path.Combine(modDir, config.iconPath); // Путь к иконке мода.

				mod.config = config; // Сохраняем конфигурацию в объекте мода.

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
		listItem.icon.sprite = CsModAPI.LoadSpriteFull(iconPath, iconFilterMode);
		listItem.modName.text = config.name;
		listItem.description.text = config.description;
		listItem.author.text = $"{authorPrefix} {config.author}";
	}

	private void Start()
	{
		// Активируем все моды после загрузки сцены.
		foreach (CsMod mod in mods.Values)
		{
			mod.Activate();
		}
	}
}