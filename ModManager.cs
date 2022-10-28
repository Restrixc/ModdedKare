using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ModManager : MonoBehaviour
{
	private Dictionary<string, object> loadedAssets = new Dictionary<string, object>();

	public static async Task GetAll(string label, IList<IResourceLocation> loadedLocations)
	{
		foreach (IResourceLocation location in await Addressables.LoadResourceLocationsAsync(label).Task)
		{
			loadedLocations.Add(location);
		}
	}

	private AsyncOperationHandle<object> Load(string key)
	{
		AsyncOperationHandle<object> t = Addressables.LoadAssetAsync<object>(key);
		t.Completed += delegate(AsyncOperationHandle<object> asset)
		{
			if (loadedAssets.ContainsKey(key))
			{
				Addressables.Release(loadedAssets[key]);
			}
			loadedAssets[key] = asset;
		};
		return t;
	}

	public async void LoadAtStart()
	{
		List<IResourceLocation> resources = new List<IResourceLocation>();
		await GetAll("LoadAtStart", resources);
		foreach (IResourceLocation resource in resources)
		{
			Load(resource.PrimaryKey);
		}
	}

	public void Awake()
	{
		LoadAtStart();
	}

	public void OnDestroy()
	{
	}
}
