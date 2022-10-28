using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveManager
{
	public delegate void SaveCompleteAction();

	public class SaveData
	{
		public readonly string imageLocation;

		public readonly Texture2D image;

		public readonly string fileName;

		public readonly DateTime time;

		public SaveData(string fileName, DateTime time)
		{
			this.fileName = fileName;
			imageLocation = fileName.Substring(0, fileName.Length - 4) + ".jpg";
			this.time = time;
			image = new Texture2D(16, 16);
			if (File.Exists(imageLocation))
			{
				image.LoadImage(File.ReadAllBytes(imageLocation));
			}
		}
	}

	public const string saveDataLocation = "saves/";

	public const string saveExtension = ".sav";

	public const string imageExtension = ".jpg";

	public const string saveHeader = "KKSAVE";

	public const int textureSize = 256;

	private static List<SaveData> saveDatas = new List<SaveData>();

	public static void Init()
	{
		string saveDataPath = Application.persistentDataPath + "/saves/";
		if (!Directory.Exists(saveDataPath))
		{
			Directory.CreateDirectory(saveDataPath);
		}
		saveDatas.Clear();
		foreach (string fileName in Directory.EnumerateFiles(saveDataPath))
		{
			if (fileName.EndsWith(".sav"))
			{
				saveDatas.Add(new SaveData(fileName, File.GetCreationTime(fileName)));
			}
		}
	}

	public static List<SaveData> GetSaveDatas()
	{
		return new List<SaveData>(saveDatas);
	}

	private static string PrefabifyGameObjectName(GameObject obj)
	{
		string name = obj.name;
		if (name.Contains("("))
		{
			return name.Split('(')[0].Trim();
		}
		return name;
	}

	public static bool IsLoadable(string filename, out string lastError)
	{
		using FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
		BinaryReader reader = new BinaryReader(file);
		if (reader.ReadString() != "KKSAVE")
		{
			lastError = "Not a save file: " + filename;
			return false;
		}
		string fileVersion = reader.ReadString();
		if (fileVersion != PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion)
		{
			lastError = "Cannot load save file, it was saved with a different version of KoboldKare.";
			return false;
		}
		lastError = "";
		return true;
	}

	public static void Save(string filename, SaveCompleteAction action = null)
	{
		string saveDataPath = Application.persistentDataPath + "/saves/";
		string savePath = string.Format("{0}{1}{2}", saveDataPath, filename, ".sav");
		if (!Directory.Exists(saveDataPath))
		{
			Directory.CreateDirectory(saveDataPath);
		}
		using (FileStream file = new FileStream(savePath, FileMode.CreateNew, FileAccess.Write))
		{
			BinaryWriter writer = new BinaryWriter(file);
			writer.Write("KKSAVE");
			writer.Write(PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion);
			int viewCount = 0;
			foreach (PhotonView view3 in PhotonNetwork.PhotonViewCollection)
			{
				if (!view3.name.Contains("DontSave"))
				{
					viewCount++;
				}
			}
			writer.Write(viewCount);
			PhotonView[] array = UnityEngine.Object.FindObjectsOfType<PhotonView>(includeInactive: true);
			foreach (PhotonView view in array)
			{
				if (!view.gameObject.activeInHierarchy && !((DefaultPool)PhotonNetwork.PrefabPool).ResourceCache.ContainsKey(PrefabifyGameObjectName(view.gameObject)))
				{
					GameObject gameObject = view.gameObject;
					Debug.LogError($"Found a disabled static viewID {view.ViewID} {gameObject.name}, this is not allowed as it prevents unique id assignments!", gameObject);
					return;
				}
			}
			foreach (PhotonView view2 in PhotonNetwork.PhotonViewCollection)
			{
				if (view2.name.Contains("DontSave"))
				{
					continue;
				}
				writer.Write(view2.ViewID);
				writer.Write(PrefabifyGameObjectName(view2.gameObject));
				foreach (Component observable in view2.ObservedComponents)
				{
					if (observable is ISavable savable)
					{
						savable.Save(writer);
					}
				}
			}
		}
		string imageSavePath = string.Format("{0}{1}{2}", saveDataPath, filename, ".jpg");
		Screenshotter.GetScreenshot(delegate(Texture2D texture)
		{
			if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}
			using (FileStream fileStream = new FileStream(imageSavePath, FileMode.CreateNew, FileAccess.Write))
			{
				byte[] array2 = texture.EncodeToJPG();
				fileStream.Write(array2, 0, array2.Length);
			}
			saveDatas.Add(new SaveData(savePath, File.GetCreationTime(savePath)));
			action?.Invoke();
		});
	}

	public static bool RemoveSave(string fileName)
	{
		string saveDataPath = Application.persistentDataPath + "/saves/";
		string imageSavePath = string.Format("{0}{1}", fileName.Substring(0, fileName.Length - 4), ".jpg");
		if (!File.Exists(fileName))
		{
			Debug.LogWarning("Indicated save file doesn't exist! (" + fileName + ") Should remove from UI rather than disk. TODO: Callback.");
			return false;
		}
		File.Delete(fileName);
		File.Delete(imageSavePath);
		return true;
	}

	private static void CleanUpImmediate()
	{
		PhotonView[] array = UnityEngine.Object.FindObjectsOfType<PhotonView>(includeInactive: true);
		foreach (PhotonView view in array)
		{
			if (((DefaultPool)PhotonNetwork.PrefabPool).ResourceCache.ContainsKey(PrefabifyGameObjectName(view.gameObject)))
			{
				PhotonNetwork.Destroy(view.gameObject);
			}
		}
	}

	private static void LoadImmediate(string filename)
	{
		if (SingletonScriptableObject<NetworkManager>.instance.online)
		{
			return;
		}
		CleanUpImmediate();
		using FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
		BinaryReader reader = new BinaryReader(file);
		if (reader.ReadString() != "KKSAVE")
		{
			throw new UnityException("Not a save file: " + filename);
		}
		string fileVersion = reader.ReadString();
		if (fileVersion != PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion)
		{
			throw new UnityException("Cannot load save file, it was saved with a different version of KoboldKare.");
		}
		int viewCount = reader.ReadInt32();
		for (int i = 0; i < viewCount; i++)
		{
			int viewID = reader.ReadInt32();
			string prefabName = reader.ReadString();
			PhotonView view = PhotonNetwork.GetPhotonView(viewID);
			if (((DefaultPool)PhotonNetwork.PrefabPool).ResourceCache.ContainsKey(prefabName))
			{
				GameObject obj = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity, 0);
				view = obj.GetComponent<PhotonView>();
			}
			if (view == null)
			{
				Debug.Log("[SaveManager] <Deserialization Log> :: Running deep check when view returned null...");
				PhotonView[] array = UnityEngine.Object.FindObjectsOfType<PhotonView>(includeInactive: true);
				foreach (PhotonView deepCheck in array)
				{
					if (deepCheck.ViewID == viewID)
					{
						view = deepCheck;
						Debug.Log("[SaveManager] <Deserialization Log> :: Deep check successful!");
						break;
					}
				}
			}
			if (view == null)
			{
				throw new UnityException($"Failed to find view id {viewID} with name {prefabName}. Failed to load...");
			}
			try
			{
				foreach (Component observable in view.ObservedComponents)
				{
					if (observable is ISavable savable)
					{
						savable.Load(reader);
					}
				}
			}
			catch
			{
				Debug.LogError($"Failed to load observable on photonView {viewID}, {prefabName}", view);
				throw;
			}
		}
	}

	private static IEnumerator MakeSureMapIsLoadedThenLoadSave(string filename)
	{
		if (!IsLoadable(filename, out var lastError))
		{
			throw new UnityException(lastError);
		}
		if (SceneManager.GetActiveScene().name != "MainMenu")
		{
			GameManager.instance.Pause(pause: false);
			GameManager.instance.loadListener.Show();
		}
		if (SceneManager.GetActiveScene().name != "MainMap")
		{
			yield return SingletonScriptableObject<NetworkManager>.instance.SinglePlayerRoutine();
		}
		yield return new WaitForSecondsRealtime(1f);
		try
		{
			LoadImmediate(filename);
		}
		catch
		{
			GameManager.instance.loadListener.Hide();
			throw;
		}
		if (SceneManager.GetActiveScene().name != "MainMenu")
		{
			GameManager.instance.loadListener.Hide();
		}
	}

	public static void Load(string filename)
	{
		GameManager.instance.StartCoroutine(MakeSureMapIsLoadedThenLoadSave(filename));
	}
}
