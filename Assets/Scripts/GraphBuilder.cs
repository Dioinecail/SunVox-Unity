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
    public bool loadOnStart;

    private SunvoxModule[] projectModules;
    private bool isInit;



    public void BuildGraph()
    {
        target.Clear();
        //CreateGraphNodes();
    }

    public void DEBUG_LoadSunvoxProjectModules()
    {
        LoadSunvoxProject();
        UnloadSunvoxProject();
    }

    private void LoadSunvoxProject()
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

    private void UnloadSunvoxProject()
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

            int moduleNumCompressor = SunVox.sv_find_module(0, "Compressor");
            int moduleNumEcho = SunVox.sv_find_module(0, "Echo");
            int moduleNumOutput = SunVox.sv_find_module(0, "Output");

            DebugModuleInputsOutputs(moduleNumCompressor);
            DebugModuleInputsOutputs(moduleNumEcho);
            DebugModuleInputsOutputs(moduleNumOutput);

            /*
            for (int i = 0; i < numModules; i++)
            {
                SunvoxModule module = new SunvoxModule();
                IntPtr namePtr = SunVox.sv_get_module_name(0, i);
                //int numControllers = SunVox.sv_get_number_of_module_ctls(0, i);
                //int pos = SunVox.sv_get_module_xy(0, i);
                IntPtr numInputsPtr = SunVox.sv_get_module_inputs(0, i);
                IntPtr numOutputsPtr = SunVox.sv_get_module_outputs(0, i);

                int moduleFlags = SunVox.sv_get_module_flags(0, i);

                int inputsCount = (moduleFlags & SunVox.SV_MODULE_INPUTS_MASK) >> SunVox.SV_MODULE_INPUTS_OFF;
                int outputsCount = (moduleFlags & SunVox.SV_MODULE_OUTPUTS_MASK) >> SunVox.SV_MODULE_OUTPUTS_OFF;

                int[] numInputs = new int[inputsCount]; 
                int[] numOutputs = new int[outputsCount]; 

                if(numInputsPtr != null && numInputs.Length > 0)
                    Marshal.PtrToStructure(numInputsPtr, numInputs);
                if(numOutputsPtr != null && numOutputs.Length > 0)
                    Marshal.PtrToStructure(numOutputsPtr, numOutputs);

                //int x = pos & 0xFFFF; 

                //if ((x & 0x8000) == 1) 
                //    x -= 0x10000;

                //int y = (pos >> 16) & 0xFFFF; 

                //if ((y & 0x8000) == 1) 
                //    y -= 0x10000;

                module.name = Marshal.PtrToStringAnsi(namePtr);
                module.index = i;
                //module.numControllers = numControllers;
                //module.posX = x;
                //module.posY = y;
                module.numInputs = numInputs;
                module.numOutputs = numOutputs;

                projectModules[i] = module;
            }
            */
        }

    }

    private void DebugModuleInputsOutputs(int numModule)
    {
        IntPtr numInputsPtr = SunVox.sv_get_module_inputs(0, numModule);
        IntPtr numOutputsPtr = SunVox.sv_get_module_outputs(0, numModule);

        SunVox.sv_lock_slot(0);
        int moduleFlags = SunVox.sv_get_module_flags(0, numModule);
        SunVox.sv_unlock_slot(0);

        Debug.Log($"module-flags:{moduleFlags}");

        int inputsCount = (moduleFlags & SunVox.SV_MODULE_INPUTS_MASK) >> SunVox.SV_MODULE_INPUTS_OFF;
        int outputsCount = (moduleFlags & SunVox.SV_MODULE_OUTPUTS_MASK) >> SunVox.SV_MODULE_OUTPUTS_OFF;

        int[] numInputs = new int[inputsCount];
        int[] numOutputs = new int[outputsCount];

        if (numInputsPtr != null && numInputs.Length > 0)
            Marshal.PtrToStructure(numInputsPtr, numInputs);
        if (numOutputsPtr != null && numOutputs.Length > 0)
            Marshal.PtrToStructure(numOutputsPtr, numOutputs);

        if (numInputs != null)
            for (int x = 0; x < numInputs.Length; x++)
            {
                Debug.Log($"{numModule}-module:input:{x}={numInputs[x]}");
            }

        if (numOutputs != null)
            for (int x = 0; x < numOutputs.Length; x++)
            {
                Debug.Log($"{numModule}-module:output:{x}={numOutputs[x]}");
            }
    }

    private void CreateGraphNodes()
    {
        for (int i = 0; i < projectModules.Length; i++)
        {
            SunvoxNodeBase newNode = target.AddNode<SunvoxNodeBase>();

            if (projectModules[i].numOutputs == null || projectModules[i].numInputs == null)
                continue;

            int moduleFlags = SunVox.sv_get_module_flags(0, i);
            int Number_of_inputs = (moduleFlags & SunVox.SV_MODULE_INPUTS_MASK) >> SunVox.SV_MODULE_INPUTS_OFF;
            int Number_of_outputs = (moduleFlags & SunVox.SV_MODULE_OUTPUTS_MASK) >> SunVox.SV_MODULE_OUTPUTS_OFF;
            int numCtrls = SunVox.sv_get_number_of_module_ctls(0, i);

            string[] ctrlNames = new string[numCtrls];

            for (int x = 0; x < numCtrls; x++)
            {
                IntPtr ctrlNamePtr = SunVox.sv_get_module_ctl_name(0, i, x);
                ctrlNames[x] = Marshal.PtrToStringAnsi(ctrlNamePtr);
            }

            newNode.name = projectModules[i].name;
            newNode.position = projectModules[i].position;
            newNode.ctrlNames = ctrlNames;

            Debug.Log($"Number_of_inputs: {projectModules[i].name} [{Number_of_inputs}]");
            Debug.Log($"Number_of_outputs: {projectModules[i].name} [{Number_of_outputs}]");

            for (int output = 0; output < projectModules[i].numOutputs.Length; output++)
            {
                //IntPtr ptr = IntPtr.Zero;
                //Marshal.StructureToPtr(projectModules[i].numOutputs[output], ptr, false);

                //int index = Marshal.ReadInt32(ptr);
                //Debug.Log($"index : {index}");
                //Debug.Log($"outputs {projectModules[i].name} [{output}:{projectModules[i].numOutputs[output] & SunVox.SV_MODULE_OUTPUTS_MASK}");
            }

        }

        //for (int i = 0; i < projectModules.Length; i++)
        //{
        //    if (projectModules[i].numOutputs == null || projectModules[i].numInputs == null)
        //        continue;

        //    SunvoxNodeBase node = target.nodes[i] as SunvoxNodeBase;

        //    for (int x = 0; x < projectModules[i].numOutputs.Length; x++)
        //    {
        //        int connectionIndex = projectModules[i].numOutputs[x];

        //        SunvoxNodeBase connectedNode = target.nodes[connectionIndex] as SunvoxNodeBase;
        //        NodePort port = connectedNode.inputPort;

        //        node.outputPort.Connect(port);
        //    }
        //}
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
