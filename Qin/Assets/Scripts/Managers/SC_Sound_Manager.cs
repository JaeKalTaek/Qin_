﻿using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SC_Sound_Manager : MonoBehaviour {

    public static SC_Sound_Manager Instance;

    #region Music
    [Header("Music")]

    #region Main menu music
    [Header("Main menu music")]
    [EventRef]
    public string mainMenuMusicRef;

    EventInstance mainMenuMusic;
    #endregion

    #region Combat Music variables
    [Header("Combat Music")]
    [EventRef]
    public string combatMusicRef;

    EventInstance combatMusic;

    public float PartValue { get; set; }

    [StructLayout(LayoutKind.Sequential)]
    class TimelineInfo {

        public int currentMusicBar = 0;
        public StringWrapper lastMarker = new StringWrapper();

    }

    TimelineInfo combatMusicTimelineInfo;
    GCHandle combatMusicTimelineHandle;

    EVENT_CALLBACK combatMusicCallback;
    #endregion
    #endregion

    #region Combat sounds
    EventDescription[] hitEvents;
    #endregion

    #region Characters
    [Header("Characters")]
    [EventRef]
    public string footstepsRef;

    EventInstance footsteps;
    #endregion

    #region Constructions
    [Header("Constructions")]
    [EventRef]
    public string constructRef;

    EventInstance construct;

    [EventRef]
    public string constructionDestroyedRef;
    #endregion

    #region UI
    [Header("UI")]
    [EventRef]
    public string onButtonClickRef;
    #endregion

    #region Cursor
    [Header("Cursor")]
    [EventRef]
    public string cursorMovedRef;
    #endregion

    void Awake () {

        Instance = this;

        #region Setup events
        mainMenuMusic = RuntimeManager.CreateInstance(mainMenuMusicRef);

        mainMenuMusic.start();

        Bank b;

        RuntimeManager.StudioSystem.getBank("HIT", out b);

        b.getEventList(out hitEvents);

        footsteps = RuntimeManager.CreateInstance(footstepsRef);

        construct = RuntimeManager.CreateInstance(constructRef);
        #endregion

        DontDestroyOnLoad(this);

    }

    #region Combat Music
    public void StartCombatMusic (Slider volume) {

        mainMenuMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        mainMenuMusic.release();

        volume.onValueChanged.AddListener((float f) => { combatMusic.setVolume(f / 100); });

        combatMusicTimelineInfo = new TimelineInfo();

        combatMusicCallback = new EVENT_CALLBACK(CombatMusicBeatEventCallback);

        combatMusic = RuntimeManager.CreateInstance(combatMusicRef);

        combatMusicTimelineHandle = GCHandle.Alloc(combatMusicTimelineInfo, GCHandleType.Pinned);

        combatMusic.setUserData(GCHandle.ToIntPtr(combatMusicTimelineHandle));

        combatMusic.setCallback(combatMusicCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        combatMusic.start();

    }

    public void AugmentPart () {

        PartValue += .5f;

        SetValue("Partie", Mathf.Floor(PartValue));

    }

    public void SetTempo () {

        int furthest = 0;

        foreach(SC_Hero hero in SC_Hero.heroes) {

            int distance = hero.transform.position.x.I() + hero.transform.position.y.I();

            furthest = distance > furthest ? distance : furthest;

        }

        SetValue("Tempo", Mathf.Floor((furthest / (float)(SC_Tile_Manager.Instance.xSize + SC_Tile_Manager.Instance.ySize)) * 6));

    }

    public void SetValue (string id, float newValue) {

        float currentValue;
        combatMusic.getParameterValue(id, out currentValue, out currentValue);

        if (currentValue != newValue) {

            combatMusic.setParameterValue(id, newValue);

            #region Try triggering last stand part
            float otherValue;
            combatMusic.getParameterValue(id == "Tempo" ? "Partie" : "Tempo", out otherValue, out otherValue);            

            if((newValue == 5) && (otherValue == 5))
                combatMusic.setParameterValue("Partie", 6);
            #endregion

            if (id == "Tempo")
                combatMusic.setParameterValue("Transition", 1);

        }

    }    

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static RESULT CombatMusicBeatEventCallback (EVENT_CALLBACK_TYPE type, EventInstance instance, IntPtr parameterPtr) {
        
        IntPtr timelineInfoPtr;

        RESULT result = instance.getUserData(out timelineInfoPtr);

        if ((result == RESULT.OK) && (timelineInfoPtr != IntPtr.Zero)) {

            if (type == EVENT_CALLBACK_TYPE.TIMELINE_MARKER) {

                string markerName = ((TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES))).name;

                if(markerName.Contains("TS"))
                    instance.setParameterValue("Transition", 0);

            }

        }

        return RESULT.OK;

    }
    #endregion

    private void Update () {

        /*if(combatMusic.isValid()) {

            float p;

            combatMusic.getParameterValue("Transition", out p, out p);

            print(p);

        }*/

        if (EventSystem.current.currentSelectedGameObject?.GetComponent<Selectable>() && (Input.GetButtonDown("Submit") || Input.GetMouseButtonDown(0)))
            OnButtonClick();

    }

    #region Fight
    public void Hit (SC_Character attacker, SC_Character attacked, SC_Construction constru) {

        string a = attacker.Hero ? (attacker.Hero.male ? "" : "FE") + "MALE" : "SOLDIER";

        string b = constru ? "BUILDING" : ((attacked.BaseQinChara || !attacked) ? "SOLDIER" : (attacked.Hero.male ? "" : "FE") + "MALE");

        bool c = attacker.CriticalAmount >= SC_Game_Manager.Instance.CommonCharactersVariables.critTrigger;

        foreach (EventDescription e in hitEvents) {

            string p;

            e.getPath(out p);

            if (p.Contains("/" + a + "_HIT_" + b + "_SLOW") && (p.Contains("CRIT") == c)) {

                EventInstance hitSound = RuntimeManager.CreateInstance(p);
                hitSound.setVolume(.67f);
                hitSound.start();
                hitSound.release();
                

            }

        }

    }
    #endregion

    #region Characters
    public void SetFootsteps (bool on) {

        if (on)
            footsteps.start();
        else
            footsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

    }
    #endregion

    #region Constructions
    public void OnConstruct () {

        construct.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        construct.start();

    }

    public void OnCancelConstruct () {

        construct.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

    }

    public void OnConstructionDestroyed () {

        RuntimeManager.PlayOneShot(constructionDestroyedRef);

    }
    #endregion

    #region UI
    public void OnButtonClick () {

        RuntimeManager.PlayOneShot(onButtonClickRef);

    }
    #endregion

    #region Cursor
    public void OnCursorMoved () {

        RuntimeManager.PlayOneShot(cursorMovedRef);        

    }
    #endregion

    void OnDestroy () {

        combatMusic.setUserData(IntPtr.Zero);
        combatMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        combatMusic.release();
        if(combatMusicTimelineHandle.IsAllocated)
            combatMusicTimelineHandle.Free();

        construct.release();

    }

}
