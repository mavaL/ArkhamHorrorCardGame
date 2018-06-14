using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class SRIATitle : MonoBehaviour
	{
		void Start() { GetComponent<Text>().text = "Optimized ScrollView Adapter v" + C.SRIA_VERSION_STRING; }
	}
}