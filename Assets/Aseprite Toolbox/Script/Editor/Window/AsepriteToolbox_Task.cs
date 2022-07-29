namespace AsepriteToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;
	using AsepriteToolbox.Core;


	// === Task ===
	public partial class AsepriteToolbox_Window {




		#region --- SUB ---



		private enum Task {
			CreateImage = 0,
			CreateSprite = 1,
			CreateAnimation = 2,
			CreatePrefab = 3,
			ToJson = 4,
			ToAse = 5,

		}



		#endregion



		// TSK
		private static void DoTask (Task task) {
			EditorApplication.delayCall += () => {
				DoTaskLogic(task);
			};
		}



		private static void DoTaskLogic (Task task) {

			RefreshFileDisplayer();

			bool hasError = false;
			string errorMsg = "";
			int successCount = 0;
			int currentTaskCount = 0;
			int taskCount = FILE_RECORDER.TaskCount;
			string pingPath = "";
			string applyingPresetName = "";

			// Export Path
			if (TheExportMode == ExportMode.AskEverytime) {
				BrowseExportPath();
			}

			// ApplyPresetByAseName
			ConfigPreset preset = null;
			if (ApplyPresetByAseName) {
				preset = GetCurrentConfig();
			}

			// Do Task
			Util.StartWatch();
			FILE_RECORDER.ForAllTasks((obj, path, middlePath, name) => {

				// ProgressBar
				Util.ProgressBar("Please Wait", string.Format("[{1}/{2}] {0}...", name, currentTaskCount + 1, taskCount), (float)currentTaskCount / taskCount);
				currentTaskCount++;

				if (!obj) { return; }

				try {
					// Path
					string ex = Util.GetExtension(path);
					string exportFolder = Util.FixPath(
						TheExportMode == ExportMode.OriginalPath ?
						Util.GetParentPath(path) :
						Util.CombinePaths(ExportPath, middlePath)
					);
					if (string.IsNullOrEmpty(exportFolder)) { return; }

					// Preset
					if (preset != null) {
						int presetCount = ConfigPreset.PresetCount;
						bool flag = false;
						for (int i = 0; i < presetCount; i++) {
							string pName = ConfigPreset.GetPresetName(i);
							if (name.StartsWith(pName, true, System.Globalization.CultureInfo.CurrentCulture)) {
								if (pName == applyingPresetName) {
									flag = true;
									break;
								} else {
									var p = ConfigPreset.GetPresetFromDisk(pName);
									if (p != null) {
										flag = true;
										p.Apply();
										LoadAllSaving();
										applyingPresetName = pName;
										break;
									}
								}
							}
						}
						if (!flag) {
							preset.Apply();
							LoadAllSaving();
							applyingPresetName = "";
						}
					}

					// Task
					switch (task) {
						default:
							break;
						case Task.CreateAnimation:
						case Task.CreateSprite:
						case Task.CreateImage:
						case Task.CreatePrefab: {
							// Ase Data
							AseData data = null;
							if (ex == ".json") {
								data = AseData.CreateFromJson(Util.Read(path));
							} else if (ex == ".ase" || ex == ".aseprite") {
								data = AseData.CreateFromBytes(Util.FileToByte(path));
							}
							if (data == null) { break; }

							// Result
							var results = new AseCore(data) {
								AseName = name,
								UserPivot = UserPivot,
								UserPivotMode = (UserPivotMode)UserPivotModeIndex.Value,
								VisibleLayerOnly = VisibleLayerOnly,
								IgnoreBackgroundLayer = IgnoreBackgroundLayer,
								TrimTexture = TrimTexture,
								TrimSprite = TrimSprite,
								IgnoreEmptySlice = IgnoreEmptySlice,
								PaddingMode = (PaddingMode)PaddingModeIndex.Value,
								PaddingTarget = (PaddingTarget)PaddingTargetIndex.Value,
								CreateAnimation = task == Task.CreateAnimation,
								SeparateEachLayer = SeparateEachLayer,
							}.CreateResults();

							// File
							pingPath = new FileCore(results) {
								AseName = name,
								PixelPerUnit = PixelPerUnit,
								IgnoreSprites = task == Task.CreateImage,
								CreateAnimation = task == Task.CreateAnimation,
								CreateAnimator = (task == Task.CreateAnimation) && CreateAnimatorController,
								FolderForTask = FolderForTask,
								TrimTexture = TrimTexture,
								TrimSprite = TrimSprite,
								SeparateEachLayer = SeparateEachLayer,
								FolderForLayer = FolderForLayer,
								ExportingFolder = exportFolder,
								CreatePrefab = task == Task.CreatePrefab,
								TextureExportMode = (TextureExportMode)TextureExportModeIndex.Value,
								AnimationExportMode = (AnimationExportMode)AnimationExportModeIndex.Value,
								PaddingMode = (PaddingMode)PaddingModeIndex.Value,
								NamingStrategy_Texture = new string[2] { GetNamingStrategyFormat(NamingStrategy_Texture, true), GetNamingStrategyFormat(NamingStrategy_Texture, false), },
								NamingStrategy_Sprite = new string[2] { GetNamingStrategyFormat(NamingStrategy_Sprite, true), GetNamingStrategyFormat(NamingStrategy_Sprite, false), },
								NamingStrategy_Animation = new string[2] { GetNamingStrategyFormat(NamingStrategy_Animation, true), GetNamingStrategyFormat(NamingStrategy_Animation, false), },
								NamingStrategy_Animator = new string[2] { GetNamingStrategyFormat(NamingStrategy_Animator, true), GetNamingStrategyFormat(NamingStrategy_Animator, false), },
							}.MakeFiles();

							// Final
							successCount++;
							break;
						}
						case Task.ToJson: {
							if (ex == ".json") { break; }
							var data = AseData.CreateFromBytes(Util.FileToByte(path));
							var json = data.ToJson();
							if (string.IsNullOrEmpty(json)) { break; }
							var jPath = Util.CombinePaths(exportFolder, name + ".json");
							pingPath = jPath;
							Util.Write(json, jPath);
							successCount++;
							break;
						}
						case Task.ToAse: {
							var aimPath = pingPath = Util.CombinePaths(exportFolder, name + ".ase");
							if (ex == ".ase") {
								break;
							} else if (ex == ".aseprite") {
								Util.CopyFile(path, aimPath);
							} else if (ex == ".json") {
								var data = AseData.CreateFromJson(Util.Read(path));
								Util.ByteToFile(data.ToBytes(), aimPath);
							}
							successCount++;
							break;
						}
					}

				} catch (System.Exception exc) {
					hasError = true;
					errorMsg = exc.Message;
					Util.ClearProgressBar();
				}
			});
			double time = Util.StopWatchAndGetTime();
			Util.ClearProgressBar();

			// Preset
			if (preset != null) {
				preset.Apply();
				LoadAllSaving();
			}

			// Ping
			if (Util.DirectoryExists(pingPath) || Util.FileExists(pingPath)) {
				var pingObj = AssetDatabase.LoadAssetAtPath<Object>(pingPath);
				if (pingObj) {
					EditorGUIUtility.PingObject(pingObj);
				}
			}

			// Final
			Util.StartWatch();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			Resources.UnloadUnusedAssets();
			AsePostprocessor.Clear();
			double importTime = Util.StopWatchAndGetTime();

			// Log
			if (hasError) {
				LogMessage(errorMsg, true);
			} else {
				LogMessage(string.Format("{0} {1}{2} created in {3} sec, import in {4} sec.", successCount, task == Task.CreateImage ? "image" : "file", successCount > 1 ? "s" : "", time.ToString("0.00"), importTime.ToString("0.00")), false);
			}

		}




	}
}