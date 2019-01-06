using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

[ExecuteInEditMode]
[RequireComponent(typeof(VisualEffect))]
public class VisualEffectInitialState : MonoBehaviour
{
    public enum DefaultState
    {
        Play,
        Stop,
        CustomEvent
    }

    public DefaultState defaultState = DefaultState.Play;
    public string CustomEventName = "CustomEvent";

    private void Awake()
    {
        ProcessInitialState();
    }

    void ProcessInitialState()
    {
        var component = GetComponent<VisualEffect>();

        component.Reinit();

        switch (defaultState)
        {
            case DefaultState.Play:
                component.Play();
                Debug.Log("Play");
                break;
            case DefaultState.Stop:
                component.Stop();
                Debug.Log("Stop");
                break;
            case DefaultState.CustomEvent:
                component.SendEvent(CustomEventName);
                Debug.Log(CustomEventName);
                break;
        }
    }


    private void OnEnable()
    {
        ProcessInitialState();
    }


}
