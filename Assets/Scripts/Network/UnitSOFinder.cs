using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public abstract class UnitSOFinder<T> : ScriptableObject where T : UnitSOFinder<T> {

	public string uniqueName;

	private static Dictionary<string, T> _sODict;

	[RuntimeInitializeOnLoadMethod]
	protected static void Load() {
		T[] sOArray = Resources.LoadAll<T>(string.Empty);

		List<string> duplicates = sOArray.ToList().GroupBy(sOArray => sOArray.uniqueName)
			.Where(group => group.Count() > 1)
			.Select(group => group.Key).ToList();

		if (duplicates.Count == 0) {
			_sODict = sOArray.ToDictionary(sO => sO.uniqueName, sO => sO);

			Debug.Log(_sODict.Count + " \"" + typeof(T).FullName + "\" scriptable object" + (_sODict.Count == 0 || _sODict.Count > 1 ? "s" : string.Empty) + " have been loaded");

			return;
		}

		for (int i = 0; i < duplicates.Count; i++) {
			Debug.LogError("Multiple \"" + typeof(T).FullName + "\" scriptable objects with the unique name \"" + duplicates[i] + "\" have been found");
		}
	}

	public static T Get(string uniqueName) {
		return _sODict.TryGetValue(uniqueName, out T result) ? result : null;//?
	}

	/*public static T GetRandom() {
		string[] uniqueNameArray = new string[_sODict.Count];

		int i = 0;

		foreach (T sO in _sODict.Values) {
			uniqueNameArray[i] = sO.uniqueName;

			i++;
		}

		return _sODict[uniqueNameArray[Utils.random.Next(0, uniqueNameArray.Length)]];
	}*/
}
