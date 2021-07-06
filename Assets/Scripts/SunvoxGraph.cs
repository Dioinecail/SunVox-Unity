namespace SunvoxNodeEditor
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using XNode;

	[CreateAssetMenu]
	public class SunvoxGraph : NodeGraph
	{
		const string streamingAssetsFolder = "Assets/StreamingAssets/";
		const string sunvoxExt = ".sunvox";
		public string projectName;
		public string Path { get { return streamingAssetsFolder + projectName + sunvoxExt; } }
	}
}