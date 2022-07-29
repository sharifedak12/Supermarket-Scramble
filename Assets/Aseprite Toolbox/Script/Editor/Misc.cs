namespace AsepriteToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using AsepriteToolbox.Saving;




	// Preset
	[System.Serializable]
	public class ConfigPreset {



		// SUB
		[System.Serializable]
		public struct KeyValueType {

			public string Key;
			public string Value;
			public DataType Type;

			public KeyValueType (EditorSavingBool saving) {
				Key = saving.Key;
				Value = saving.Value.ToString();
				Type = DataType.Bool;
			}

			public KeyValueType (EditorSavingColor saving) {
				Key = saving.Key;
				Value = string.Format(
					"{0}\n{1}\n{2}\n{3}",
					saving.Value.r.ToString(),
					saving.Value.g.ToString(),
					saving.Value.b.ToString(),
					saving.Value.a.ToString()
				);
				Type = DataType.Color;
			}

			public KeyValueType (EditorSavingFloat saving) {
				Key = saving.Key;
				Value = saving.Value.ToString();
				Type = DataType.Float;
			}

			public KeyValueType (EditorSavingInt saving) {
				Key = saving.Key;
				Value = saving.Value.ToString();
				Type = DataType.Int;
			}

			public KeyValueType (EditorSavingString saving) {
				Key = saving.Key;
				Value = saving.Value;
				Type = DataType.String;
			}

			public KeyValueType (EditorSavingVector2 saving) {
				Key = saving.Key;
				Value = string.Format(
					"{0}\n{1}",
					saving.Value.x.ToString(),
					saving.Value.y.ToString()
				);
				Type = DataType.Vector2;
			}

			public KeyValueType (EditorSavingVector3 saving) {
				Key = saving.Key;
				Value = string.Format(
					"{0}\n{1}\n{2}",
					saving.Value.x.ToString(),
					saving.Value.y.ToString(),
					saving.Value.z.ToString()
				);
				Type = DataType.Vector3;
			}

		}


		public struct NameCode {
			public string Name;
			public string Code;
			public NameCode (string n, string c) {
				Name = n;
				Code = c;
			}
		}


		[System.Serializable]
		public enum DataType {
			Float = 0,
			Int = 1,
			String = 2,
			Bool = 3,
			Color = 4,
			Vector2 = 5,
			Vector3 = 6,

		}




		public class NameSorter : IComparer<NameCode> {
			public int Compare (NameCode x, NameCode y) {
				return x.Name.CompareTo(y.Name);
			}
		}


		// Global
		private static string PresetPath = "";
		private readonly static List<NameCode> PresetNames = new List<NameCode>();


		// Short
		public static int PresetCount {
			get {
				return PresetNames.Count;
			}
		}

		public KeyValueType[] Datas {
			get {
				return m_Datas;
			}
			private set {
				m_Datas = value;
			}
		}


		// Ser
		[SerializeField] private KeyValueType[] m_Datas = null;
		[SerializeField] private string m_Code = "";


		// API
		public ConfigPreset (List<KeyValueType> dataList, string code) {
			Datas = dataList.ToArray();
			m_Code = code;
		}


		public static void RefreshPreset () {
			// Preset Path
			if (string.IsNullOrEmpty(PresetPath) || !Util.DirectoryExists(PresetPath)) {
				PresetPath = Util.CombinePaths(Util.GetRootPath(), "Preset");
			}
			// Preset Names/Codes
			PresetNames.Clear();
			if (Util.DirectoryExists(PresetPath)) {
				var files = Util.GetFilesIn(PresetPath, "*.json");
				if (files != null) {
					for (int i = 0; i < files.Length; i++) {
						var file = files[i];
						if (Util.GetExtension(file.FullName) == ".json") {
							string code = "";
							try {
								var preset = JsonUtility.FromJson<ConfigPreset>(Util.Read(file.FullName));
								if (preset != null) {
									code = preset.m_Code;
								}
							} catch { }
							PresetNames.Add(new NameCode(Util.GetNameWithoutExtension(file.FullName), code));
						}
					}
				}
			}
			PresetNames.Sort(new NameSorter());
		}


		public static ConfigPreset GetPresetFromDisk (string name) {
			string path = Util.CombinePaths(PresetPath, name + ".json");
			if (Util.FileExists(path)) {
				try {
					return JsonUtility.FromJson<ConfigPreset>(Util.Read(path));
				} catch { }
			}
			return null;
		}


		public static string GetPresetName (int index) {
			return PresetNames[index].Name;
		}


		public static string GetPresetCode (int index) {
			return PresetNames[index].Code;
		}


		public static void ImportPreset (ConfigPreset preset, string name) {
			try {
				if (preset == null || string.IsNullOrEmpty(PresetPath)) { return; }
				string newPath = Util.FixPath(Util.CombinePaths(PresetPath, name + ".json"));
				Util.Write(JsonUtility.ToJson(preset, true), newPath);
				RefreshPreset();
				AssetDatabase.Refresh();
			} catch { }
		}


		public static void DeletePresetFromDisk (string name) {
			var path = Util.CombinePaths(PresetPath, name + ".json");
			if (Util.FileExists(path)) {
				Util.DeleteFile(path);
				RefreshPreset();
				AssetDatabase.Refresh();
			}
		}


		public static bool PresetExists (string name) {
			return Util.FileExists(Util.CombinePaths(PresetPath, name + ".json"));
		}


		public void Apply () {
			if (Datas == null) { return; }
			foreach (var data in Datas) {
				if (string.IsNullOrEmpty(data.Key) || string.IsNullOrEmpty(data.Value)) { continue; }
				switch (data.Type) {
					default:
						break;
					case DataType.Bool: {
							bool result;
							if (bool.TryParse(data.Value, out result)) {
								EditorPrefs.SetBool(data.Key, result);
							}
						}
						break;
					case DataType.Float: {
							float result;
							if (float.TryParse(data.Value, out result)) {
								EditorPrefs.SetFloat(data.Key, result);
							}
							break;
						}
					case DataType.Int: {
							int result;
							if (int.TryParse(data.Value, out result)) {
								EditorPrefs.SetInt(data.Key, result);
							}
							break;
						}
					case DataType.String: {
							EditorPrefs.SetString(data.Key, data.Value);
							break;
						}
					case DataType.Vector2: {
							var strs = data.Value.Split('\n');
							if (strs != null && strs.Length >= 2) {
								float x, y;
								if (float.TryParse(strs[0], out x) && float.TryParse(strs[1], out y)) {
									EditorPrefs.SetFloat(data.Key + ".x", x);
									EditorPrefs.SetFloat(data.Key + ".y", y);
								}
							}
							break;
						}
					case DataType.Vector3: {
							var strs = data.Value.Split('\n');
							if (strs != null && strs.Length >= 3) {
								float x, y, z;
								if (float.TryParse(strs[0], out x) && float.TryParse(strs[1], out y) && float.TryParse(strs[2], out z)) {
									EditorPrefs.SetFloat(data.Key + ".x", x);
									EditorPrefs.SetFloat(data.Key + ".y", y);
									EditorPrefs.SetFloat(data.Key + ".z", z);
								}
							}
							break;
						}
					case DataType.Color: {
							var strs = data.Value.Split('\n');
							if (strs != null && strs.Length >= 4) {
								float r, g, b, a;
								if (float.TryParse(strs[0], out r) && float.TryParse(strs[1], out g) && float.TryParse(strs[2], out b) && float.TryParse(strs[3], out a)) {
									EditorPrefs.SetFloat(data.Key + ".r", r);
									EditorPrefs.SetFloat(data.Key + ".g", g);
									EditorPrefs.SetFloat(data.Key + ".b", b);
									EditorPrefs.SetFloat(data.Key + ".a", a);
								}
							}
							break;
						}
				}

			}
		}


	}




}