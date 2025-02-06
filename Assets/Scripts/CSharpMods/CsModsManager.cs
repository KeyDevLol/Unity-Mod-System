using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class CsModsManager : MonoBehaviour
{
	public static Dictionary<Assembly, CsMod> mods; // ������� ��� �������� ���� ����������� �����. ���� � ������ ����, �������� � ������ CsMod.
	private string modFolderPath; // ���� � �����, ��� �������� ����.

	[SerializeField]
	private ModListItem modListPrefab; // ������ �������� ������ ����� ��� ����������� � �������� �����.
	[SerializeField]
	private Transform modScrollContent; // ������ Content � Scroll View ��� ���������� ��������� ������ �����.
	[SerializeField]
	private FilterMode iconFilterMode = FilterMode.Bilinear; // ����� ���������� ��� ������ �����.
	[SerializeField]
	private string authorPrefix = "�����"; // ������� ��� ����������� ������ ����.

	private void Awake()
	{
		if (mods != null)
		{
			// ���� ���� ��� ���������, ���������� �� � ������.
			foreach (CsMod mod in mods.Values)
			{
				ModConfig config = mod.config;
				string iconPath = Path.Combine(mod.modDir, config.iconPath); // �������� ���� � ������ ����.

				SpawnModListItem(config, iconPath); // ������� ������� ������ ��� ����.
			}

			return;
		}

		LoadMods(); // ��������� ����, ���� ��� ��� �� ���������.
	}

	// ��������� ��� ���� �� ����� "mods" � ����� ���� (��� �������, ���� ����������� �� ���������).
	public void LoadMods()
	{
		mods = new Dictionary<Assembly, CsMod>();

		// ���������� ���� � ����� � ������.
		modFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "mods");

		// ������� ����� "mods", ���� ��� �� ����������.
		if (Directory.Exists(modFolderPath) == false)
			Directory.CreateDirectory(modFolderPath);

		// �������� �� ���� �������������� � ����� "mods".
		foreach (string modDir in Directory.GetDirectories(modFolderPath))
		{
			string configPath = Path.Combine(modDir, "config.json"); // ���� � ����� ������������ ����.

			if (File.Exists(configPath) == false)
				return; // ���������� ���, ���� ������������ �����������.

			string[] files = Directory.GetFiles(modDir, "*.dll"); // ���� DLL-����� � ���������� ����.

			if (files.Length > 0)
			{
				// ��������� ������ ����.
				Assembly asm = Assembly.Load(File.ReadAllBytes(files[0]));

				if (mods.ContainsKey(asm))
					continue; // ���������� ���, ���� �� ��� ��������.

				// ������� ������ CsMod ��� ����.
				CsMod mod = new CsMod(asm, modDir);
				mods.Add(asm, mod);

				// ��������� ������������ ���� �� ����� config.json.
				ModConfig config = JsonUtility.FromJson<ModConfig>(File.ReadAllText(configPath));
				string iconPath = Path.Combine(modDir, config.iconPath); // ���� � ������ ����.

				mod.config = config; // ��������� ������������ � ������� ����.

				SpawnModListItem(config, iconPath); // ������� ������� ������ ��� ����.
			}
		}
	}

	// ������� ������� ������ ����� � �������� �����.
	public void SpawnModListItem(ModConfig config, string iconPath)
	{
		// ������� ������ �������� ������ �� �������.
		GameObject listObj = GameObject.Instantiate(modListPrefab.gameObject, modScrollContent);

		// �������� ��������� ModListItem ��� ���������.
		ModListItem listItem = listObj.GetComponent<ModListItem>();

		// ��������� ������ ���� � ����������� ������� ������.
		listItem.icon.sprite = CsModAPI.LoadSpriteFull(iconPath, iconFilterMode);
		listItem.modName.text = config.name;
		listItem.description.text = config.description;
		listItem.author.text = $"{authorPrefix} {config.author}";
	}

	private void Start()
	{
		// ���������� ��� ���� ����� �������� �����.
		foreach (CsMod mod in mods.Values)
		{
			mod.Activate();
		}
	}
}