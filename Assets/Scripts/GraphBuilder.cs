using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SunvoxNodeEditor;
using System;
using System.Runtime.InteropServices;
using XNode;

public class SunvoxCtrl
{
    public string name;
    public int Value
    {
        get => _value;
        set
        {
            _value = Mathf.Max(0, value);

            SunVox.sv_set_event_t(0, 1, 0);
            SunVox.sv_send_event(0, 0, 0, 0, moduleIndex + 1, (index + 1) << 8, _value);
        }
    }

    private int index = -1;
    private int _value = -1;
    private int moduleIndex;



    public SunvoxCtrl(int moduleIndex, int index, string name, int value)
    {
        this.moduleIndex = moduleIndex;
        this.index = index;
        this._value = value;
        this.name = name;
    }
}

[Serializable]
public class SunvoxModule
{
    public int index;
    public string name;
    public SunvoxCtrl[] controllers;
    public List<int> inputs;
    public List<int> outputs;
    public int posX, posY;
    public Vector2 position { get => new Vector2(posX * 2, posY * 2); }
    public Color tint;
    public SunvoxNodeBase targetNode;
}

public class GraphBuilder : MonoBehaviour
{
    public event Action onProjectLoaded;

    public SunvoxGraph target;
    public bool loadOnStart;

    private SunvoxModule[] projectModules;
    private bool isInit;
    private int lastConnectedFrom, lastConnectedTo;



    public void Play()
    {
        SunVox.sv_play_from_beginning(0);
    }

    public void Stop()
    {
        SunVox.sv_stop(0);
    }

    public void BuildGraph()
    {
        target.Clear();
        GetModules();
        CreateGraphNodes();
    }

    public void LoadSunvoxProject()
    {
        try
        {
            int ver = SunVox.sv_init("0", 44100, 2, 0);
            isInit = true;
            if (ver >= 0)
            {
                int major = (ver >> 16) & 255;
                int minor1 = (ver >> 8) & 255;
                int minor2 = (ver) & 255;

                SunVox.sv_open_slot(0);

                if (SunVox.sv_load(0, target.Path) == 0)
                {
                    Debug.Log("[GraphBuilder.LoadSunvoxProject] Loaded");
                }
                else
                {
                    Debug.Log("[GraphBuilder.LoadSunvoxProject] Error loading sunvox project");
                    SunVox.sv_volume(0, 256);
                }

                onProjectLoaded?.Invoke();
            }
            else
            {
                    Debug.Log("[GraphBuilder.LoadSunvoxProject] sv_init() error");
            }
        }
        catch (Exception e)
        {
            Debug.Log("[GraphBuilder.LoadSunvoxProject] Exception: " + e);
        }
    }

    public void DEBUG_SendEventVolume(int value)
    {
        SunVox.sv_set_event_t(0, 1, 0);
        SunVox.sv_send_event(0, 0, 0, 0, 02 + 1, 01 << 8, value);
    }

    public void UnloadSunvoxProject()
    {
        if (!enabled)
            return;

        if(isInit)
        {
            SunVox.sv_close_slot(0);
            SunVox.sv_deinit();
        }
    }

    private void GetModules()
    {
        unsafe
        {
            int numModules = SunVox.sv_get_number_of_modules(0);
            projectModules = new SunvoxModule[numModules];

            for (int i = 0; i < numModules; i++)
            {
                projectModules[i] = CreateSunvoxModule(i);
            }
        }
    }

    private unsafe SunvoxModule CreateSunvoxModule(int numModule)
    {
        SunvoxModule module = new SunvoxModule();

        IntPtr namePtr = SunVox.sv_get_module_name(0, numModule);
        string moduleName = Marshal.PtrToStringAnsi(namePtr);

        if (string.IsNullOrEmpty(moduleName))
            return null;

        //Get packed XY:
        int xy = SunVox.sv_get_module_xy(0, numModule);
        //Unpack X and Y:
        int x = xy & 0xFFFF; if ((x & 0x8000) == 1) x -= 0x10000;
        int y = (xy >> 16) & 0xFFFF; if ((y & 0x8000) == 1) y -= 0x10000;

        SunVox.sv_lock_slot(0);
        int moduleFlags = SunVox.sv_get_module_flags(0, numModule);
        SunVox.sv_unlock_slot(0);

        int inputsCount = (moduleFlags & SunVox.SV_MODULE_INPUTS_MASK) >> SunVox.SV_MODULE_INPUTS_OFF;
        int outputsCount = (moduleFlags & SunVox.SV_MODULE_OUTPUTS_MASK) >> SunVox.SV_MODULE_OUTPUTS_OFF;

        int* numInputsPtr = SunVox.sv_get_module_inputs(0, numModule);
        int* numOutputsPtr = SunVox.sv_get_module_outputs(0, numModule);

        module.index = numModule;
        module.name = moduleName;
        module.posX = x;
        module.posY = y;
        int color = SunVox.sv_get_module_color(0, numModule); 
        int r = color & 0xFF;           //r = 0...255
        int g = (color >> 8) & 0xFF;  //g = 0...255
        int b = (color >> 16) & 0xFF; //b = 0...255

        module.tint = new Color32((byte)r, (byte)g, (byte)b, (byte)255);

        int numCtrls = SunVox.sv_get_number_of_module_ctls(0, numModule);

        module.controllers = new SunvoxCtrl[numCtrls];

        for (int i = 0; i < numCtrls; i++)
        {
            IntPtr ctrlNamePtr = SunVox.sv_get_module_ctl_name(0, numModule, i);

            int index = i;
            string ctrlName = Marshal.PtrToStringAnsi(ctrlNamePtr);
            int value = SunVox.sv_get_module_ctl_value(0, numModule, index, 1);

            SunvoxCtrl ctrl  = new SunvoxCtrl(numModule, index, ctrlName, (value & 0xCCEE));
            module.controllers[i] = ctrl;
        }

        if (inputsCount > 0)
        {
            module.inputs = new List<int>();

            for (int i = 0; i < inputsCount; i++)
            {
                int index = numInputsPtr[i];

                if (index > -1)
                    module.inputs.Add(index);
            }
        }

        if(outputsCount > 0)
        {
            module.outputs = new List<int>();

            for (int i = 0; i < outputsCount; i++)
            {
                int index = numOutputsPtr[i];

                if (index > -1)
                    module.outputs.Add(index);
            }
        }

        return module;
    }

    private void CreateGraphNodes()
    {
        for (int i = 0; i < projectModules.Length; i++)
        {
            if (projectModules[i] == null)
                continue;

            int index = i;
            SunvoxNodeBase newNode = target.AddNode<SunvoxNodeBase>();
            newNode.SetModule(projectModules[index]);
            newNode.name = projectModules[i].name;
            newNode.position = projectModules[i].position;
            projectModules[i].targetNode = newNode;
        }

        for (int i = 0; i < projectModules.Length; i++)
        {
            if (projectModules[i] == null)
                continue;

            SunvoxNodeBase node = projectModules[i].targetNode;

            if (projectModules[i].inputs != null)
            {
                for (int x = 0; x < projectModules[i].inputs.Count; x++)
                {
                    int connectionIndex = projectModules[i].inputs[x];

                    SunvoxNodeBase connectedNode = projectModules[connectionIndex].targetNode;
                    NodePort port = connectedNode.GetOutputPort("output");

                    port.Connect(node.GetInputPort("input"));
                }
            }
        }

        for (int i = 0; i < projectModules.Length; i++)
        {
            if (projectModules[i] == null)
                continue;

            projectModules[i].targetNode.onConnected += OnNodeConnected;
            projectModules[i].targetNode.onDisconnected += OnNodeDisconnected;
        }
    }

    private void OnNodeConnected(int from, int to)
    {
        if (lastConnectedFrom == from && lastConnectedTo == to)
            return;

        SunVox.sv_lock_slot(0);

        int result = SunVox.sv_connect_module(0, from, to);

        SunVox.sv_unlock_slot(0);

        Debug.Log($"Connected: {from} to {to}");
        Debug.Log($"Result: {result}");

        lastConnectedFrom = from;
        lastConnectedTo = to;
    }

    private void OnNodeDisconnected(int from, int to)
    {
        SunVox.sv_lock_slot(0);

        int result = SunVox.sv_disconnect_module(0, from, to);

        SunVox.sv_unlock_slot(0);

        Debug.Log($"Disconnected: {from} from {to}");
        Debug.Log($"Result: {result}");

        lastConnectedFrom = -1;
        lastConnectedTo = -1;
    }

    private void OnEnable()
    {
        if(loadOnStart)
            LoadSunvoxProject();
    }

    private void OnDestroy()
    {
        UnloadSunvoxProject();
    }

    private int DecToOct (int input)
    {
        string oct = Convert.ToString(input, 8);

        if (int.TryParse(oct, out int result))
            return result;
        else return -1;
    }
}
