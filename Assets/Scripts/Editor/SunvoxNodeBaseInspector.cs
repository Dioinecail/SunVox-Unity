namespace SunvoxNodeEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(SunvoxNodeBase))]
    public class SunvoxNodeBaseInspector : Editor
    {
        SunvoxNodeBase node;



        private void OnEnable()
        {
            node = target as SunvoxNodeBase;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (node.TargetModule != null && node.TargetModule.controllers != null)
            {
                ModuleGUI(node.TargetModule);
            }
        }

        private void ModuleGUI(SunvoxModule targetModule)
        {
            for (int i = 0; i < targetModule.controllers.Length; i++)
            {
                string ctrlName = targetModule.controllers[i].name;
                int ctrlValue = targetModule.controllers[i].Value;

                targetModule.controllers[i].Value = EditorGUILayout.IntSlider(ctrlName, ctrlValue, 0, 32768);
            }
        }
    }
}