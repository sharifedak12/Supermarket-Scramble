namespace AsepriteToolbox {
	using AsepriteToolbox.Saving;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;


	// === Saving ===
	public partial class AsepriteToolbox_Window : MoenenEditorWindow {


		private readonly static EditorSavingBool ColorfulTitle = new EditorSavingBool("A2U.ColorfulTitle", true);
		private readonly static EditorSavingBool LogToConsole = new EditorSavingBool("A2U.LogToConsole", true);
		private readonly static EditorSavingBool LogToDialog = new EditorSavingBool("A2U.LogToDialog", false);
		private readonly static EditorSavingBool DockEditorToSceneView = new EditorSavingBool("A2U.DockEditorToSceneView", true);
		private readonly static EditorSavingBool LayoutF_Preview = new EditorSavingBool("A2U.LayoutF_Preview", true);
		private readonly static EditorSavingBool LayoutF_Create = new EditorSavingBool("A2U.LayoutF_Create", true);
		private readonly static EditorSavingBool LayoutF_Tool = new EditorSavingBool("A2U.LayoutF_Tool", true);
		private readonly static EditorSavingBool LayoutF_Setting = new EditorSavingBool("A2U.LayoutF_Setting", false);
		private readonly static EditorSavingBool LayoutF_About = new EditorSavingBool("A2U.LayoutF_About", false);
		private readonly static EditorSavingBool LayoutF_Setting_General = new EditorSavingBool("A2U.LayoutF_Setting_General", false);
		private readonly static EditorSavingBool LayoutF_Setting_NamingStrategy = new EditorSavingBool("A2U.LayoutF_Setting_NamingStrategy", false);
		private readonly static EditorSavingBool LayoutF_Setting_System = new EditorSavingBool("A2U.LayoutF_Setting_System", false);
		private readonly static EditorSavingBool LayoutF_Setting_Preset = new EditorSavingBool("A2U.LayoutF_Setting_Preset", false);
		private readonly static EditorSavingInt ExportModeIndex = new EditorSavingInt("A2U.ExportModeIndex", 0);
		private readonly static EditorSavingInt UserPivotModeIndex = new EditorSavingInt("A2U.UserPivotModeIndex", 0);
		private readonly static EditorSavingInt TextureExportModeIndex = new EditorSavingInt("A2U.TextureExportModeIndex", 0);
		private readonly static EditorSavingInt AnimationExportModeIndex = new EditorSavingInt("A2U.AnimationExportModeIndex", 0);
		private readonly static EditorSavingInt PixelPerUnit = new EditorSavingInt("A2U.PixelPerUnit", 16);
		private readonly static EditorSavingInt PaddingModeIndex = new EditorSavingInt("A2U.PaddingModeIndex", 0);
		private readonly static EditorSavingInt PaddingTargetIndex = new EditorSavingInt("A2U.PaddingTargetIndex", 0);
		private readonly static EditorSavingBool VisibleLayerOnly = new EditorSavingBool("A2U.VisibleLayerOnly", true);
		private readonly static EditorSavingBool IgnoreBackgroundLayer = new EditorSavingBool("A2U.IgnoreBackgroundLayer", true);
		private readonly static EditorSavingBool SeparateEachLayer = new EditorSavingBool("A2U.SeparateEachLayer", false);
		private readonly static EditorSavingBool TrimTexture = new EditorSavingBool("A2U.TrimTexture", false);
		private readonly static EditorSavingBool TrimSprite = new EditorSavingBool("A2U.TrimSprite", false);
		private readonly static EditorSavingBool CreateAnimatorController = new EditorSavingBool("A2U.CreateAnimatorController", true);
		private readonly static EditorSavingBool FolderForTask = new EditorSavingBool("A2U.FolderForTask", true);
		private readonly static EditorSavingBool FolderForLayer = new EditorSavingBool("A2U.FolderForLayer", false);
		private readonly static EditorSavingBool ApplyPresetByAseName = new EditorSavingBool("A2U.ApplyPresetByAseName", false);
		private readonly static EditorSavingBool IgnoreEmptySlice = new EditorSavingBool("A2U.IgnoreEmptySlice", true);
		private readonly static EditorSavingVector2 UserPivot = new EditorSavingVector2("A2U.UserPivot", new Vector2(0.5f, 0.5f));
		private readonly static EditorSavingString ExportPath = new EditorSavingString("A2U.ExportPath", "Assets");
		private readonly static EditorSavingString NamingStrategy_Texture = new EditorSavingString("A2U.NamingStrategy_Texture", "{Ase}(_{Frame})");
		private readonly static EditorSavingString NamingStrategy_Sprite = new EditorSavingString("A2U.NamingStrategy_Sprite", "{Slice}");
		private readonly static EditorSavingString NamingStrategy_Animation = new EditorSavingString("A2U.NamingStrategy_Animation", "{Ase}(_{Tag})");
		private readonly static EditorSavingString NamingStrategy_Animator = new EditorSavingString("A2U.NamingStrategy_Animator", "{Ase}");




		private static void LoadAllSaving () {
			ColorfulTitle.Load();
			LayoutF_Preview.Load();
			LayoutF_Create.Load();
			LayoutF_Tool.Load();
			LayoutF_Setting.Load();
			LayoutF_About.Load();
			LogToConsole.Load();
			LogToDialog.Load();
			LayoutF_Setting_General.Load();
			LayoutF_Setting_System.Load();
			LayoutF_Setting_NamingStrategy.Load();
			ExportModeIndex.Load();
			ExportPath.Load();
			VisibleLayerOnly.Load();
			IgnoreBackgroundLayer.Load();
			CreateAnimatorController.Load();
			UserPivot.Load();
			UserPivotModeIndex.Load();
			PixelPerUnit.Load();
			DockEditorToSceneView.Load();
			NamingStrategy_Texture.Load();
			NamingStrategy_Sprite.Load();
			NamingStrategy_Animation.Load();
			NamingStrategy_Animator.Load();
			TextureExportModeIndex.Load();
			AnimationExportModeIndex.Load();
			TrimSprite.Load();
			PaddingModeIndex.Load();
			FolderForTask.Load();
			SeparateEachLayer.Load();
			LayoutF_Setting_Preset.Load();
			PaddingTargetIndex.Load();
			FolderForLayer.Load();
			ApplyPresetByAseName.Load();
			TrimTexture.Load();
			IgnoreEmptySlice.Load();
		}


		private static void DeleteAllSaving () {
			ColorfulTitle.Reset();
			LogToConsole.Reset();
			LogToDialog.Reset();
			ExportModeIndex.Reset();
			ExportPath.Reset();
			VisibleLayerOnly.Reset();
			IgnoreBackgroundLayer.Reset();
			CreateAnimatorController.Reset();
			UserPivot.Reset();
			UserPivotModeIndex.Reset();
			PixelPerUnit.Reset();
			DockEditorToSceneView.Reset();
			NamingStrategy_Texture.Reset();
			NamingStrategy_Sprite.Reset();
			NamingStrategy_Animation.Reset();
			NamingStrategy_Animator.Reset();
			TextureExportModeIndex.Reset();
			AnimationExportModeIndex.Reset();
			TrimSprite.Reset();
			PaddingModeIndex.Reset();
			FolderForTask.Reset();
			SeparateEachLayer.Reset();
			PaddingTargetIndex.Reset();
			FolderForLayer.Reset();
			ApplyPresetByAseName.Reset();
			TrimTexture.Reset();
			IgnoreEmptySlice.Reset();
		}



		private static ConfigPreset GetCurrentConfig () {
			RefreshCurrentPresetCode();
			return new ConfigPreset(new List<ConfigPreset.KeyValueType>() {
				new ConfigPreset.KeyValueType(VisibleLayerOnly),
				new ConfigPreset.KeyValueType(IgnoreBackgroundLayer),
				new ConfigPreset.KeyValueType(CreateAnimatorController),
				new ConfigPreset.KeyValueType(UserPivot),
				new ConfigPreset.KeyValueType(UserPivotModeIndex),
				new ConfigPreset.KeyValueType(PixelPerUnit),
				new ConfigPreset.KeyValueType(NamingStrategy_Texture),
				new ConfigPreset.KeyValueType(NamingStrategy_Sprite),
				new ConfigPreset.KeyValueType(NamingStrategy_Animation),
				new ConfigPreset.KeyValueType(NamingStrategy_Animator),
				new ConfigPreset.KeyValueType(TextureExportModeIndex),
				new ConfigPreset.KeyValueType(AnimationExportModeIndex),
				new ConfigPreset.KeyValueType(TrimTexture),
				new ConfigPreset.KeyValueType(TrimSprite),
				new ConfigPreset.KeyValueType(PaddingModeIndex),
				new ConfigPreset.KeyValueType(FolderForTask),
				new ConfigPreset.KeyValueType(SeparateEachLayer),
				new ConfigPreset.KeyValueType(PaddingTargetIndex),
				new ConfigPreset.KeyValueType(FolderForLayer),
				new ConfigPreset.KeyValueType(IgnoreEmptySlice),

			}, CurrentPresetCode);
		}


	}
}