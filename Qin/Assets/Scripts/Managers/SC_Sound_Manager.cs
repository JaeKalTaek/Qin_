using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SC_Sound_Manager : MonoBehaviour {

    public static SC_Sound_Manager Instance;

    [Header("Music")]

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

    [Tooltip("Slider to change music's volume")]
    public Slider musicVolume;

    void Awake () {

        Instance = this;

    }

    public void StartCombatMusic () {               

        combatMusicTimelineInfo = new TimelineInfo();

        combatMusicCallback = new EVENT_CALLBACK(CombatMusicBeatEventCallback);

        combatMusic = RuntimeManager.CreateInstance(combatMusicRef);

        combatMusicTimelineHandle = GCHandle.Alloc(combatMusicTimelineInfo, GCHandleType.Pinned);

        combatMusic.setUserData(GCHandle.ToIntPtr(combatMusicTimelineHandle));

        combatMusic.setCallback(combatMusicCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        combatMusic.start();

    }

    public void SetMusicVolume () {

        combatMusic.setParameterValue("", musicVolume.value / 100);

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

    private void Update () {

        if (Input.GetKeyDown(KeyCode.G))
            AugmentPart();

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

                // print(markerName);

                if(markerName.Contains("TS"))
                    instance.setParameterValue("Transition", 0);

            }

        }

        return RESULT.OK;

    }

    void OnDestroy () {

        combatMusic.setUserData(IntPtr.Zero);
        combatMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        combatMusic.release();
        combatMusicTimelineHandle.Free();

    }

}
