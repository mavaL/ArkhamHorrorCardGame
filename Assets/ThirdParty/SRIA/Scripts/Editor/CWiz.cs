using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util.GridView;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Editor
{
	public static class CWiz
	{
		public const string HOR_SCROLLBAR_RESPATH = "SRIA/Templates/Scrollbars/HorizontalScrollbar";
		public const string VERT_SCROLLBAR_RESPATH = "SRIA/Templates/Scrollbars/VerticalScrollbar";
		public const string TEMPLATE_TEXT_CLASSNAME_PREFIX = "public class ";
		public const string TEMPLATE_DEFAULT_NAMESPACE = "Your.Namespace.Here";

		public const string TEMPLATES_RESPATH = "SRIA/Templates";
		public const string TEMPLATE_SCRIPTS_RESPATH = TEMPLATES_RESPATH + "/Scripts";
		public const string TEMPLATE_SCRIPTS_HEADERCOMMENT_RESPATH = TEMPLATES_RESPATH + "/ScriptsChunks/HeaderComment";
		public const string TEMPLATE_ITEM_PREFABS_DIR_RESPATH = TEMPLATES_RESPATH + "/ExampleItemPrefabs";
		public const int SLOW_UPDATE_SKIPPED_FRAMES = 5;
		public const float SPACE_FOR_SCROLLBAR = 27 + 20;
		public const float VALUE_WIDTH_FLOAT = 200f;
		public const float VALUE_WIDTH2_FLOAT = 250f;
		public static GUILayoutOption LABEL_WIDTH = GUILayout.Width(150f);
		public static GUILayoutOption VALUE_WIDTH = GUILayout.Width(VALUE_WIDTH_FLOAT);


		public static string GetExampleItemPrefabResPath(string templateName)
		{
			templateName = templateName.Replace("SRIA", "");
			string prefabName = templateName + "ItemPrefab(Example)";
			return TEMPLATE_ITEM_PREFABS_DIR_RESPATH + "/" + prefabName;
		}

		public static EditorWindow GetBestEditorWindowToShowNotification(bool allowFocusedWindow = true)
		{
			EditorWindow editorWindow = EditorWindow.focusedWindow;
			if (!editorWindow)
			{
				editorWindow = EditorWindow.mouseOverWindow;
				if (!editorWindow)
					editorWindow = EditorWindow.GetWindow<SceneView>();
			}

			return editorWindow;
		}

		public static void ShowNotification(string msg, bool beep, bool allowFocusedWindow) { ShowNotification(msg, beep, null, allowFocusedWindow); }

		public static void ShowNotification(string msg, bool beep, EditorWindow editorWindow, bool allowFocusedWindow)
		{
			if (!editorWindow)
				editorWindow = GetBestEditorWindowToShowNotification(allowFocusedWindow);

			if (!editorWindow)
				return;

			try { editorWindow.ShowNotification(new GUIContent(msg)); } catch { }

			if (beep)
				try { EditorApplication.Beep(); } catch { }
		}

		public static void ShowCouldNotExecuteCommandNotification(EditorWindow editorWindow) { ShowNotification("Invalid state. Check console for details", true, editorWindow); }

		public static bool IsSubclassOfRawGeneric(Type baseType, Type derivedType)
		{
			while (derivedType != null && derivedType != typeof(object))
			{
				var currentType = derivedType.IsGenericType ? derivedType.GetGenericTypeDefinition() : derivedType;
				if (baseType == currentType)
					return true;

				derivedType = derivedType.BaseType;
			}
			return false;
		}

		public static bool IsSubclassOfSRIA(Type derivedType) { return IsSubclassOfRawGeneric(typeof(SRIA<,>), derivedType); }

		public static void ReplaceTemplateDefaultNamespaceWithUnique(ref string template)
		{
			template = template.Replace(TEMPLATE_DEFAULT_NAMESPACE, TEMPLATE_DEFAULT_NAMESPACE + DateTime.Now.ToString("yyMMMddhhmmssfff", CultureInfo.InvariantCulture));
		}
	}
}
