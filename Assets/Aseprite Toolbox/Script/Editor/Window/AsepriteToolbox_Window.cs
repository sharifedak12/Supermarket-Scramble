namespace AsepriteToolbox {
	using System.Collections;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using UnityEngine;
	using UnityEditor;
	using AsepriteToolbox.Saving;
	using AsepriteToolbox.Core;


	// === Main ===
	public partial class AsepriteToolbox_Window : MoenenEditorWindow {




		#region --- SUB ---



		private enum ExportMode {
			OriginalPath = 0,
			SpecificPath = 1,
			AskEverytime = 2,

		}



		#endregion




		#region --- VAR ---


		// Global
		private const string TITLE = "Aseprite Toolbox";
		private static string COLORFUL_TITLE = "";
		private const int LABEL_WIDTH = 102;
		private const string NAMING_ASE = "{ase}";
		private const string NAMING_FRAME = "{frame}";
		private const string NAMING_SLICE = "{slice}";
		private const string NAMING_TAG = "{tag}";
		private const string NAMING_COUNT = "{count}";
		private const string NAME_STRATEGY_HINT = @"<color=#dddddd>{ase}</color> = .ase file name
<color=#dddddd>{frame}</color> = Aseprite frame number
<color=#dddddd>{tag}</color> = tag in Aseprite timeline
<color=#dddddd>{slice}</color> = slice name in Aseprite
<color=#dddddd>{count}</color> = current count for processing object
● Ignore case for all keywords.
● Content inside () will be ignored when there's only one item.";
		private readonly static FileRecorder FILE_RECORDER = new FileRecorder(".ase", ".aseprite", ".json");


		// Short
		private static GUIStyle TitleStyle {
			get {
				return _TitleStyle ?? (_TitleStyle = new GUIStyle(GUI.skin.label) {
					alignment = TextAnchor.MiddleCenter,
					richText = true,
					fontSize = 12,
					fontStyle = FontStyle.Bold,
				});
			}
		}

		private static GUIStyle HelpBoxStyle {
			get {
				return _HelpBoxStyle ?? (_HelpBoxStyle = new GUIStyle(EditorStyles.helpBox) {
					richText = true,
				});
			}
		}

		private static Texture2D AseIcon {
			get {
				return _AseIcon ?? (_AseIcon = GetImage("Ase Icon.png") ?? Texture2D.blackTexture);
			}
		}

		private static Texture2D JsonIcon {
			get {
				return _JsonIcon ?? (_JsonIcon = GetImage("Json Icon.png") ?? Texture2D.blackTexture);
			}
		}

		private static ExportMode TheExportMode {
			get {
				return (ExportMode)ExportModeIndex.Value;
			}
			set {
				ExportModeIndex.Value = (int)value;
			}
		}

		private static UserPivotMode TheUserPivotMode {
			get {
				return (UserPivotMode)UserPivotModeIndex.Value;
			}
		}


		// Data
		private static GUIStyle _TitleStyle = null;
		private static GUIStyle _HelpBoxStyle = null;
		private static Texture2D _AseIcon = null;
		private static Texture2D _JsonIcon = null;
		private static string NewPresetName = "New Preset";
		private static string CurrentPresetCode = "";
		private static string CurrentPresetName = "";
		private Vector2 MasterScrollPosition = Vector2.zero;


		#endregion




		#region --- MSG ---


		[MenuItem("Tools/Aseprite Toolbox/Aseprite Toolbox")]
		private static void OpenWindow () {
			try {
				var inspector = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
				var window = inspector != null ?
					GetWindow<AsepriteToolbox_Window>("Aseprite Toolbox", true, inspector) :
					GetWindow<AsepriteToolbox_Window>("Aseprite Toolbox", true);
				window.minSize = new Vector2(275, 400);
				window.maxSize = new Vector2(600, 1000);
				// Icon
				var icon = GetImage("Icon.png");
				if (icon) {
					window.titleContent = new GUIContent("Aseprite Toolbox", icon);
				}
			} catch (System.Exception ex) {
				Debug.LogWarning("Failed to open window.\n" + ex.Message);
			}
		}


		[InitializeOnLoadMethod]
		private static void Init () {
			COLORFUL_TITLE = GetColorfulString(TITLE, 8);
			ConfigPreset.RefreshPreset();
			MainInit();
		}


		private void OnGUI () {
			MasterScrollPosition = GUILayout.BeginScrollView(MasterScrollPosition, GUI.skin.scrollView);
			MainGUI();
			GUILayout.EndScrollView();
			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl(null);
				Repaint();
			}
		}


		private void OnSelectionChange () {
			RefreshFileDisplayer();
			Repaint();
		}


		private void OnFocus () {
			RefreshFileDisplayer();
			Repaint();
		}


		#endregion




		#region --- GUI ---



		private static void TitleGUI () {
			var rect = GUIRect(0, 30);
			EditorGUI.DropShadowLabel(rect, TITLE, TitleStyle);
			if (ColorfulTitle) {
				GUI.Label(rect, COLORFUL_TITLE, TitleStyle);
			}
		}


		private static void PreviewGUI () {
			LayoutF_Preview.Value = LayoutF(() => {
				const int HEIGHT = 36;
				if (FILE_RECORDER.ItemCount == 0) {
					// Nothing Selecting
					EditorGUI.HelpBox(GUIRect(0, HEIGHT), "Select *.ase or *.aseprite files or folders in project view.", MessageType.Info);
				} else {
					// Something Selecting
					if (FILE_RECORDER.FolderCount + FILE_RECORDER.TaskCount == 0) {
						// No Aseprite file or folder selecting
						EditorGUI.HelpBox(GUIRect(0, HEIGHT), "No *.ase or *.aseprite file is selecting.", MessageType.Warning);
					} else if (FILE_RECORDER.TaskCount == 0) {
						// No Aseprite file selecting but folder selecting
						EditorGUI.HelpBox(GUIRect(0, HEIGHT), "Selecting folder do not contains *.ase or *.aseprite file.", MessageType.Warning);
					} else {
						// Aseprite file selecting
						LayoutH(() => {
							Space(8);
							FILE_RECORDER.ForAllExtensions((ex, count) => {
								if (count == 0) { return; }
								var rect = GUIRect(36, HEIGHT);
								GUI.Box(rect, GUIContent.none);
								var icon = ex == ".ase" || ex == ".aseprite" ? AseIcon : JsonIcon;
								if (icon) {
									rect.x += 3;
									rect.y += 3;
									rect.width -= 6;
									rect.height -= 6;
									GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
								}
								Space(4);
								LayoutV(() => {
									Space(4);
									GUI.Label(GUIRect(0, (HEIGHT - 8) / 2), ex);
									GUI.Label(GUIRect(0, (HEIGHT - 8) / 2), "× " + count);
									Space(4);
								});
							});
						});
					}
				}
				Space(4);
			}, "Selecting Files", LayoutF_Preview, true);
		}


		private static void CreateGUI () {
			LayoutF_Create.Value = LayoutF(() => {
				// Create
				LayoutH(() => {
					GUIRect(12, 1);
					LayoutV(() => {
						const int HEIGHT = 32;
						if (Button(GUIRect(0, HEIGHT), "Create Images", new Color(242f / 256f, 138f / 256f, 73f / 256f, 0.5f), FILE_RECORDER.TaskCount > 0)) {
							DoTask(Task.CreateImage);
						}
						Space(2);
						if (Button(GUIRect(0, HEIGHT), "Create Sprites", new Color(242f / 256f, 138f / 256f, 73f / 256f, 0.5f), FILE_RECORDER.TaskCount > 0)) {
							DoTask(Task.CreateSprite);
						}
						Space(2);
						if (Button(GUIRect(0, HEIGHT), "Create Animations", new Color(242f / 256f, 138f / 256f, 73f / 256f, 0.5f), FILE_RECORDER.TaskCount > 0)) {
							DoTask(Task.CreateAnimation);
						}
						Space(2);
						if (Button(GUIRect(0, HEIGHT), "Create Prefabs", new Color(242f / 256f, 138f / 256f, 73f / 256f, 0.5f), FILE_RECORDER.TaskCount > 0)) {
							DoTask(Task.CreatePrefab);
						}
						Space(2);
						LayoutH(() => {
							if (Button(GUIRect(0, HEIGHT), "To Json", new Color(1, 1, 1, 0.3f), FILE_RECORDER.GetExtensionCount(".ase") + FILE_RECORDER.GetExtensionCount(".aseprite") > 0)) {
								DoTask(Task.ToJson);
							}
							if (Button(GUIRect(0, HEIGHT), "To Ase", new Color(1, 1, 1, 0.3f), FILE_RECORDER.GetExtensionCount(".aseprite") + FILE_RECORDER.GetExtensionCount(".json") > 0)) {
								DoTask(Task.ToAse);
							}
						});
						Space(2);
					});
					GUIRect(12, 1);
				});
				Space(2);
				// Export Mode
				LayoutV(() => {
					const int HEIGHT = 16;
					Space(4);
					// Mode Popup
					LayoutH(() => {
						GUI.Label(GUIRect(82, HEIGHT), "Export To:");
						TheExportMode = (ExportMode)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), TheExportMode);
					});
					Space(4);
					// Path Field
					if (TheExportMode == ExportMode.SpecificPath) {
						LayoutH(() => {
							GUI.Label(GUIRect(82, HEIGHT), "Path:");
							var oldE = GUI.enabled;
							GUI.enabled = false;
							GUI.Label(GUIRect(0, HEIGHT), ExportPath, GUI.skin.textField);
							GUI.enabled = oldE;
							if (GUI.Button(GUIRect(52, HEIGHT), "Browse", EditorStyles.miniButtonRight)) {
								BrowseExportPath();
							}
						});
						Space(4);
					}
				}, true);
				Space(2);
			}, "Create", LayoutF_Create, true);
		}


		private static void SettingGUI () {
			LayoutF_Setting.Value = LayoutF(() => {
				const int HEIGHT = 18;
				const int HEIGHT_ALT = 16;

				bool changedForPreset = false;

				// General
				LayoutF_Setting_General.Value = LayoutF(() => {

					EditorGUI.BeginChangeCheck();

					Space(2);
					// Texture Export Mode  
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Texture Export");
						TextureExportModeIndex.Value = (int)(TextureExportMode)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), (TextureExportMode)TextureExportModeIndex.Value);
					});
					Space(2);
					// Animation Export Mode
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Animation Export");
						AnimationExportModeIndex.Value = (int)(AnimationExportMode)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), (AnimationExportMode)AnimationExportModeIndex.Value);
					});
					Space(2);
					// Padding Mode
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Padding");
						PaddingModeIndex.Value = (int)(PaddingMode)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), (PaddingMode)PaddingModeIndex.Value);
						if (PaddingModeIndex.Value != (int)PaddingMode.NoPadding) {
							PaddingTargetIndex.Value = (int)(PaddingTarget)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), (PaddingTarget)PaddingTargetIndex.Value);
						}
					});
					Space(2);
					// Pivot Mode
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Pivot Mode");
						UserPivotModeIndex.Value = (int)(UserPivotMode)EditorGUI.EnumPopup(GUIRect(0, HEIGHT), (UserPivotMode)UserPivotModeIndex.Value);
					});
					Space(2);
					// Pivot
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), TheUserPivotMode == UserPivotMode.IgnoreSlicePivot ? "Final Pivot" : "Failback Pivot");
						UserPivot.Value = EditorGUI.Vector2Field(GUIRect(0, HEIGHT), "", UserPivot);
					});
					Space(2);
					// Pixel Per Unit
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Pixel Per Unit");
						PixelPerUnit.Value = EditorGUI.IntField(GUIRect(0, HEIGHT), PixelPerUnit);
					});
					Space(2);
					// Visible Layer Only
					VisibleLayerOnly.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Visible Layer Only", VisibleLayerOnly);
					Space(2);
					// Ignore Background
					LayoutH(() => {
						IgnoreBackgroundLayer.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Ignore Background Layer", IgnoreBackgroundLayer);
						if (GUI.Button(GUIRect(HEIGHT, HEIGHT), "?", EditorStyles.miniButton)) {
							Util.Dialog("Tip", "Right click a layer in Aseprite, Choose \"Background from Layer\" to set a layer to the background.", "OK");
						}
					});
					Space(2);
					// Separate Each Layer
					SeparateEachLayer.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Separate Each Layer", SeparateEachLayer);
					Space(2);
					// Create Folder for Layer 
					bool oldE = GUI.enabled;
					GUI.enabled = SeparateEachLayer;
					FolderForLayer.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Create a Folder for Each Layer", FolderForLayer);
					Space(2);
					GUI.enabled = oldE;
					// Create Folder
					FolderForTask.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Create a Folder for Each Task", FolderForTask);
					Space(2);
					LayoutH(() => {
						// Trim Texture
						TrimTexture.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Trim Texture", TrimTexture);
						// Trim Sprite
						TrimSprite.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Trim Sprite", TrimSprite);
					});
					Space(2);
					// Ignore Empty Slice
					IgnoreEmptySlice.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Ignore Empty Slice", IgnoreEmptySlice);
					Space(2);
					// Create Animator
					CreateAnimatorController.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Create Animator Controller", CreateAnimatorController);
					Space(2);
					changedForPreset = EditorGUI.EndChangeCheck() || changedForPreset;
				}, "General", LayoutF_Setting_General, true);


				// Naming Strategy
				LayoutF_Setting_NamingStrategy.Value = LayoutF(() => {

					EditorGUI.BeginChangeCheck();

					// Texture 
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Texture");
						NamingStrategy_Texture.Value = EditorGUI.TextField(GUIRect(0, HEIGHT), NamingStrategy_Texture);
					});
					Space(2);

					// Sprite 
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Sprite");
						NamingStrategy_Sprite.Value = EditorGUI.TextField(GUIRect(0, HEIGHT), NamingStrategy_Sprite);
					});
					Space(2);

					// Animation 
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Animation");
						NamingStrategy_Animation.Value = EditorGUI.TextField(GUIRect(0, HEIGHT), NamingStrategy_Animation);
					});
					Space(2);

					// Animator Controller 
					LayoutH(() => {
						GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Animator Ctrller");
						NamingStrategy_Animator.Value = EditorGUI.TextField(GUIRect(0, HEIGHT), NamingStrategy_Animator);
					});
					Space(2);

					changedForPreset = EditorGUI.EndChangeCheck() || changedForPreset;

					// Help Box
					Space(6);
					GUI.Label(GUIRect(0, 112), NAME_STRATEGY_HINT, HelpBoxStyle);
					Space(2);

				}, "Naming Strategy", LayoutF_Setting_NamingStrategy, true);


				// System
				LayoutF_Setting_System.Value = LayoutF(() => {

					Space(2);
					LogToConsole.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Log Message To Console", LogToConsole);
					Space(2);
					LogToDialog.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Log Message To Dialog", LogToDialog);
					Space(2);
					ColorfulTitle.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Colorful Title", ColorfulTitle);
					Space(2);
					bool oldE = GUI.enabled;
					GUI.enabled = false;
					DockEditorToSceneView.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Dock Editor Next To Scene", DockEditorToSceneView);
					GUI.enabled = oldE;

					Space(12);
					LayoutH(() => {
						GUIRect(24, 1);
						if (GUI.Button(GUIRect(0, 16), "Reset All Settings", EditorStyles.miniButton)) {
							bool reset = Util.Dialog("Confirm", "Reset all settings for Aseprite Toolbox?", "Reset ？", "Cancel");
							if (reset) { reset = Util.Dialog("Confirm", "Reset all settings for Aseprite Toolbox?", "Reset !", "Cancel"); }
							if (reset) {
								DeleteAllSaving();
								changedForPreset = true;
							}
						}
						GUIRect(24, 1);
					});
					Space(4);
				}, "System", LayoutF_Setting_System, true);


				// Preset
				LayoutF_Setting_Preset.Value = LayoutF(() => {

					// Buttons
					Space(4);
					LayoutH(() => {
						// Name Field
						NewPresetName = EditorGUI.TextField(GUIRect(0, HEIGHT_ALT), NewPresetName);
						// Add Button
						if (GUI.Button(GUIRect(64, HEIGHT_ALT), "+ Current", EditorStyles.miniButtonMid)) {
							if (!string.IsNullOrEmpty(NewPresetName)) {
								if (!ConfigPreset.PresetExists(NewPresetName) || Util.Dialog("Confirm", "Override preset file " + NewPresetName + "?", "Override", "Cancel")) {
									var preset = GetCurrentConfig();
									if (preset != null) {
										ConfigPreset.ImportPreset(preset, NewPresetName);
										RefreshCurrentPresetCode();
									}
								}
							} else {
								Util.Dialog("", "Preset name can not be empty.", "OK");
							}
						}
						// Refresh
						if (GUI.Button(GUIRect(64, HEIGHT_ALT), "Refresh", EditorStyles.miniButtonRight)) {
							ConfigPreset.RefreshPreset();
							RefreshCurrentPresetCode();
						}
					});
					Space(4);

					// List
					LayoutV(() => {
						Space(4);
						for (int i = 0; i < ConfigPreset.PresetCount; i++) {
							LayoutH(() => {
								var name = ConfigPreset.GetPresetName(i);
								GUI.Label(GUIRect(0, HEIGHT_ALT), name);
								// Apply Button
								if (GUI.Button(GUIRect(42, HEIGHT_ALT), "Apply", EditorStyles.miniButtonLeft)) {
									var preset = ConfigPreset.GetPresetFromDisk(name);
									if (preset != null) {
										preset.Apply();
										LoadAllSaving();
										RefreshCurrentPresetCode();
										NewPresetName = name;
										GUI.FocusControl(null);
									}
								}
								// Delete Button
								if (GUI.Button(GUIRect(24, HEIGHT_ALT), "×", EditorStyles.miniButtonRight)) {
									if (Util.Dialog("Confirm", "Delete preset file " + name + "?", "Delete", "Cancel")) {
										ConfigPreset.DeletePresetFromDisk(name);
										RefreshCurrentPresetCode();
									}
								}
							});
							Space(4);
						}
					}, true);
					Space(2);

					// Setting 
					LayoutH(() => {
						ApplyPresetByAseName.Value = EditorGUI.ToggleLeft(GUIRect(0, HEIGHT), " Apply Preset By Ase Name", ApplyPresetByAseName);
						if (GUI.Button(GUIRect(HEIGHT, HEIGHT), "?", EditorStyles.miniButton)) {
							Util.Dialog("Tip", "Apply the preset if the name of ase file start with the name of a saved preset. This operation ignore cases.\n\neg. You have a preset called \"tile\" and a ase file called \"tile_Trees.ase\", before the ase file is processed, that preset will be apply.", "OK");
						}
					});
					Space(4);

				}, "Preset   [ " + CurrentPresetName + " ]", LayoutF_Setting_Preset, true);


				if (changedForPreset) {
					RefreshCurrentPresetCode();
				}


			}, "Setting", LayoutF_Setting, true);
		}


		private static void AboutGUI () {
			LayoutF_About.Value = LayoutF(() => {
				const int HEIGHT = 18;
				GUI.Label(GUIRect(0, HEIGHT), "Aseprite Toolbox by 楠瓜Moenen.");
				Link(GUIRect(0, HEIGHT), "★★★★★ if you like it.", @"http://u3d.as/1Aqc");
				Space(4);
				// Contact
				LayoutH(() => {
					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Twitter");
					Link(GUIRect(0, HEIGHT), "@_Moenen", @"https://twitter.com/_Moenen");
				});
				LayoutH(() => {
					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "QQ");
					Link(GUIRect(0, HEIGHT), "1182032752", @"tencent://message/?Menu=yes&uin=1182032752&Service=300&sigT=45a1e5847943b64c6ff3990f8a9e644d2b31356cb0b4ac6b24663a3c8dd0f8aa12a595b1714f9d45");
				});
				LayoutH(() => {
					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Art Station");
					Link(GUIRect(0, HEIGHT), "Moenen", @"https://www.artstation.com/moenen");
				});
				LayoutH(() => {
					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Unity Store");
					Link(GUIRect(0, HEIGHT), "Moenen", @"https://assetstore.unity.com/publishers/15506");
				});
				LayoutH(() => {
					GUI.Label(GUIRect(LABEL_WIDTH, HEIGHT), "Email");
					Link(GUIRect(0, HEIGHT), "moenenn@163.com", @"mailto:moenenn@163.com");
				});
				Space(6);
				// Products
				ProductItemGUI("0", "Pixel Character", @"http://u3d.as/Tjd");
				Space(2);
				ProductItemGUI("1", "Pixel Environment", @"http://u3d.as/Tjg");
				Space(2);
				ProductItemGUI("2", "Pixel Props", @"http://u3d.as/Tjh");
				Space(2);
				ProductItemGUI("3", "Pixel Vegetation", @"http://u3d.as/Tjj");
				Space(2);
				ProductItemGUI("4", "Pixel Vehicle", @"http://u3d.as/Tjo");
				Space(2);
				ProductItemGUI("5", "The Pixel Man (Free)", @"http://u3d.as/XvH");
				Space(2);
				ProductItemGUI("6", "Pixel Cards", @"http://u3d.as/1kWc ");
				Space(2);
				ProductItemGUI("7", "Pixel Particles", @"http://u3d.as/1hbh");
				Space(2);

			}, "About", LayoutF_About, true);
		}


		#endregion




		#region --- API --


		public static void MainInit () {
			LoadAllSaving();
			RefreshFileDisplayer();
			RefreshCurrentPresetCode();
		}


		public static void MainGUI () {
			BeginLogger();
			TitleGUI();
			PreviewGUI();
			CreateGUI();
			SettingGUI();
			AboutGUI();
			EndLogger();
		}


		#endregion




		#region --- LGC ---


		private static void RefreshFileDisplayer () {
			FILE_RECORDER.SetSelection(Selection.GetFiltered<Object>(SelectionMode.Assets));
		}


		private static bool BrowseExportPath () {
			if (!Util.DirectoryExists(ExportPath)) {
				ExportPath.Value = "Assets";
			}
			string newPath = Util.FixPath(EditorUtility.OpenFolderPanel("Select Export Path", ExportPath, ""));
			if (!string.IsNullOrEmpty(newPath)) {
				newPath = Util.FixedRelativePath(newPath);
				if (!string.IsNullOrEmpty(newPath)) {
					ExportPath.Value = newPath;
					return true;
				} else {
					Util.Dialog("Warning", "Export path must in Assets folder.", "OK");
				}
			}
			return false;
		}


		private static void LogMessage (string message, bool warning) {
			if (LogToConsole) {
				string msg = string.Format("[{0}] {1}", TITLE, message);
				if (warning) {
					Debug.LogWarning(msg);
				} else {
					Debug.Log(msg);
				}
			}
			if (LogToDialog) {
				Util.Dialog(TITLE, message, "OK");
			}
		}


		private static string GetNamingStrategyFormat (string strategy, bool ignoreBrackets) {
			if (ignoreBrackets) {
				strategy = new Regex(@"\([^\(]*\)").Replace(strategy, "");
			} else {
				strategy = strategy.Replace("(", "").Replace(")", "");
			}
			strategy = new Regex(@"{\d}").Replace(strategy, "");
			strategy = Regex.Replace(strategy, NAMING_ASE, "{0}", RegexOptions.IgnoreCase);
			strategy = Regex.Replace(strategy, NAMING_FRAME, "{1}", RegexOptions.IgnoreCase);
			strategy = Regex.Replace(strategy, NAMING_SLICE, "{2}", RegexOptions.IgnoreCase);
			strategy = Regex.Replace(strategy, NAMING_TAG, "{3}", RegexOptions.IgnoreCase);
			strategy = Regex.Replace(strategy, NAMING_COUNT, "{4}", RegexOptions.IgnoreCase);
			return strategy;
		}


		private static void ProductItemGUI (string imageName, string name, string link) {
			LayoutH(() => {
				Space(4);
				var icon = GetImage("AD/" + imageName + ".png");
				if (icon) {
					GUI.DrawTexture(GUIRect(24, 24), icon);
				} else {
					GUI.Box(GUIRect(24, 24), GUIContent.none);
				}
				Space(4);
				LayoutV(() => {
					GUIRect(0, 3);
					Link(GUIRect(0, 18), name, link);
					GUIRect(0, 3);
				});
			});
		}


		private static void RefreshCurrentPresetCode () {
			// Code
			var s = new System.Text.StringBuilder();
			s.AppendLine(TextureExportModeIndex.Value.ToString());
			s.AppendLine(AnimationExportModeIndex.Value.ToString());
			s.AppendLine(PaddingModeIndex.Value.ToString());
			s.AppendLine(PaddingTargetIndex.Value.ToString());
			s.AppendLine(UserPivotModeIndex.Value.ToString());
			s.AppendLine(UserPivot.Value.x.ToString("0.000"));
			s.AppendLine(UserPivot.Value.y.ToString("0.000"));
			s.AppendLine(PixelPerUnit.Value.ToString());
			s.AppendLine(VisibleLayerOnly.Value.ToString());
			s.AppendLine(IgnoreBackgroundLayer.Value.ToString());
			s.AppendLine(SeparateEachLayer.Value.ToString());
			s.AppendLine(FolderForTask.Value.ToString());
			s.AppendLine(FolderForLayer.Value.ToString());
			s.AppendLine(TrimTexture.Value.ToString());
			s.AppendLine(TrimSprite.Value.ToString());
			s.AppendLine(CreateAnimatorController.Value.ToString());
			s.AppendLine(IgnoreEmptySlice.Value.ToString());
			s.AppendLine(NamingStrategy_Texture.Value);
			s.AppendLine(NamingStrategy_Sprite.Value);
			s.AppendLine(NamingStrategy_Animation.Value);
			s.AppendLine(NamingStrategy_Animator.Value);
			CurrentPresetCode = s.ToString();
			// Name
			CurrentPresetName = "";
			for (int i = 0; i < ConfigPreset.PresetCount; i++) {
				if (CurrentPresetCode == ConfigPreset.GetPresetCode(i)) {
					if (!string.IsNullOrEmpty(CurrentPresetName)) {
						CurrentPresetName += ", ";
					}
					CurrentPresetName += ConfigPreset.GetPresetName(i);
				}
			}
		}


		#endregion




	}




	[CustomEditor(typeof(DefaultAsset)), CanEditMultipleObjects]
	public class AsepriteInspector : Editor {


		private bool IsValid = false;


		private void OnEnable () {
			IsValid = HasAseTarget();
			if (IsValid) {
				AsepriteToolbox_Window.MainInit();
			}
		}


		public override void OnInspectorGUI () {
			base.OnInspectorGUI();
			if (IsValid) {
				bool oldE = GUI.enabled;
				GUI.enabled = true;
				AsepriteToolbox_Window.MainGUI();
				GUI.enabled = oldE;
			}
		}


		private bool HasAseTarget () {
			for (int i = 0; i < targets.Length; i++) {
				string path = AssetDatabase.GetAssetPath(targets[i]);
				string ex = Util.GetExtension(path);
				if (ex == ".ase" || ex == ".aseprite" || Util.HasFileIn(path, "*.ase", "*.aseprite")) {
					return true;
				}
			}
			return false;
		}


	}



}