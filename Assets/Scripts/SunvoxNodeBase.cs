namespace SunvoxNodeEditor
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using XNode;

    [NodeWidth(150)]
	public class SunvoxNodeBase : Node
	{
		public event Action<int, int> onConnected;
		public event Action<int, int> onDisconnected;

		public ModuleBase TargetModule { get; private set; }

		[Input(backingValue = ShowBackingValue.Never)]
		public SunvoxNodeBase input;
		[Output(backingValue = ShowBackingValue.Never)]
		public SunvoxNodeBase output;



		protected override void Init()
		{
			base.Init();
		}

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return null; // Replace this
		}

		public void SetModule(ModuleBase module)
        {
			TargetModule = module;
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
			SunvoxNodeBase nodeFrom = from.node as SunvoxNodeBase;
			SunvoxNodeBase nodeTo = to.node as SunvoxNodeBase;

            if (nodeFrom.TargetModule == null || nodeTo.TargetModule == null)
                return;

			int indexFrom = nodeFrom.TargetModule.Index;
			int indexTo = nodeTo.TargetModule.Index;

			onConnected?.Invoke(indexFrom, indexTo);
		}

		public override void OnRemoveConnection(NodePort from, NodePort to)
        {
			SunvoxNodeBase nodeFrom = from.node as SunvoxNodeBase;
			SunvoxNodeBase nodeTo = to.node as SunvoxNodeBase;

            if (nodeFrom.TargetModule == null || nodeTo.TargetModule == null)
                return;

            int indexFrom = nodeFrom.TargetModule.Index;
			int indexTo = nodeTo.TargetModule.Index;

			onDisconnected?.Invoke(indexFrom, indexTo);
		}
    }
}