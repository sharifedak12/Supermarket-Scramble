namespace AsepriteToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using System.Linq;


	public class FileRecorder {


		// SUB
		private struct Task {
			public string Name;
			public string ObjectPath;
			public string MiddlePath;

		}


		// Short
		public int FolderCount {
			get {
				return _FolderCount;
			}
		}

		public int ItemCount {
			get {
				return _ItemCount;
			}
		}

		public int TaskCount {
			get {
				return TaskMap.Count;
			}
		}



		// Data
		private readonly Dictionary<Object, Task> TaskMap = new Dictionary<Object, Task>();
		private readonly Dictionary<string, int> CountMap = new Dictionary<string, int>();
		private int _FolderCount = 0;
		private int _ItemCount = 0;


		// API
		public FileRecorder (params string[] extensions) {
			foreach (var ex in extensions) {
				if (!CountMap.ContainsKey(ex)) {
					CountMap.Add(ex, 0);
				}
			}
		}


		public void SetSelection (Object[] objs) {
			TaskMap.Clear();
			_FolderCount = 0;
			_ItemCount = objs.Length;
			string[] searchExs = new string[CountMap.Count];
			int index = 0;
			foreach (var key in CountMap.Keys.ToList()) {
				CountMap[key] = 0;
				searchExs[index] = "*" + key;
				index++;
			}
			foreach (var obj in objs) {
				if (TaskMap.ContainsKey(obj)) { continue; }
				var path = AssetDatabase.GetAssetPath(obj);
				if (string.IsNullOrEmpty(path)) { continue; }
				if (Util.PathIsDirectory(path)) {
					// Folder
					var files = Util.GetFilesIn(path, searchExs);
					foreach (var file in files) {
						var _path = Util.FixedRelativePath(file.FullName);
						var name = Util.GetNameWithoutExtension(_path);
						var _ex = Util.GetExtension(_path);
						var _obj = AssetDatabase.LoadAssetAtPath<Object>(_path);
						if (!_obj || TaskMap.ContainsKey(_obj)) { continue; }
						if (CountMap.ContainsKey(_ex)) {
							TaskMap.Add(_obj, new Task() {
								ObjectPath = _path,
								MiddlePath = GetMiddlePath(path, _path),
								Name = name,
							});
							CountMap[_ex]++;
						}
					}
					_FolderCount++;
				} else {
					// File
					var ex = Util.GetExtension(path);
					var name = Util.GetNameWithoutExtension(path);
					if (CountMap.ContainsKey(ex)) {
						TaskMap.Add(obj, new Task() {
							ObjectPath = path,
							MiddlePath = "",
							Name = name,
						});
						CountMap[ex]++;
					}
				}
			}
		}


		public void ForAllTasks (System.Action<Object, string, string, string> action) {
			if (action == null) { return; }
			foreach (var task in TaskMap) {
				action(task.Key, task.Value.ObjectPath, task.Value.MiddlePath, task.Value.Name);
			}
		}


		public void ForAllExtensions (System.Action<string, int> action) {
			if (action == null) { return; }
			foreach (var ex in CountMap) {
				action(ex.Key, ex.Value);
			}
		}


		public int GetExtensionCount (string ex) {
			return CountMap.ContainsKey(ex) ? CountMap[ex] : 0;
		}


		// LGC
		private string GetMiddlePath (string folderPath, string filePath) {
			string midPath = "";
			folderPath = Util.FixPath(Util.GetParentPath(folderPath));
			filePath = Util.FixPath(filePath);
			if (Util.IsChildPathCompar(filePath, folderPath) || folderPath.Length == 0) {
				int midLen = filePath.Length - folderPath.Length - Util.GetNameWithExtension(filePath).Length;
				if (midLen > 0) {
					midPath = Util.FixPath(
						filePath.Substring(folderPath.Length, midLen)
					);
				}
			}
			return midPath;
		}


	}
}