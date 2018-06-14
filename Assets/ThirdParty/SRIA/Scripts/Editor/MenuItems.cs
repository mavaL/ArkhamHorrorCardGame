using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Editor
{
    static class MenuItems
	{
		[MenuItem("frame8/SRIA/Code reference")]
		public static void OpenDoc()
		{ Application.OpenURL("http://thefallengames.com/unityassetstore/optimizedscrollviewadapter/doc"); }

		[MenuItem("frame8/SRIA/Quick start tutorial (YouTube)")]
		public static void OpenTutorial()
		{ Application.OpenURL("https://youtu.be/rcgnF16JybY"); }

		[MenuItem("frame8/SRIA/Thank you!")]
		public static void OpenThankYou()
		{ Application.OpenURL("http://thefallengames.com/unityassetstore/optimizedscrollviewadapter/thankyou"); }

		[MenuItem("frame8/SRIA/Ask us a question")]
		public static void AskQuestion()
		{ Application.OpenURL("https://forum.unity.com/threads/30-off-optimized-scrollview-adapter-listview-gridview.395224"); }

		[MenuItem("CONTEXT/ScrollRect/Optimize with SRIA")]
		static void OptimizeSelectedScrollRectWithSRIA(MenuCommand command)
		{
			ScrollRect scrollRect = (ScrollRect)command.context;
			var validationResult = InitSRIAWindow.Validate(true, scrollRect);
			// Manually checking for validation, as this provides richer info about the case when initialization is not possible
			if (!validationResult.isValid)
			{
				CWiz.ShowCouldNotExecuteCommandNotification(null);
				Debug.Log("SRIA: Could not optimize '" + scrollRect.name + "': " + validationResult.reasonIfNotValid);
				return;
			}

			InitSRIAWindow.Open(new InitSRIAWindow.Parameters(validationResult));
		}

		[MenuItem("GameObject/UI/Optimized ScrollView (SRIA)", false, 10)]
		static void CreateSRIA(MenuCommand menuCommand)
		{

			string reasonIfNotValid;
			// Manually checking for validation, as this provides richer info about the case when creation is not possible
			if (!CreateSRIAWindow.Validate(true, out reasonIfNotValid))
			{
				CWiz.ShowCouldNotExecuteCommandNotification(null);
				Debug.Log("SRIA: Could not create ScrollView on the selected object: " + reasonIfNotValid);
				return;
			}

			CreateSRIAWindow.Open(new CreateSRIAWindow.Parameters());
		}


	}
}
