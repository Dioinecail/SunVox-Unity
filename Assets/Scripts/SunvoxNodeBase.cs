﻿namespace SunvoxNodeEditor
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using XNode;

	public class SunvoxNodeBase : Node
	{
		public event Action<int, int> onConnected;
		public event Action<int, int> onDisconnected;

		public SunvoxModule TargetModule { get; private set; }

		[Input]
		public SunvoxNodeBase inputPort;
		[Output]
		public SunvoxNodeBase outputPort;



		protected override void Init()
		{
			base.Init();
		}

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return null; // Replace this
		}

		public void SetModule(SunvoxModule module)
        {
			TargetModule = module;
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
			SunvoxNodeBase nodeFrom = from.node as SunvoxNodeBase;
			SunvoxNodeBase nodeTo = to.node as SunvoxNodeBase;

			int indexFrom = nodeFrom.TargetModule.index;
			int indexTo = nodeTo.TargetModule.index;

			onConnected?.Invoke(indexFrom, indexTo);
		}

		public override void OnRemoveConnection(NodePort from, NodePort to)
        {
			SunvoxNodeBase nodeFrom = from.node as SunvoxNodeBase;
			SunvoxNodeBase nodeTo = to.node as SunvoxNodeBase;

			int indexFrom = nodeFrom.TargetModule.index;
			int indexTo = nodeTo.TargetModule.index;

			onDisconnected?.Invoke(indexFrom, indexTo);
		}
    }
}