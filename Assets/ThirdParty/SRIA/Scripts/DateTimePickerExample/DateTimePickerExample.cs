using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.Logic.Misc.Other.Extensions;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.Logic.Misc.Visual.UI.DateTimePicker;

namespace frame8.ScrollRectItemsAdapter.DateTimePickerExample
{
	/// <summary>Implementing multiple adapters to get a generic picker which returns a <see cref="DateTime"/> object</summary>
	public class DateTimePickerExample : MonoBehaviour
	{
		void Start()
        {
			DrawerCommandPanel.Instance.Init(new ISRIA[0], false, false, false, false, false);
			DrawerCommandPanel.Instance.AddButtonsPanel("Show another").button1.onClick.AddListener(Show);
			//DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = .2f;
			DrawerCommandPanel.Instance.simulateLowEndDeviceSetting.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.galleryEffectSetting.gameObject.SetActive(false);

			//// No adding/removing at the head of the list
			//DrawerCommandPanel.Instance.addRemoveOnePanel.button2.gameObject.SetActive(false);
			//DrawerCommandPanel.Instance.addRemoveOnePanel.button4.gameObject.SetActive(false);

			////// Disable setCount while snapping animation is running
			////var snapper = GetComponent<Snapper8>();
			////if (snapper)
			////{
			////	snapper.SnappingStarted += () => DrawerCommandPanel.Instance.setCountPanel.button.interactable = false;
			////	snapper.SnappingEndedOrCancelled += () => DrawerCommandPanel.Instance.setCountPanel.button.interactable = true;
			////}

			//DrawerCommandPanel.Instance.ItemCountChangeRequested += ChangeItemsCountWithChecks;
			//// Initially set the number of items to the number in the input field
			//DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
			Show();
		}

		public void Show() { DateTimePicker8.Show(null); }
	}
}
