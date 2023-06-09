using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class DialogueVariables
{
    public Dictionary<string, Ink.Runtime.Object> var { get; private set; }
    private Story globalVariablesStory;//privado para refernciarlo en el guardado y cargado de datos
    private const string saveVarKey = "Ink_Var";
    public DialogueVariables(TextAsset loadGlobalsJSON)
    {
        globalVariablesStory = new Story(loadGlobalsJSON.text); //Crea la historia

        if (PlayerPrefs.HasKey(saveVarKey))
        {
            string jsonState = PlayerPrefs.GetString(saveVarKey);
            globalVariablesStory.state.LoadJson(jsonState);
        }

        //inicializa el diccionario
        var = new Dictionary<string, Ink.Runtime.Object>();
        foreach (string name in globalVariablesStory.variablesState)
        {
            Ink.Runtime.Object value = globalVariablesStory.variablesState.GetVariableWithName(name);
            var.Add(name, value);
            Debug.Log("Initialized global dialogue variable:" + name + "=" + value);
        }
    }

    public void SaveVariables()
    {
        if (globalVariablesStory != null)
        {
            //Carga los datos de las variables de la historia
            VariablesToStory(globalVariablesStory);
            PlayerPrefs.SetString(saveVarKey, globalVariablesStory.state.ToJson());//Cambiar esto por un save load de verdad, sin usar playerprefs
        }
    }

    public void StartListening(Story story)
    {
        VariablesToStory(story);//importante que se llame a este metodo antes del observador 
        story.variablesState.variableChangedEvent += VarChanged;
    }
    public void StopListening(Story story)
    {
        story.variablesState.variableChangedEvent -= VarChanged;
    }
    private void VarChanged(string name, Ink.Runtime.Object value)
    {
        if (var.ContainsKey(name))
        {
            var.Remove(name);
            var.Add(name, value);
        }
    }

    void VariablesToStory(Story story)
    {
        foreach (KeyValuePair<string, Ink.Runtime.Object> var in var)
        {
            story.variablesState.SetGlobal(var.Key, var.Value);
        }
    }
}