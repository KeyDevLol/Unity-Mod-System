using System.IO;
using System.Reflection;
using UnityEngine;

public static class CsModAPI
{
    #region Full path resource loading methods

    //���� ����������� ��������� ����� ��� ����� (������ ���� �� �����)
    public static AudioClip LoadAudioFull(string fullPath)
    {
        if (File.Exists(fullPath) == false)
            throw new FileNotFoundException("Audio file not founded!", Path.GetFileNameWithoutExtension(fullPath));

        WWW www = new WWW(fullPath);

        while (www.isDone != true) { }

        return www.GetAudioClip();
    }

	//���� ����������� ��������� ������� ��� ����� (������ ���� �� �����)
	public static Sprite LoadSpriteFull(string fullPath, FilterMode mode = FilterMode.Bilinear, int pixelsPerUnit = 32)
    {
        Texture2D texture = LoadTextureFull(fullPath, mode);

        if (texture == null)
            return null;

        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), 0.5f * Vector2.one, pixelsPerUnit);
    }

	//���� ����������� ��������� �������� ��� ����� (������ ���� �� �����)
	public static Texture2D LoadTextureFull(string fullPath, FilterMode mode = FilterMode.Bilinear)
    {
        byte[] data;

        try
        {
            data = File.ReadAllBytes(fullPath);
        }
        catch { return null; }

        Texture2D texture = new Texture2D(0, 0);

        if (!texture.LoadImage(data))
        {
            throw new InvalidDataException("Texture load failed");
        }

        texture.filterMode = mode;
        texture.wrapMode = TextureWrapMode.Clamp;

        return texture;
    }

	#endregion

	#region Relative path resource loading methods

	//���� ����������� ��������� ����� ��� ����� (������������� �� ����� ���� ���� �� �����)
	public static AudioClip LoadAudio(string path)
    {
        string fullPath = Path.Combine(CsModsManager.mods[Assembly.GetCallingAssembly()].modDir, path);
        return LoadAudioFull(fullPath);
    }

	//���� ����������� ��������� ������� ��� ����� (������������� �� ����� ���� ���� �� �����)
	public static Sprite LoadSprite(string path, FilterMode mode = FilterMode.Bilinear, int pixelsPerUnit = 32)
    {
        string fullPath = Path.Combine(CsModsManager.mods[Assembly.GetCallingAssembly()].modDir, path);
        return LoadSpriteFull(fullPath, mode, pixelsPerUnit);
    }

	//���� ����������� ��������� �������� ��� ����� (������������� �� ����� ���� ���� �� �����)
	public static Texture2D LoadTexture(string path, FilterMode mode = FilterMode.Bilinear)
    {
        string fullPath = Path.Combine(CsModsManager.mods[Assembly.GetCallingAssembly()].modDir, path);

        byte[] data;

        try
        {
            data = File.ReadAllBytes(fullPath);
        }
        catch { return null; }

        Texture2D texture = new Texture2D(0, 0);

        if (!texture.LoadImage(data))
        {
            throw new InvalidDataException("Texture load failed");
        }

        texture.filterMode = mode;
        texture.wrapMode = TextureWrapMode.Clamp;

        return texture;
    }

    #endregion

    //���� ����������� ���� �������� ���� �� ����� �������� �����
    public static string GetModFolderPath()
    {
        return CsModsManager.mods[Assembly.GetCallingAssembly()].modDir;
    }
}
