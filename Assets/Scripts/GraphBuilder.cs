using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SunvoxNodeEditor;
using System;
using System.Runtime.InteropServices;
using XNode;

[System.Serializable]
public class SunvoxModule
{
    public int index;
    public string name;
    public int numControllers;
    public int[] numInputs;
    public int[] numOutputs;
    public int posX, posY;
    public Vector2 position { get => new Vector2(posX, posY); }
}

public class GraphBuilder : MonoBehaviour
{
    public event Action onProjectLoaded;

    public SunvoxGraph target;

    private SunvoxModule[] projectModules;



    public void BuildGraph()
    {
        target.Clear();
        CreateGraphNodes();
    }

    private void LoadSunvoxProject()
    {
        try
        {
            int ver = SunVox.sv_init("0", 44100, 2, 0);
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

                GetModules();
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

    private void GetModules()
    {
        int numModules = SunVox.sv_get_number_of_modules(0);

        projectModules = new SunvoxModule[numModules];

        for (int i = 0; i < numModules; i++)
        {
            int index = i;
            SunvoxModule module = new SunvoxModule();
            IntPtr namePtr = SunVox.sv_get_module_name(0, i);
            int numControllers = SunVox.sv_get_number_of_module_ctls(0, i);
            int pos = SunVox.sv_get_module_xy(0, i);
            int[] numInputs = SunVox.sv_get_module_inputs(0, i);
            int[] numOutputs = SunVox.sv_get_module_outputs(0, i);

            int x = pos & 0xFFFF; 

            if ((x & 0x8000) == 1) 
                x -= 0x10000;

            int y = (pos >> 16) & 0xFFFF; 

            if ((y & 0x8000) == 1) 
                y -= 0x10000;

            module.name = Marshal.PtrToStringAnsi(namePtr);
            module.index = index;
            module.numControllers = numControllers;
            module.posX = x;
            module.posY = y;
            module.numInputs = numInputs;
            module.numOutputs = numOutputs;

            projectModules[index] = module;
        }
    }

    private void CreateGraphNodes()
    {
        for (int i = 0; i < projectModules.Length; i++)
        {
            SunvoxNodeBase newNode = target.AddNode<SunvoxNodeBase>();
            newNode.name = projectModules[i].name;
            newNode.position = projectModules[i].position;

            if (projectModules[i].numOutputs == null || projectModules[i].numInputs == null)
                continue;

            int moduleFlags = SunVox.sv_get_module_flags(0, i);
            int Number_of_inputs = (moduleFlags & SunVox.SV_MODULE_INPUTS_MASK) >> SunVox.SV_MODULE_INPUTS_OFF;
            int Number_of_outputs = (moduleFlags & SunVox.SV_MODULE_OUTPUTS_MASK) >> SunVox.SV_MODULE_OUTPUTS_OFF;


            Debug.Log($"Number_of_inputs: {projectModules[i].name} [{Number_of_inputs}]");
            Debug.Log($"Number_of_outputs: {projectModules[i].name} [{Number_of_outputs}]");

            for (int output = 0; output < projectModules[i].numOutputs.Length; output++)
            {
                //IntPtr ptr = IntPtr.Zero;
                //Marshal.StructureToPtr(projectModules[i].numOutputs[output], ptr, false);

                //int index = Marshal.ReadInt32(ptr);
                //Debug.Log($"index : {index}");
                Debug.Log($"outputs {projectModules[i].name} [{output}:{projectModules[i].numOutputs[output] & SunVox.SV_MODULE_OUTPUTS_MASK}");
            }

        }

        for (int i = 0; i < projectModules.Length; i++)
        {
            if (projectModules[i].numOutputs == null || projectModules[i].numInputs == null)
                continue;

            SunvoxNodeBase node = target.nodes[i] as SunvoxNodeBase;

            for (int x = 0; x < projectModules[i].numOutputs.Length; x++)
            {
                int connectionIndex = projectModules[i].numOutputs[x];

                SunvoxNodeBase connectedNode = target.nodes[connectionIndex] as SunvoxNodeBase;
                NodePort port = connectedNode.inputPort;

                node.outputPort.Connect(port);
            }
        }
    }

    private void OnEnable()
    {
        LoadSunvoxProject();
    }

    private void OnDisable()
    {
        if (!enabled) return;

        SunVox.sv_close_slot(0);
        SunVox.sv_deinit();
    }
}
