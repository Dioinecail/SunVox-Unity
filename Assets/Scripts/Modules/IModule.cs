namespace SunvoxNodeEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IModule
    {
        int Index { get; }
        string Name { get; }
        SunvoxCtrl[] Controllers { get; set; }
        List<int> Inputs { get; set; }
        List<int> Outputs { get; set; }
        Color Tint { get; }
        SunvoxNodeBase TargetNode { get; set; }



        Vector2 GetPosition();
    }
}