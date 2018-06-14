#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
#define SRIA_SCROLLRECT_HAS_VIEWPORT
#endif

// This allows faster debugging when need to simualte other platforms by commenting the custom define directive
#if UNITY_EDITOR_WIN
#define SRIA_UNITY_EDITOR_WIN
#endif

using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util.GridView;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

//namespace UnityEditor
//{
//	internal class t : UnityEditor.SyncVS
//	{
//		t ()
//		{
			
//		}
//	}
//}

namespace frame8.ScrollRectItemsAdapter.Editor
{
	public class InitSRIAWindow : BaseSRIAWindow<InitSRIAWindow.Parameters>
	{
		[SerializeField]
		State _State;


		protected override string CompilingScriptsText
		{
			get
			{
				return base.CompilingScriptsText + ((_WindowParams != null && _WindowParams.indexOfExistingImplementationToUse == 0) ? 
							"\n(Unity could briefly switch to the code editor and back. This is normal)" : "");
			}
		}

		Dictionary<Type, Action<BaseParams, RectTransform>> _MapParamBaseTypeToPrefabSetter;
		List<Type> _AllMonobehaviours = new List<Type>();
		//bool _VSSolutionReloaded;


		#region Visual studio solution reload code for windows
		// This prevents another visual studio instance from being opened when the solution was externally modified
		// by automatically presing the 'Reload' button.
		// Some changes were made
		// Original source https://gamedev.stackexchange.com/questions/124320/force-reload-vs-soution-explorer-when-adding-new-c-script-via-unity3d
#if SRIA_UNITY_EDITOR_WIN

		private enum ShowWindowEnum
		{
			Hide = 0,
			ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
			Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
			Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
			Restore = 9, ShowDefault = 10, ForceMinimized = 11
		};

		// = Is minimized
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool IsIconic(IntPtr handle);

		//[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
		//private static extern IntPtr GetForegroundWindow();

		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
		static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern int FindWindow(String ClassName, String WindowName);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern int SetForegroundWindow(IntPtr hwnd);

		// TFG: method added
		[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

		[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);


		// Delegate to filter which windows to include 
		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

		const string WINDOW_CAPTION = "File Modification Detected";


		static string GetWindowText(IntPtr hwnd)
		{
			int charCount = 512;
			var sb = new System.Text.StringBuilder(charCount);
			GetWindowText(hwnd, sb, charCount);
			return sb.ToString();
		}

		/// <summary> Find all windows that match the given filter </summary>
		/// <param name="filter"> A delegate that returns true for windows
		///    that should be returned and false for windows that should
		///    not be returned </param>
		public static List<IntPtr> FindWindows(EnumWindowsProc filter)
		{
			List<IntPtr> windows = new List<IntPtr>();

			EnumWindows(delegate (IntPtr wnd, IntPtr param)
			{
				if (filter(wnd, param))
				{
					// only add the windows that pass the filter
					windows.Add(wnd);
				}

				// but return true here so that we iterate all windows
				return true;
			}, IntPtr.Zero);

			return windows;
		}
		public static IntPtr FindWindow(EnumWindowsProc filter)
		{
			var list = FindWindows(filter);
			return list.Count > 0 ? list[0] : IntPtr.Zero;
		}

		static string GetProjectName()
		{
			string[] s = Application.dataPath.Split('/');
			return s[s.Length - 2];
		}
		static string[] GetTargetVSWindowNames(string projectName)
		{
			return new string[]
				{
				"UnityVS." + projectName + "-csharp - Microsoft Visual Studio",
				"UnityVS." + projectName + " - Microsoft Visual Studio",
				projectName + " - Microsoft Visual Studio",
				projectName + "-csharp - Microsoft Visual Studio",
				}
			;
		}

		static bool ContainsTargeVSWindowName(string title)
		{
			string projectName = GetProjectName();
			return Array.Exists(GetTargetVSWindowNames(projectName), pName => title.Contains(pName)); }

		static IntPtr GetVisualStudioHWNDIfOpenedWithCurrentProject()
		{
			return FindWindow((hwnd, __) => ContainsTargeVSWindowName(GetWindowText(hwnd)));
		}

		static bool IsVisualStudioOpenedWithCurrentProjectAndBusy()
		{
			if (GetVisualStudioHWNDIfOpenedWithCurrentProject() == IntPtr.Zero)
				return false;

			var vsProcesses = System.Diagnostics.Process.GetProcessesByName("devenv");

			// Exactly one visual studio instance is needed. Otherwise, we can't tell
			if (vsProcesses.Length != 1)
				return false;

			var process = vsProcesses[0];

			//return process.MainWindowHandle == IntPtr.Zero;
			return !process.Responding;
		}

		static bool ReloadVisualStudioSolutionIfOpened(out bool canOpenScript)
		{
			canOpenScript = false;

			string projectName = GetProjectName();
			IntPtr projectVisualStudioHWND = GetVisualStudioHWNDIfOpenedWithCurrentProject();
			if (projectVisualStudioHWND == IntPtr.Zero)
			{
				canOpenScript = true;
				return false;
			}

			if (IsIconic(projectVisualStudioHWND))
			{
				var succ = ShowWindow(projectVisualStudioHWND, ShowWindowEnum.Restore);
				if (!succ)
					Debug.Log("ShowWindow(projectVisualStudioHWND) failed");
			}
			SetForegroundWindow(projectVisualStudioHWND);

			int maxAttempts = 400;
			int ms = 5;
			int i = 0;
			IntPtr fileModificationDetectedHWND = IntPtr.Zero;
			do
			{
				fileModificationDetectedHWND = FindWindowByCaption(IntPtr.Zero, WINDOW_CAPTION);
				System.Threading.Thread.Sleep(ms);
			}
			while (fileModificationDetectedHWND == IntPtr.Zero && ++i < maxAttempts) ;

			if (fileModificationDetectedHWND == IntPtr.Zero) // found no window modification => stay here to edit (since this is the final goal)
			{
				canOpenScript = true;
				return false;
			}

			SetForegroundWindow(fileModificationDetectedHWND);

			IntPtr buttonPtr = IntPtr.Zero;
			int ii = 0;
			string label = null;
			bool found = false;
			do
			{
				buttonPtr = FindWindowEx(fileModificationDetectedHWND, buttonPtr, "Button", null);
				label = GetWindowText(buttonPtr);
				found = label == "&Reload" || label.ToLower().Contains("reload");
			}
			while (!found && ++ii < 5 /*avoid potential infinite loop*/ && buttonPtr != IntPtr.Zero);

			if (found)
				SendMessage(buttonPtr, 0x00F5 /*BM_CLICK*/, IntPtr.Zero, IntPtr.Zero);
			else
			{
				// shouldn't happen...
			}

			System.Threading.Thread.Sleep(100);

			string winText;
			var unityHWND = FindWindow((win, _) => ((winText = GetWindowText(win)).Contains("Unity ")) && (winText.Contains(".unity - ") || winText.Contains("- Untitled -")/*the current scene is new & not saved*/) && winText.Contains("- " + projectName + " -"));
			if (unityHWND == IntPtr.Zero)
			{
				// TODO
			}
			else
			{
				if (IsIconic(unityHWND))
					ShowWindow(unityHWND, ShowWindowEnum.Restore);
				SetForegroundWindow(unityHWND);
			}

			System.Threading.Thread.Sleep(100);

			//// Send 'Enter'
			//keybd_event(0x0D, 0, 0, 0);
			canOpenScript = true;
			return true;

			//var vsProcesses = System.Diagnostics.Process.GetProcessesByName("devenv");

			//// Exactly one visual studio instance is needed
			//if (vsProcesses.Length != 1)
			//{
			//	Debug.Log("Len=" + vsProcesses.Length);
			//	canOpenScript = true;
			//	return false;
			//}

			//var visualStudioProcess = vsProcesses[0];
			//visualStudioProcess.Refresh();

			//if (visualStudioProcess.MainWindowHandle == IntPtr.Zero)
			//	return false;

			//visualStudioProcess.Refresh();

			//int i = 0;
			////for (; i < 20 && visualStudioProcess.MainWindowHandle == IntPtr.Zero; ++i)
			////{
			////	System.Threading.Thread.Sleep(100);
			////	visualStudioProcess.Refresh();
			////}
			//if (visualStudioProcess.MainWindowHandle != IntPtr.Zero)
			//	Debug.Log("i="+ i + ", " + visualStudioProcess.MainWindowHandle + ", " + visualStudioProcess.Handle);

			//if (visualStudioProcess.MainWindowHandle == IntPtr.Zero)
			//	return false;

			//bool windowShown = false;
			//if (IsIconic(visualStudioProcess.MainWindowHandle))
			//{
			//	// The window is minimized. try to restore it before setting focus
			//	ShowWindow(visualStudioProcess.MainWindowHandle, ShowWindowEnum.Restore);

			//	windowShown = true;
			//}

			//var unityProcess = System.Diagnostics.Process.GetCurrentProcess();
			
			//var sb = GetWindowText(visualStudioProcess.MainWindowHandle);
			//if (sb.Length <= 0)
			//{
			//	if (windowShown && (int)unityProcess.MainWindowHandle != 0)
			//		SetForegroundWindow(unityProcess.MainWindowHandle);

			//	canOpenScript = true;
			//	return false;
			//}
			//Debug.Log("LLL: " + sb + ", " + visualStudioProcess.MainWindowTitle);

			//Debug.Log(sb + "\n" + projectName);
			//if (!sb.Contains(projectName)) // this visual studio doesn't point to our solution => go back
			//{
			//	canOpenScript = true;
			//	if (windowShown)
			//	{
			//		if (unityProcess.MainWindowHandle == IntPtr.Zero) // hidden => show it
			//		{
			//			if (unityProcess.Handle == IntPtr.Zero)
			//				return false;

			//			ShowWindow(unityProcess.Handle, ShowWindowEnum.Restore);
			//		}

			//		if (unityProcess.MainWindowHandle != IntPtr.Zero)
			//			SetForegroundWindow(unityProcess.MainWindowHandle);
			//	}

			//	return false;
			//}

			//SetForegroundWindow(visualStudioProcess.MainWindowHandle);

			//var fileModificationDetectedHWND = FindWindowByCaption(IntPtr.Zero, WINDOW_CAPTION);
			//Debug.Log("fileModificationDetectedHWND="+fileModificationDetectedHWND);
			//if (fileModificationDetectedHWND == IntPtr.Zero) // found no window modification => stay here to edit (since this is the final goal)
			//{
			//	canOpenScript = true;
			//	// Switch back to unity
			//	//var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
			//	//if ((int)currentProcess.MainWindowHandle != 0)
			//	//{
			//	//	SetForegroundWindow(currentProcess.MainWindowHandle);
			//	//}
			//	return false;
			//}

			//SetForegroundWindow(fileModificationDetectedHWND);

			//// Send 'Enter'
			//keybd_event(0x0D, 0, 0, 0);
			//canOpenScript = true;
			//return true;
		}
#endif

		static bool IfPossible_ReloadVisualStudioSolutionIfOpened(out bool canOpenScript)
		{
			canOpenScript = true;
#if SRIA_UNITY_EDITOR_WIN
			try { return ReloadVisualStudioSolutionIfOpened(out canOpenScript); }
			catch { }
#endif
			return false;
		}

		static bool CheckIfPossible_IsVisualStudioOpenedWithCurrentProjectAndBusy()
		{
#if SRIA_UNITY_EDITOR_WIN
			try { return IsVisualStudioOpenedWithCurrentProjectAndBusy(); }
			catch { }
#endif
			return false;
		}
		
		#endregion


		public static bool IsOpen() { return Resources.FindObjectsOfTypeAll(typeof(InitSRIAWindow)).Length > 0; }

		public static void Open(Parameters windowParams)
		{
			InitSRIAWindow windowInstance = GetWindow<InitSRIAWindow>();
			windowInstance.InitWithNewParams(windowParams);
		}

		public static ValidationResult Validate(bool checkForWindows, ScrollRect scrollRect)
		{
			ValidationResult result = new ValidationResult();
			result.scrollRect = scrollRect;

			if (!BaseValidate(out result.reasonIfNotValid))
				return result;

			if (checkForWindows)
			{
				if (CreateSRIAWindow.IsOpen())
				{
					result.reasonIfNotValid = "Creation window already opened";
					return result;
				}
				if (IsOpen())
				{
					result.reasonIfNotValid = "Initialization window already opened";
					return result;
				}
			}

			if (!scrollRect)
			{
				result.reasonIfNotValid = "The provided scrollrect is now null. Maybe it was destroyed meanwhile?";
				return result;
			}

			if (scrollRect.horizontal == scrollRect.vertical)
			{
				result.reasonIfNotValid = "Both 'horizontal' and 'vertical' properties are set to " + scrollRect.horizontal + ". Exactly one needs to be true.";
				return result;
			}

			var existingSRIAComponents = scrollRect.GetComponents(typeof(ISRIA));
			if (existingSRIAComponents.Length > 0)
			{
				string[] s = Array.ConvertAll(existingSRIAComponents, c => " '" + c.GetType().Name + "' ");
				var sc = string.Concat(s);
				result.reasonIfNotValid = "ScrollRect contains " + existingSRIAComponents.Length+" existing component(s) extending SRIA ("+ sc + "). Please remove any existing SRIA component before proceeding";
				return result;
			}

			string requiresADirectViewportChild = "The ScrollRect requires a direct, active child named 'Viewport', which will contain the Content";
#if SRIA_SCROLLRECT_HAS_VIEWPORT
			if (scrollRect.viewport.parent != scrollRect.transform)
			{
				result.reasonIfNotValid = "The 'viewport' property doesn't point to a direct child of the ScrollRect";
				return result;
			}
			if (scrollRect.viewport.name != "Viewport" || !scrollRect.viewport.gameObject.activeSelf)
			{
				result.reasonIfNotValid = requiresADirectViewportChild;
				return result;
			}
			result.viewportRT = scrollRect.viewport;
#else
			var activeChildrenNamedViewport = new List<Transform>();
			foreach (Transform child in scrollRect.transform)
			{
				if (!child.gameObject.activeSelf)
					continue;
				if (child.name == "Viewport")
					activeChildrenNamedViewport.Add(child);
			}

			if (activeChildrenNamedViewport.Count == 0)
			{
				result.reasonIfNotValid = requiresADirectViewportChild;
				return result;
			}

			if (activeChildrenNamedViewport.Count > 1)
			{
				result.reasonIfNotValid = "The ScrollRect has more than one direct, active child named 'Viewport'";
				return result;
			}
			result.viewportRT = activeChildrenNamedViewport[0] as RectTransform;
			if (!result.viewportRT)
			{
				result.reasonIfNotValid = "The ScrollRect's child 'Viewport' does not have a RectTransform component";
				return result;
			}
#endif
			if (!scrollRect.content)
			{
				result.reasonIfNotValid = "The 'content' property is not set";
				return result;
			}

			if (scrollRect.content.parent != result.viewportRT)
			{
				result.reasonIfNotValid = "The 'content' property points to "+scrollRect.content+", which is not a direct child of the ScrollRect";
				return result;
			}

			if (!scrollRect.content.gameObject.activeSelf)
			{
				result.reasonIfNotValid = "The 'content' property points to a game object that's not active";
				return result;
			}

			if (scrollRect.content.childCount > 0)
			{
				result.reasonIfNotValid = "The 'content' property points to a game object that has some children. The content should have none";
				return result;
			}

			var activeChildrenScrollbars = new List<Scrollbar>();
			foreach (Transform child in scrollRect.transform)
			{
				if (!child.gameObject.activeSelf)
					continue;
				var sb = child.GetComponent<Scrollbar>();
				if (sb)
					activeChildrenScrollbars.Add(sb);
			}

			if (activeChildrenScrollbars.Count > 0)
			{
				if (activeChildrenScrollbars.Count > 1)
				{
					result.reasonIfNotValid = "Found more than 1 Scrollbar among the ScrollRect's direct, active children";
					return result;
				}

				result.scrollbar = activeChildrenScrollbars[0];
				bool sbIsHorizontal = result.scrollbar.direction == Scrollbar.Direction.LeftToRight || result.scrollbar.direction == Scrollbar.Direction.RightToLeft;
				if (sbIsHorizontal != scrollRect.horizontal)
				{
					// Only showing a warning, because the user may intentionally set it this way
					result.warning = "Init SRIA: The scrollbar's direction is " + (sbIsHorizontal ? "horizontal" : "vertical") + ", while the ScrollRect is not. If this was intended, ignore this warning";
				}
			}

			result.isValid = true;
			return result;
		}



		protected override void InitWithNewParams(Parameters windowParams)
		{
			base.InitWithNewParams(windowParams);

			// Commented: alraedy done in the constructor with paramater
			//_WindowParams.ResetValues();
			InitializeAfterParamsSet();
			_WindowParams.UpdateAvailableSRIAImplementations(true);
		}

		protected override void InitWithExistingParams()
		{
			//Debug.Log("InitWithExistingParams: _WindowParams.scrollRect=" + _WindowParams.scrollRect + "_WindowParams.scrollbar=" + _WindowParams.scrollbar);
			if (ScheduleCloseIfUndefinedState())
				return;

			base.InitWithExistingParams();
			_WindowParams.InitNonSerialized();
			InitializeAfterParamsSet();

			string scriptName = _WindowParams.generatedScriptNameToUse;
			string fullName;

			if (_State == State.ATTACH_EXISTING_SRIA_PENDING)
			{
				// TODO if have time: create property only for keeping track of the selected template
			}
			else if (_State == State.RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING) // this represents the previous state, so now PRE changes to POST
			{
				string implementationsString = _WindowParams.availableImplementations.Count + ": ";
				foreach (var t in _WindowParams.availableImplementations)
				{
					implementationsString += t.Name + ", ";
				}
				implementationsString = implementationsString.Substring(0, implementationsString.Length - 2);

				if (string.IsNullOrEmpty(scriptName))
				{
					throw new UnityException("SRIA: Internal error: _WindowParams.generatedScriptNameToUse is null after recompilation; " +
						"availableImplementations=" + implementationsString);
				}
				else if ((fullName = GetFullNameIfScriptExists(scriptName)) == null)
				{
					throw new UnityException("SRIA: Internal error: Couldn't find the type's fullName for script '" + scriptName + "'. Did you delete the newly created script?\n " +
						"availableImplementations=" + implementationsString);
				}
				else
				{
					// Commented this is done in initInFirstOnGUI
					//_WindowParams.UpdateAvailableSRIAImplementations();
					int index = _WindowParams.availableImplementations.FindIndex(t => t.FullName == fullName);
					if (index == -1)
					{
						throw new UnityException("SRIA: Internal error: Couldn't find index of new implementation of '" + scriptName + "': " +
							"availableImplementations=" + _WindowParams.availableImplementations.Count + ", " +
							"given fullName=" + fullName);
					}

					_WindowParams.indexOfExistingImplementationToUse = index + 1; // skip the <generate> option
				}

				//_State = State.POST_RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING;
				//_State = State.POST_RECOMPILATION_RELOAD_SOLUTION_PENDING;
				_State = State.NONE;

				// Switch to visual studio to fake-press Reload due to solution being changed
				//if (_WindowParams.openForEdit)// && _WindowParams.indexOfExistingImplementationToUse > 0)
				//ReloadVisualStudioSolutionIfOpenedAndIfPossible();
				//bool b;
				//ReloadVisualStudioSolutionIfOpenedAndIfPossible(out b);

			}
		}
		
		protected override void GetErrorAndWarning(out string error, out string warning)
		{
			var vr = Validate(false, _WindowParams == null ? null : _WindowParams.scrollRect);
			error = vr.reasonIfNotValid;
			warning = vr.warning;
			// TODO check if prefab is allowed and if the prefab is NOT the viewport, scrollrect content, scrollbar
		}

		protected override void UpdateImpl()
		{
			ScheduleCloseIfUndefinedState(); // do not return in case of true, since the close code is below

			switch (_State)
			{
				case State.CLOSE_PENDING:
					_State = State.NONE;
					Close();
					break;

				case State.RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING:
					// TODO think about if need to wait or something. maybe import/refresh assets
					break;

				//case State.POST_RECOMPILATION_RELOAD_SOLUTION_PENDING:
				//	//bool canOpenScript;
				//	//if (ReloadVisualStudioSolutionIfOpenedAndIfPossible(out canOpenScript) || canOpenScript)
				//	if (!CheckIfPossible_IsVisualStudioOpenedWithCurrentProjectAndBusy())
				//		_State = State.NONE;
				//	break;

				case State.ATTACH_EXISTING_SRIA_PENDING:
				//case State.POST_RECOMPILATION_ATTACH_GENERATED_SRIA_PENDING:
					if (!_WindowParams.ImplementationsInitialized)
						throw new UnityException("wat. it shold've been initialized in initwithexistingparams");

					ConfigureScrollView(_WindowParams.scrollRect, _WindowParams.viewportRT, _WindowParams.itemPrefab);
					Canvas.ForceUpdateCanvases();
					if (_WindowParams.useScrollbar && _WindowParams.GenerateDefaultScrollbar)
						_WindowParams.scrollbar = InstantiateDefaultSRIAScrollbar();
					if (_WindowParams.scrollbar)
						ConfigureScrollbar();
					_WindowParams.scrollRect.gameObject.AddComponent(_WindowParams.ExistingImplementationToUse);
					// Selcting the game object is important. Unity starts the initial serialization of a script (and thus, setting a valid value to the SRIA's _Params field)
					// only if its inspector is shown
					Selection.activeGameObject = _WindowParams.scrollRect.gameObject; 
					_State = State.POST_ATTACH_CONFIGURE_SRIA_PENDING;
					break;

				case State.POST_ATTACH_CONFIGURE_SRIA_PENDING:
					if (_WindowParams == null || !_WindowParams.scrollRect)
					{
						_State = State.CLOSE_PENDING;
						break;
					}

					var isria = _WindowParams.scrollRect.GetComponent(typeof(ISRIA)) as ISRIA;
					if (isria == null)
					{
						_State = State.CLOSE_PENDING;
						break;
					}

					if (isria.BaseParameters == null)
						break;

					OnSRIAParamsInitialized(isria);

					if (_WindowParams.openForEdit)
					{
						var monoScript = MonoScript.FromMonoBehaviour(isria as MonoBehaviour);
						var success = AssetDatabase.OpenAsset(monoScript);
						if (success)
						{
						//	ReloadVisualStudioSolutionIfOpenedAndIfPossible();
						}
						else
							Debug.Log("SRIA: Could not open '" + isria.GetType().Name + "' in external code editor");
					}

					_State = State.PING_SCROLL_RECT_PENDING;
					break;

				//case State.POST_ATTACH_AND_POST_PING_CONFIGURE_SRIA_PARAMS_PENDING:
				//	if (ConfigureSRIAParamsPostAttachAndPostPing())
				//		_State = State.CLOSE_PENDING;
				//	break;
			}
		}

		protected override void OnGUIImpl()
		{
			if (ScheduleCloseIfUndefinedState())
				return;

			switch (_State)
			{
				case State.PING_SCROLL_RECT_PENDING: // can only be done in OnGUI because EditorStyles are used by EditorGUIUtility.PingObject
					if (_WindowParams.scrollRect)
					{
						PingAndSelect(_WindowParams.scrollRect);
						//ShowNotification(new GUIContent("SRIA: Initialized"));

						string msg = "SRIA: Initialized";
						bool shownNotification = false;
						try
						{
							var inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
							var allInspectors = Resources.FindObjectsOfTypeAll(inspectorWindowType);
							if (allInspectors != null && allInspectors.Length == 1)
							{
								(allInspectors[0] as EditorWindow).ShowNotification(new GUIContent(msg));
								shownNotification = true;
							}
						}
						catch { }

						if (!shownNotification)
							Debug.Log(msg);
					}
					else
						Debug.Log("SRIA: Unexpected state: the scrollrect was destroyed meanwhile. Did you delete it from the scene?");

					_State = State.CLOSE_PENDING;

					break;

				case State.RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING:
					EditorGUI.BeginDisabledGroup(true);
					{
						EditorGUILayout.BeginHorizontal(_BoxGUIStyle);
						{
							string scriptName = "(???)";
							if (_WindowParams != null && !string.IsNullOrEmpty(_WindowParams.generatedScriptNameToUse))
								scriptName = _WindowParams.generatedScriptNameToUse;
							scriptName = "'" + scriptName + "'";

							var style = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
							EditorGUILayout.LabelField("Waiting for script " + scriptName + " to be generated & attached...", style);
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUI.EndDisabledGroup();

					break;

				case State.POST_ATTACH_CONFIGURE_SRIA_PENDING:
					EditorGUI.BeginDisabledGroup(true);
					{
						string s = _WindowParams != null && _WindowParams.scrollRect != null ? "(named '" + _WindowParams.scrollRect.name + "')" : "";
						EditorGUILayout.LabelField(
							"If this window stays open for too long, please select the newly initialized ScrollView in hierarchy " + s + "\n" +
							"This is done automatically, but it fails if you have locked the inspector window",
							GUILayout.Height(100f)
							);
					}
					EditorGUI.EndDisabledGroup();

					break;

				case State.PING_PREFAB_PENDING:
					if (_WindowParams != null && _WindowParams.itemPrefab)
						PingAndSelect(_WindowParams.itemPrefab);

					_State = State.NONE;
					goto case State.NONE;

				case State.NONE:
					if (_WindowParams.ImplementationsInitialized) // wait for params intialization
						DrawDefaultGUI();
					break; // continue drawing normally

				default:
					break;
			}

		}

		protected override void ConfigureScrollView(ScrollRect scrollRect, RectTransform viewport, params Transform[] objectsToSkipDisabling)
		{
			base.ConfigureScrollView(scrollRect, viewport, objectsToSkipDisabling);
		}

		protected override void OnSubmitClicked()
		{
			// Commented: this is already checked and if there's and error, the submit button is disabled
			//// Validate again, to make sure the hierarchy wasn't modified
			//var validationRes = Validate(_WindowParams.scrollRect);
			//if (!validationRes.isValid)
			//{
			//	C.ShowCouldNotExecuteCommandNotification(this);
			//	Debug.Log("SRIA: Could not initialize (the hierarchy was probably modified): " + validationRes.reasonIfNotValid);
			//	return;
			//}

			bool generateNew = _WindowParams.ExistingImplementationToUse == null;
			if (generateNew)
			{
				if (string.IsNullOrEmpty(_WindowParams.generatedScriptNameToUse))
				{
					CWiz.ShowNotification("Invalid script name", true, this);
					return;
				}

				string alreadyExistingTypeFullName = GetFullNameIfScriptExists(_WindowParams.generatedScriptNameToUse);
				if (alreadyExistingTypeFullName != null)
				{
					CWiz.ShowNotification( "Invalid script name. A script already exists as '" + alreadyExistingTypeFullName + "'", true, this);
					return;
				}

				string genScriptDirectoryPath = Application.dataPath + "/Scripts";
				string genScriptPath = genScriptDirectoryPath + "/" + _WindowParams.generatedScriptNameToUse + ".cs";

				if (File.Exists(genScriptPath))
				{
					CWiz.ShowNotification("A script named '" + _WindowParams.generatedScriptNameToUse + "' already exists", true, this);
					return;
				}

				if (!Directory.Exists(genScriptDirectoryPath))
				{
					try { Directory.CreateDirectory(genScriptDirectoryPath); }
					catch
					{
						Debug.LogError("SRIA: Could not create directory: " + genScriptDirectoryPath);
						return;
					}
				}

				string templateText = _WindowParams.TemplateToUseForNewScript;

				// Replace the class name with the chosen one
				templateText = templateText.Replace(
					CWiz.TEMPLATE_TEXT_CLASSNAME_PREFIX + _WindowParams.availableTemplatesNames[_WindowParams.IndexOfTemplateToUseForNewScript],
					CWiz.TEMPLATE_TEXT_CLASSNAME_PREFIX + _WindowParams.generatedScriptNameToUse
				);

				// Add header
				templateText = _WindowParams.TemplateHeader + templateText;

				// Create unique namespace. Even if we're checking for any existing monobehaviour with the same name before creating a new one, 
				// the params, views holder and the model classes still have the same name
				CWiz.ReplaceTemplateDefaultNamespaceWithUnique(ref templateText);

				// Create, import and wait for recompilation
				try { File.WriteAllText(genScriptPath, templateText); }
				catch
				{
					CWiz.ShowCouldNotExecuteCommandNotification(this);
					Debug.LogError("SRIA: Could not create file: " + genScriptPath);
					return;
				}
				// ImportAssetOptions
				//var v = AssetImporter.GetAtPath(FileUtil.GetProjectRelativePath(genScriptPath));
				//Debug.Log("v.GetInstanceID()" + v.GetInstanceID());
				//Debug.Log(FileUtil.GetProjectRelativePath(genScriptPath)+", " + genScriptPath);
				//AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(genScriptPath));
				//AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(genScriptPath), ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
				AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(genScriptPath));
				//AssetDatabase.Refresh();
				// Will be executed in Update, but after re-compilation
				// TODO check
				_State = State.RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING;
			}
			else
				// Will be executed in the next Update
				_State = State.ATTACH_EXISTING_SRIA_PENDING;
		}

		bool ScheduleCloseIfUndefinedState()
		{
			if (!_WindowParams.scrollRect)
			{
				if (_State != State.CLOSE_PENDING)
				{
					_State = State.CLOSE_PENDING;
					Debug.Log("SRIA wizard closed because the ScrollRect was destroyed or the scene changed");
				}
				//C.ShowNotification("SRIA wizard closed because the ScrollRect was destroyed", false, false);
				//DestroyImmediate(this);
				return true;
			}

			return false;
		}

		void InitializeAfterParamsSet()
		{
			_MapParamBaseTypeToPrefabSetter = new Dictionary<Type, Action<BaseParams, RectTransform>>();
			_MapParamBaseTypeToPrefabSetter[typeof(GridParams)] = (parms, pref) => (parms as GridParams).cellPrefab = pref;
			_MapParamBaseTypeToPrefabSetter[typeof(BaseParamsWithPrefab)] = (parms, pref) => (parms as BaseParamsWithPrefab).itemPrefab = pref;
		}

		RectTransform GetItemPrefabResourceForParamsBaseType(Type type)
		{
			string nameToUse;
			if (type == typeof(GridParams))
				nameToUse = "GridSRIA";
			else if (type == typeof(BaseParamsWithPrefab))
				nameToUse = "ListSRIA";
			else
				return null;

			var go = Resources.Load<GameObject>(CWiz.GetExampleItemPrefabResPath(nameToUse));
			if (!go)
				return null;

			return go.transform as RectTransform;
		}

		void DrawDefaultGUI()
		{
			DrawSectionTitle("Implement SRIA");

			// Game Object to initialize
			DrawObjectWithPath(_BoxGUIStyle, "ScrollRect to initialize", _WindowParams.scrollRect == null ? null : _WindowParams.scrollRect.gameObject);

			// Scrollbar
			EditorGUILayout.BeginVertical(_BoxGUIStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Scrollbar", EditorStyles.boldLabel, CWiz.LABEL_WIDTH);
					_WindowParams.useScrollbar = EditorGUILayout.Toggle(_WindowParams.useScrollbar, CWiz.VALUE_WIDTH);
				}
				EditorGUILayout.EndHorizontal();

				if (_WindowParams.useScrollbar)
				{
					EditorGUILayout.Space();

					if (_WindowParams.MiscScrollbarWasAlreadyPresent)
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Generate scrollbar", CWiz.LABEL_WIDTH);
							_WindowParams.overrideMiscScrollbar = EditorGUILayout.Toggle("", _WindowParams.overrideMiscScrollbar, CWiz.VALUE_WIDTH);
						}
						EditorGUILayout.EndHorizontal();
						_WindowParams.scrollbar.gameObject.SetActive(!_WindowParams.overrideMiscScrollbar);
					}

					if (!_WindowParams.MiscScrollbarWasAlreadyPresent || _WindowParams.overrideMiscScrollbar)
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Scrollbar position", CWiz.LABEL_WIDTH);
							_WindowParams.isScrollbarPosAtStart =
								GUILayout.SelectionGrid(
									_WindowParams.isScrollbarPosAtStart ? 0 : 1,
									_WindowParams.isHorizontal ? new string[] { "Top", "Bottom" } : new string[] { "Left", "Right" },
									2,
									CWiz.VALUE_WIDTH
								) == 0 ? true : false;
						}
						EditorGUILayout.EndHorizontal();
					}

					if (_WindowParams.MiscScrollbarWasAlreadyPresent)
					{
						EditorGUILayout.HelpBox
						(
							_WindowParams.overrideMiscScrollbar ?
								"'" + _WindowParams.scrollbar.name + "' was disabled. The default scrollbar will be generated"
								:
								"An existing scrollbar was found ('" + _WindowParams.scrollbar.name + "') and it'll be automatically linked to SRIA. " +
								"If you want to disable it & generate the default one instead, tick 'Generate scrollbar'", MessageType.Info
						);
					}
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			// SRIA implementation
			EditorGUILayout.BeginVertical(_BoxGUIStyle);
			{
				EditorGUILayout.LabelField("Script to use", EditorStyles.boldLabel, CWiz.LABEL_WIDTH);

				EditorGUILayout.Space();

				// Exclude examples/demos toggle
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Exclude examples/demos", CWiz.LABEL_WIDTH);
					var before = _WindowParams.excludeExampleImplementations;
					_WindowParams.excludeExampleImplementations = EditorGUILayout.Toggle(_WindowParams.excludeExampleImplementations, CWiz.VALUE_WIDTH);
					if (_WindowParams.excludeExampleImplementations != before)
						_WindowParams.UpdateAvailableSRIAImplementations(true);
				}
				EditorGUILayout.EndHorizontal();
				if (!_WindowParams.excludeExampleImplementations)
					EditorGUILayout.HelpBox("Using the provided example/demo scripts have no use in production. Intead, use them as a guide for implementing your own", MessageType.Warning);

				// Implementation to use
				var indexBefore = _WindowParams.indexOfExistingImplementationToUse;
				_WindowParams.indexOfExistingImplementationToUse =
					EditorGUILayout.Popup(_WindowParams.indexOfExistingImplementationToUse, _WindowParams.availableImplementationsStringsOptions, GUILayout.Width(CWiz.VALUE_WIDTH2_FLOAT));

				// When the user manually switches from generate to existing, don't keep the value of "openForEdit"
				if (indexBefore != _WindowParams.indexOfExistingImplementationToUse && _WindowParams.indexOfExistingImplementationToUse > 0)
					_WindowParams.openForEdit = false;

				// SRIA template to use if need to generate new implementation
				if (_WindowParams.indexOfExistingImplementationToUse == 0)
				{
					if (_WindowParams.availableTemplates.Length == 0)
						EditorGUILayout.HelpBox("There are no templates in */Resources/" + CWiz.TEMPLATE_SCRIPTS_RESPATH + ". Did you manually delete them?", MessageType.Error);
					else
					{
						_WindowParams.IndexOfTemplateToUseForNewScript =
							GUILayout.SelectionGrid(_WindowParams.IndexOfTemplateToUseForNewScript, _WindowParams.availableTemplatesNames, 3, GUILayout.MinWidth(CWiz.VALUE_WIDTH_FLOAT));

						// Script name
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Generated script name", CWiz.LABEL_WIDTH);
							_WindowParams.generatedScriptNameToUse = EditorGUILayout.TextField(_WindowParams.generatedScriptNameToUse, CWiz.VALUE_WIDTH);

							// Name validation
							var filteredChars = new List<char>(_WindowParams.generatedScriptNameToUse.ToCharArray());
							filteredChars.RemoveAll(c => !char.IsLetterOrDigit(c));
							while (filteredChars.Count > 0 && char.IsDigit(filteredChars[0]))
								filteredChars.RemoveAt(0);
							_WindowParams.generatedScriptNameToUse = new string(filteredChars.ToArray());
						}
						EditorGUILayout.EndHorizontal();

						// Open for edit toggle
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Open for edit", CWiz.LABEL_WIDTH);
							_WindowParams.openForEdit = EditorGUILayout.Toggle(_WindowParams.openForEdit, CWiz.VALUE_WIDTH);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				else
				{
					if (_WindowParams.availableImplementations == null)
					{
						// TODO: shouldn't happen though
					}
					else
					{
						// Prefab, if applicable
						var implToUse = _WindowParams.ExistingImplementationToUse;
						Type paramsType = GetBaseTypeOfPrefabContainingParams(implToUse);
						if (paramsType == null)
						{
							EditorGUILayout.HelpBox(
								"Couldn't detect the params of '" + implToUse.Name + "' to set the prefab. Make sure to manually set it after, in inspector or (advanced) in code",
								MessageType.Warning
							);
						}
						else
						{
							EditorGUILayout.HelpBox(
								"Params are of type '" + paramsType.Name + "', which contain a prefab property" +
								(_WindowParams.itemPrefab != null ? ":" : ". If you don't set it here, make sure to do it after, through inspector or (advanced) in code"),
								MessageType.Info
							);

							EditorGUILayout.BeginHorizontal();
							{
								EditorGUILayout.LabelField("Item prefab", CWiz.LABEL_WIDTH);
								_WindowParams.itemPrefab = EditorGUILayout.ObjectField(_WindowParams.itemPrefab, typeof(RectTransform), true, CWiz.VALUE_WIDTH) as RectTransform;

								if (!_WindowParams.itemPrefab)
								{
									var itemPreabRes = GetItemPrefabResourceForParamsBaseType(paramsType);
									if (itemPreabRes)
									{
										var buttonRect = EditorGUILayout.GetControlRect(GUILayout.Width(200f), GUILayout.Height(20f));
										if (GUI.Button(buttonRect, "Generate example for " + itemPreabRes.name.Replace("ItemPrefab", "").Replace("(Example)", "")))
										{
											var instanceRT = (Instantiate(itemPreabRes.gameObject) as GameObject).GetComponent<RectTransform>();
											instanceRT.name = instanceRT.name.Replace("(Clone)", "");
											instanceRT.SetParent(_WindowParams.ScrollRectRT, false);
											_WindowParams.itemPrefab = instanceRT;
											_State = State.PING_PREFAB_PENDING;
										}
									}
								}
							}
							EditorGUILayout.EndHorizontal();
						}
					}
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			// Create button
			DrawSubmitButon(_WindowParams.ExistingImplementationToUse == null ? "Generate script" : "Initialize");
		}

		string GetFullNameIfScriptExists(string scriptName)
		{
			scriptName = scriptName.ToLower();
			string result = null;
			if (_AllMonobehaviours.Count == 0)
			{
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var type in assembly.GetTypes())
					{
						if (type.IsAbstract)
							continue;
						if (!type.IsClass)
							continue;
						if (type.IsGenericType)
							continue;
						if (type.IsNotPublic)
							continue;
						if (!typeof(MonoBehaviour).IsAssignableFrom(type))
							continue;

						_AllMonobehaviours.Add(type);
						if (type.Name.ToLower() == scriptName && result == null)
							result = type.FullName;
					}
				}
			}
			else
			{
				var found = _AllMonobehaviours.Find(t => t.Name.ToLower() == scriptName);
				if (found != null)
					result = found.FullName;
			}

			return result;
		}

		Type GetBaseTypeOfPrefabContainingParams(Type derivedType)
		{
			var curDerivedType = derivedType;
			var prefabContainingParamsTypes = new List<Type>(_MapParamBaseTypeToPrefabSetter.Keys);
			while (curDerivedType != null && curDerivedType != typeof(object))
			{
				var genericArguments = curDerivedType.GetGenericArguments();
				var tParams = new List<Type>(genericArguments).Find(t => typeof(BaseParams).IsAssignableFrom(t));

				if (tParams != null)
				{
					Type type = prefabContainingParamsTypes.Find(t => CWiz.IsSubclassOfRawGeneric(t, tParams));
					if (type != null)
						return type;
				}

				curDerivedType = curDerivedType.BaseType;
			}

			return null;
		}

		bool SetPrefab(ISRIA isria, RectTransform prefab)
		{
			var type = GetBaseTypeOfPrefabContainingParams(isria.GetType());
			if (type == null)
				return false;

			_MapParamBaseTypeToPrefabSetter[type](isria.BaseParameters, prefab);

			return true;
		}

		Scrollbar InstantiateDefaultSRIAScrollbar()
		{
			var respath = _WindowParams.isHorizontal ? CWiz.HOR_SCROLLBAR_RESPATH : CWiz.VERT_SCROLLBAR_RESPATH;
			var sbPrefab = Resources.Load<GameObject>(respath);
			var sbInstanceRT = (GameObject.Instantiate(sbPrefab) as GameObject).transform as RectTransform;
			sbInstanceRT.name = sbInstanceRT.name.Replace("(Clone)", "");
			sbInstanceRT.SetParent(_WindowParams.ScrollRectRT, false);

			return sbInstanceRT.GetComponent<Scrollbar>();
		}
		
		void ConfigureScrollbar()
		{
			var instanceScollbarFixer8 = _WindowParams.scrollbar.GetComponent<ScrollbarFixer8>();
			if (!instanceScollbarFixer8)
				instanceScollbarFixer8 = _WindowParams.scrollbar.gameObject.AddComponent<ScrollbarFixer8>();
			instanceScollbarFixer8.scrollRect = _WindowParams.scrollRect;
			instanceScollbarFixer8.viewport = _WindowParams.viewportRT;

			DisableOrNotifyAboutMiscComponents(_WindowParams.scrollbar.gameObject, "scrollbar", typeof(ScrollbarFixer8), typeof(Scrollbar));

			//if (_WindowParams.scrollbarIsFromSRIAPrefab)
			if (_WindowParams.GenerateDefaultScrollbar)
			{
				// The scrollbar is initially placed at end if it's from the default scrollbar prefab 
				if (_WindowParams.isScrollbarPosAtStart)
				{
					var sbInstanceRT = _WindowParams.ScrollbarRT;
					var newAnchPos = sbInstanceRT.anchoredPosition;
					int i = 1 - _WindowParams.Hor0_Vert1;
					var v = sbInstanceRT.anchorMin;
					v[i] = 1f - v[i];
					sbInstanceRT.anchorMin = v;
					v = sbInstanceRT.anchorMax;
					v[i] = 1f - v[i];
					sbInstanceRT.anchorMax = v;
					v = sbInstanceRT.pivot;
					v[i] = 1f - v[i];
					sbInstanceRT.pivot = v;
					newAnchPos[i] = -newAnchPos[i];
					sbInstanceRT.anchoredPosition = newAnchPos;
				}
			}
		}

		void PingAndSelect(Component c)
		{
			Selection.activeGameObject = c.gameObject;
			EditorGUIUtility.PingObject(c);
		}

		void OnSRIAParamsInitialized(ISRIA isria)
		{
			//if (_WindowParams == null || _WindowParams.scrollRect == null)
			//	return true;

			//var isria = _WindowParams.scrollRect.GetComponent(typeof(ISRIA)) as ISRIA;
			//if (isria == null)
			//	return true; // shouldn't happen

			var baseParams = isria.BaseParameters;
			//if (baseParams == null)
			//	return false; // wait until params initialized

			baseParams.contentSpacing = 10f;
			baseParams.contentPadding = new RectOffset(10, 10, 10, 10);

			var gridParams = baseParams as GridParams;
			if (gridParams != null)
			{
				gridParams.numCellsPerGroup = 3;
				gridParams.groupPadding = _WindowParams.isHorizontal ? new RectOffset(0, 0, 10, 10) : new RectOffset(10, 10, 0, 0);
				gridParams.alignmentOfCellsInGroup = TextAnchor.MiddleCenter;
			}

			baseParams.viewport = _WindowParams.viewportRT;
			if (_WindowParams.itemPrefab)
			{
				var success = SetPrefab(isria, _WindowParams.itemPrefab);
				if (!success)
					Debug.Log("SRIA: Could not set the item prefab for '" + isria.GetType().Name + "'. Make sure to manually set it through inspector or (advanced) in code");
			}
		}


		[Serializable]
		public class Parameters : BaseWindowParams, ISerializationCallbackReceiver
		{
#region Serialization
			public ScrollRect scrollRect;
			public RectTransform viewportRT;
			public Scrollbar scrollbar;

			// View state
			public bool useScrollbar, isScrollbarPosAtStart, overrideMiscScrollbar;
			public bool excludeExampleImplementations;
			public int indexOfExistingImplementationToUse;
			public string generatedScriptNameToUse;
			public RectTransform itemPrefab;
			public bool openForEdit;

			[SerializeField]
			int _IndexOfTemplateToUseForNewScript;
			[SerializeField]
			bool _MiscScrollbarWasAlreadyPresent;
#endregion

			[NonSerialized]
			public string[] availableTemplates;
			[NonSerialized]
			public string[] availableTemplatesNames;

			public string TemplateHeader
			{
				get
				{
					if (_TemplateHeader == null)
					{
						var headerComment = Resources.Load<TextAsset>(CWiz.TEMPLATE_SCRIPTS_HEADERCOMMENT_RESPATH);
						_TemplateHeader = headerComment.text;
					}

					return _TemplateHeader;
				}
			}
			[NonSerialized]
			public List<Type> availableImplementations;
			[NonSerialized]
			public string[] availableImplementationsStringsOptions;

			public override Vector2 MinSize { get { return new Vector2(700f, 500f); } }
			public bool ImplementationsInitialized { get { return availableImplementations != null; } }
			public bool MiscScrollbarWasAlreadyPresent { get { return _MiscScrollbarWasAlreadyPresent; } }

			public int IndexOfTemplateToUseForNewScript
			{
				get { return _IndexOfTemplateToUseForNewScript; }
				set
				{
					if (_IndexOfTemplateToUseForNewScript != value)
					{
						_IndexOfTemplateToUseForNewScript = value;
						generatedScriptNameToUse = null;
					}

					if (generatedScriptNameToUse == null && value >= 0)
						generatedScriptNameToUse = availableTemplatesNames[value];
				}
			}
			public string TemplateToUseForNewScript { get { return IndexOfTemplateToUseForNewScript < 0 ? null : availableTemplates[IndexOfTemplateToUseForNewScript]; } }
			//public string TemplateNameToUse { get { return indexOfTemplateToUse < 1 ? null : availableTemplatesNames[indexOfTemplateToUse - 1]; } }
			public Type ExistingImplementationToUse { get { return indexOfExistingImplementationToUse < 1 ? null : availableImplementations[indexOfExistingImplementationToUse - 1]; } }
			public bool GenerateDefaultScrollbar { get { return !MiscScrollbarWasAlreadyPresent || overrideMiscScrollbar; } }
			public RectTransform ScrollRectRT { get { return scrollRect.transform as RectTransform; } }
			public RectTransform ScrollbarRT { get { return scrollbar.transform as RectTransform; } }

			const string DEFAULT_TEMPLATE_TO_USE_FOR_NEW_SCRIPT_IF_EXISTS = "ListSRIA";

			string _TemplateHeader;


			public Parameters() { } // For unity serialization

			public Parameters(ValidationResult validationResult)
			{
				scrollRect = validationResult.scrollRect;
				viewportRT = validationResult.viewportRT;
				scrollbar = validationResult.scrollbar;
				_MiscScrollbarWasAlreadyPresent = scrollbar != null;
				ResetValues();
				InitNonSerialized();
			}


#region ISerializationCallbackReceiver implementation
			public void OnBeforeSerialize() { }
			//public void OnAfterDeserialize() { InitNonSerialized(); }
			// Commented: "Load is not allowed to be called durng serialization"
			public void OnAfterDeserialize() { }
#endregion

			public void InitNonSerialized()
			{
				var allTemplatesTextAssets = Resources.LoadAll<TextAsset>(CWiz.TEMPLATE_SCRIPTS_RESPATH);
				availableTemplatesNames = new string[allTemplatesTextAssets.Length];
				availableTemplates = new string[allTemplatesTextAssets.Length];
				for (int i = 0; i < allTemplatesTextAssets.Length; i++)
				{
					var ta = allTemplatesTextAssets[i];
					availableTemplatesNames[i] = ta.name;
					availableTemplates[i] = ta.text;
				}
				if (_IndexOfTemplateToUseForNewScript >= allTemplatesTextAssets.Length)
					_IndexOfTemplateToUseForNewScript = allTemplatesTextAssets.Length - 1;

				UpdateAvailableSRIAImplementations(false);
				if (indexOfExistingImplementationToUse >= availableImplementationsStringsOptions.Length)
					indexOfExistingImplementationToUse = availableImplementationsStringsOptions.Length - 1;
			}

			public override void ResetValues()
			{
				base.ResetValues();

				isHorizontal = scrollRect.horizontal;
				useScrollbar = MiscScrollbarWasAlreadyPresent;
				overrideMiscScrollbar = false;
				isScrollbarPosAtStart = false;

				// SRIA implementation
				excludeExampleImplementations = true;
				indexOfExistingImplementationToUse = 0; // create new
				//ResetIndexOfTemplateToUse();
				_IndexOfTemplateToUseForNewScript = -1;
				//itemPrefab = null;
				openForEdit = true;
			}

			public void UpdateAvailableSRIAImplementations(bool resetSelectedTemplateAndImplementation)
			{
				if (availableImplementations == null)
					availableImplementations = new List<Type>();
				else
					availableImplementations.Clear();
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var type in assembly.GetTypes())
					{
						if (type.IsAbstract)
							continue;
						if (!type.IsClass)
							continue;
						if (type.IsGenericType)
							continue;
						if (type.IsNested)
							continue;
						if (type.IsNotPublic)
							continue;
						if (!CWiz.IsSubclassOfSRIA(type))
							continue;
						if (excludeExampleImplementations
							&& (
								type.Name.ToLower().Contains("example")
								|| type.Name == typeof(SimpleTutorialExample.SimpleTutorial).Name
								|| type.Name == typeof(Logic.Misc.Visual.UI.DateTimePicker.DateTimePickerAdapter).Name
							))
							continue;

						availableImplementations.Add(type);
					}
				}
				availableImplementationsStringsOptions = new string[availableImplementations.Count + 1];
				availableImplementationsStringsOptions[0] = "<Generate new from template>";
				for (int i = 0; i < availableImplementations.Count; i++)
					availableImplementationsStringsOptions[i+1] = availableImplementations[i].Name;

				if (resetSelectedTemplateAndImplementation)
				{
					indexOfExistingImplementationToUse = 0; // default to create new
					ResetIndexOfTemplateToUse();
				}
			}

			void ResetIndexOfTemplateToUse()
			{
				int index = 0;
				if (availableTemplates != null)
					index = Array.IndexOf(availableTemplates, DEFAULT_TEMPLATE_TO_USE_FOR_NEW_SCRIPT_IF_EXISTS); // -1 if not exists
				if (index == -1 && availableTemplates.Length > 0) // ..but 0 if there are others
					index = 0;
				IndexOfTemplateToUseForNewScript = index;
			}

		}


		public class ValidationResult
		{
			public bool isValid;
			public string reasonIfNotValid;
			public string warning;
			public RectTransform viewportRT;
			public Scrollbar scrollbar;
			public ScrollRect scrollRect;

			public override string ToString()
			{
				return	"isValid = " + isValid + "\n" +
						"viewportRT = " + (viewportRT == null ? "(null)" : viewportRT.name) + "\n" +
						"scrollbar = " + (scrollbar == null ? "(null)" : scrollbar.name) + "\n" +
						"scrollRect = " + (scrollRect == null ? "(null)" : scrollRect.name) + "\n";
			}
		}


		enum State
		{
			NONE,
			//POST_RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING,
			RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING,
			//POST_RECOMPILATION_RELOAD_SOLUTION_PENDING,
			ATTACH_EXISTING_SRIA_PENDING,
			POST_ATTACH_CONFIGURE_SRIA_PENDING,
			PING_SCROLL_RECT_PENDING,
			//POST_ATTACH_AND_POST_PING_CONFIGURE_SRIA_PARAMS_PENDING,
			PING_PREFAB_PENDING,
			//PING_PREFAB_PENDING_STEP_2,
			CLOSE_PENDING
		}
	}
}
