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
	public class ShowDateTimePickerButton : MonoBehaviour
	{
		void Start()
        {
			var b = GetComponent<Button>();
			if (!b)
				b = gameObject.AddComponent<Button>();
			b.onClick.AddListener(() => DateTimePicker8.Show(null));
		}
	}
}
