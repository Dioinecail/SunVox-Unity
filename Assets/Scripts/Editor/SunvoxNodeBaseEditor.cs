namespace SunvoxNodeEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using XNodeEditor;
    using UnityEditor;
    using System.Linq;
    using System;

    [CustomNodeEditor(typeof(SunvoxNodeBase))]
    public class SunvoxNodeBaseEditor : NodeEditor
    {
        private SunvoxNodeBase tgtNode;



        private void OnEnable()
        {
            tgtNode = target as SunvoxNodeBase;
        }

        public override Color GetTint()
        {
            tgtNode = target as SunvoxNodeBase;

            if (tgtNode.TargetModule != null)
                return tgtNode.TargetModule.tint;
            else
                return base.GetTint();
        }

        public override void OnBodyGUI()
        {
            tgtNode = target as SunvoxNodeBase;

            // Unity specifically requires this to save/update any serial object.
            // serializedObject.Update(); must go at the start of an inspector gui, and
            // serializedObject.ApplyModifiedProperties(); goes at the end.
            serializedObject.Update();
            string[] excludes = { "m_Script", "graph", "position", "ports" };

            // Iterate through serialized properties and draw them like the Inspector (But with ports)
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (excludes.Contains(iterator.name))
                    continue;

                NodeEditorGUILayout.PropertyField(iterator, true);
            }

            // Iterate through dynamic ports and draw them in the order in which they are serialized
            foreach (XNode.NodePort dynamicPort in target.DynamicPorts)
            {
                if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort))
                    continue;
                NodeEditorGUILayout.PortField(dynamicPort);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}