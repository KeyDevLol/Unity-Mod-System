using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LuaModsManager : MonoBehaviour
{
	private static List<LuaMod> mods; // ������ ��� �������� Lua-�����.
	private string modFolderPath; // ���� � ����� � ������.

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
			foreach (LuaMod luaMod in mods)
			{
				ModConfig config = luaMod.config;
				string iconPath = Path.Combine(luaMod.modDir, config.iconPath); // �������� ���� � ������ ����.

				SpawnModListItem(config, iconPath); // ������� ������� ������ ��� ����.
			}

			return;
		}

		LoadMods(); // ��������� ����, ���� ��� ��� �� ���������.
	}

	// ������������� ��� Lua-����.
	public void ReloadMods()
	{
		mods.Clear(); // ������� ������ �����.
		mods = null; // ���������� ������ �����.

		LoadMods(); // ��������� ���� ������.

		// ������������� ����� ��� ���������� ���������.
		UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
	}

	// ��������� ��� Lua-���� �� ����� "mods".
	private void LoadMods()
	{
		mods = new List<LuaMod>();
		modFolderPath = Path.Combine(Environment.CurrentDirectory, "mods"); // ���� � ����� � ������.

		// ������� ����� "mods", ���� ��� �� ����������.
		if (Directory.Exists(modFolderPath) == false)
			Directory.CreateDirectory(modFolderPath);

		// �������� �� ���� �������������� � ����� "mods".
		foreach (string modDir in Directory.GetDirectories(modFolderPath))
		{
			string configPath = Path.Combine(modDir, "config.json"); // ���� � ����� ������������ ����.

			if (File.Exists(configPath) == false)
				return; // ���������� ���, ���� ������������ �����������.

			string[] scriptFiles = Directory.GetFiles(modDir, "*.lua*", SearchOption.AllDirectories); // ���� Lua-����� � ���������� ����.

			if (scriptFiles.Length > 0)
			{
				// ������� ������ LuaMod ��� ����.
				LuaMod mod = new LuaMod(Path.GetDirectoryName(modDir), modDir, scriptFiles);

				// ��������� ������������ ���� �� ����� config.json.
				ModConfig config = JsonUtility.FromJson<ModConfig>(File.ReadAllText(configPath));
				string iconPath = Path.Combine(modDir, config.iconPath); // ���� � ������ ����.

				mod.config = config; // ��������� ������������ � ������� ����.

				mods.Add(mod); // ��������� ��� � ������.

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
		listItem.icon.sprite = LoadSpriteFull(iconPath, iconFilterMode);
		listItem.modName.text = config.name;
		listItem.description.text = config.description;
		listItem.author.text = $"{authorPrefix} {config.author}";
	}


	// ��������� ������ �� ������� ����.
	private Sprite LoadSpriteFull(string fullPath, FilterMode mode = FilterMode.Bilinear, int pixelsPerUnit = 32)
	{
		Texture2D texture = LoadTextureFull(fullPath, mode); // ��������� ��������.

		if (texture == null)
			return null;

		// ������� ������ �� ��������.
		return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), 0.5f * Vector2.one, pixelsPerUnit);
	}

	// ��������� �������� �� ������� ����.
	public Texture2D LoadTextureFull(string fullPath, FilterMode mode = FilterMode.Bilinear)
	{
		byte[] data;

		try
		{
			data = File.ReadAllBytes(fullPath); // ������ ����� �� �����.
		}
		catch { return null; } // ���� ��������� ������, ���������� null.

		Texture2D texture = new Texture2D(0, 0);

		if (!texture.LoadImage(data))
		{
			throw new InvalidDataException("Texture load failed"); // ���� �������� �� �������, ����������� ����������.
		}

		// ����������� ���������� � ����� ����������� ��������.
		texture.filterMode = mode;
		texture.wrapMode = TextureWrapMode.Clamp;

		return texture;
	}

	private void Start()
	{
		// �������� ����� CallStart ��� ���� Lua-����� ����� �������� �����.
		foreach (LuaMod mod in mods)
		{
			mod.CallStart();
		}
	}

	private void Update()
	{
		// �������� ����� CallUpdate ��� ���� Lua-����� ������ ����.
		foreach (LuaMod mod in mods)
		{
			mod.CallUpdate();
		}
	}
}
