namespace SunvoxNodeEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ModuleBase : IModule
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public SunvoxCtrl[] Controllers { get; set; }
        public List<int> Inputs { get; set; }
        public List<int> Outputs { get; set; }
        public Color Tint { get; set; }
        public SunvoxNodeBase TargetNode { get; set; }

        private int X, Y;



        public ModuleBase(int index, string name, int x, int y, SunvoxCtrl[] controllers, List<int> inputs, List <int> outputs, Color tint)
        {
            Index = index;
            Name = name;
            X = x;
            Y = y;
            Controllers = controllers;
            Inputs = inputs;
            Outputs = outputs;
            Tint = tint;
        }

        public Vector2 GetPosition()
        {
            return new Vector2(X * 2, Y * 2);
        }
    }
}