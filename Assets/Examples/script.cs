using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class script : MonoBehaviour {
    public KMBombInfo bomb;
    public KMAudio bombAudio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool moduleSolved = false;
    private bool incorrect = false;

    public KMSelectable[] buttons;

    // Use this for initialization
    void Start () {

    }

	// Update is called once per frame
	void Awake () {
        ModuleId = ModuleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { buttonPressed(pressedButton); return false; };
        }
    }

    void buttonPressed(KMSelectable pressedButton)
    {
        pressedButton.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (moduleSolved)
        {
            return;
        }


    }

    /*private bool isCommandValid(string cmd)
    {
        string[] validbtns = { "1","2","3","4" };

        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        foreach (var btn in parts)
        {
            if (!validbtns.Contains(btn.ToLower()))
            {
                return false;
            }
        }
        return true;
    }

    public string TwitchHelpMessage = "Use !{0} press 1 13 to press the first button 13 times.";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        if (isCommandValid(cmd))
        {
            yield return null;
            for (int i = 0; i < parts.Count(); i++)
            {
                if (parts[i] == "1")
                {
                    yield return new KMSelectable[] { buttons[0] };
                }
                else if (parts[i] == "2")
                {
                    yield return new KMSelectable[] { buttons[1] };
                }
                else if (parts[i] == "3")
                {
                    yield return new KMSelectable[] { buttons[2] };
                }
                else if (parts[i] == "4")
                {
                    yield return new KMSelectable[] { buttons[3] };
                }
            }
        }
        else
        {
            yield break;
        }
    }*/

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Template #{0}] {1}", ModuleId, msg);
    }
}
