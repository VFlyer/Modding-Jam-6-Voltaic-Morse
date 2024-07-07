using KModkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class VoltaicMorse : MonoBehaviour {

	public KMBombModule modSelf;
	public KMBombInfo bombInfo;
	public KMAudio mAudio;
	public KMSelectable controlBtn, leftBtn, rightBtn;
	public MeshRenderer ledRenderer;
	public Light statusLight;
	public NeedleHandlerScript needleHandler;

	static List<string> possibleVoltages = new List<string> {
		"0.5", "1", "1.5", "2", "2.5",
		"3", "3.5", "4", "4.5", "5",
		"5.5", "6", "6.5", "7", "7.5",
		"8", "8.5", "9", "9.5", "10" },
		possibleWords = new List<string> {
		"absorb", "busts", "circuit", "direct", "electric",
		"force", "green", "hello", "ionic", "jolty",
		"kinetic", "measure", "nerve", "omega", "pulse",
		"rocky", "static", "ting", "wreck", "yotta"};
	static string[] morseRepresentations = new[] {
		".-", "-...", "-.-.", "-..", ".",
		"..-.", "--.", "....", "..", ".---",
		"-.-", ".-..", "--", "-.", "---",
		".--.", "--.-", ".-.", "...", "-",
		"..-", "...-", ".--", "-..-", "-.--", "--.." };
	const string alphabet = "abcdefghijklmnopqrstuvwxyz";

	int moduleID;
	static int modIDCnt;

	string wordPicked;
	int idxPicked, expectedIdx;
	bool activated, solved, interactable;
	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
    }
	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		modSelf.OnActivate += GenerateProcedure;
		leftBtn.OnInteract += delegate {
			leftBtn.AddInteractionPunch(0.5f);
			mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, leftBtn.transform);
			HandleOffsetPress(-1);
			return false;
		};
		rightBtn.OnInteract += delegate {
			rightBtn.AddInteractionPunch(0.5f);
			mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, rightBtn.transform);
			HandleOffsetPress(1);
			return false;
		};
		controlBtn.OnInteract += delegate {
			controlBtn.AddInteractionPunch(0.5f);
			mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, controlBtn.transform);
			HandleSubmit();
			return false;
		};
		statusLight.range *= transform.lossyScale.x;
		ledRenderer.material.color = Color.black;
		statusLight.enabled = false;
	}
	void HandleOffsetPress(int delta)
    {
		if (!interactable || !activated || solved) return;
		idxPicked = Mathf.Clamp(idxPicked + delta, 0, possibleVoltages.Count);
		if (idxPicked > 0)
		{
			needleHandler.nextProg = (float)idxPicked / possibleVoltages.Count;
			needleHandler.speed = 0.5f;
		}
		else
		{
			needleHandler.nextProg = 0.5f;
			needleHandler.speed = 0.25f;
		}
	}

	void HandleSubmit()
    {
		if (!interactable || !activated || solved) return;
		if (idxPicked == 0)
        {
			interactable = false;
			StartCoroutine(RenderPickedWord());
			return;
        }
		statusLight.enabled = true;
		if (idxPicked == expectedIdx)
        {
			QuickLog("Correct voltage submitted.");
			modSelf.HandlePass();
			ledRenderer.material.color = Color.green;
			
			statusLight.color = Color.green;
			solved = true;
			needleHandler.nextProg = 0f;
			needleHandler.speed = 0.125f;
		}
		else
		{
			QuickLog("Submitted an incorrect voltage of {0}", possibleVoltages[idxPicked - 1]);
			modSelf.HandleStrike();
			ledRenderer.material.color = Color.red;
			statusLight.color = Color.red;
			idxPicked = 0;
			needleHandler.nextProg = 0.5f;
			needleHandler.speed = 0.25f;
		}
	}

	IEnumerator RenderPickedWord()
    {
		needleHandler.nextProg = 0f;
		for (var curLetterIdx = 0; curLetterIdx < wordPicked.Length; curLetterIdx++)
        {
			var curLetter = wordPicked[curLetterIdx];
			var curMorse = morseRepresentations[alphabet.IndexOf(curLetter)];
			var timeRequired = curMorse.Sum(a => a == '-' ? 0.75f : 0.25f) - 0.25f * (curMorse.Length - 1);
			//Debug.Log(timeRequired);
			while (needleHandler.progress + (timeRequired * needleHandler.speed) > 1f - needleHandler.progress && needleHandler.progress > 0f)
				yield return null;
			if (needleHandler.progress <= 0f)
            {
				StartCoroutine(Failsafe());
				yield break;
            }
			for (var n = 0; n < curMorse.Length; n++)
            {
				needleHandler.nextProg = 1f;
				yield return new WaitForSeconds(curMorse[n] == '-' ? 0.75f : 0.25f);
				if (needleHandler.progress >= 1f)
                {
					StartCoroutine(Failsafe());
					yield break;
				}
				needleHandler.nextProg = 0f;
				if (n + 1 < curMorse.Length)
					yield return new WaitForSeconds(0.25f);
            }
		}
		needleHandler.nextProg = 0.5f;
		while (needleHandler.progress < 0.5f)
			yield return null;
		interactable = true;
		yield break;
    }
	IEnumerator Failsafe()
    {
		needleHandler.nextProg = 0f;
		yield return new WaitForSeconds(5f);
		needleHandler.nextProg = 0.5f;
		while (needleHandler.progress < 0.5f)
			yield return null;
		interactable = true;
		yield break;
    }

	void GenerateProcedure()
    {
		var serialNoDigits = bombInfo.GetSerialNumberNumbers();
		var voltages = bombInfo.QueryWidgets("voltage", "exish");
		var initialVoltage = possibleVoltages[serialNoDigits.Last() * 2 + serialNoDigits.First() % 2];
		if (voltages.Count >= 1)
			initialVoltage = voltages.Max(a => possibleVoltages.IndexOf(a)).ToString();
		QuickLog("{0} Voltage registered as {1}", voltages.Any() ? "Voltage meter present." : "Using the serial number to generate fake voltage.", initialVoltage);
		wordPicked = possibleWords.PickRandom();
		QuickLog("Selected word: {0}", wordPicked);
		expectedIdx = 1 + (possibleWords.IndexOf(wordPicked) + possibleVoltages.IndexOf(initialVoltage)) % possibleVoltages.Count;
		QuickLog("Expected Voltage to submit: {0}", possibleVoltages[expectedIdx - 1]);
		activated = true;
		interactable = true;
		needleHandler.nextProg = 0.5f;
	}
	string TwitchHelpMessage = "\"!{0} submit 5.5\" OR \"!{0} sub 5.5\" OR \"!{0} enter 5.5\" [Submits a voltage of 5.5] | \"!{0} play\" OR \"!{0} receive\" [Plays a transmission.]";
	IEnumerator ProcessTwitchCommand(string cmd)
    {
		var intCmd = cmd.Trim();
		var rgxSubmit = Regex.Match(intCmd, @"^(sub|enter|submit)\s", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		var rgxPlay = Regex.Match(intCmd, @"^(play|receive)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (rgxPlay.Success)
        {
			if (!interactable || !activated)
			{
				yield return "sendtochaterror I cannot interact with the button right now. Wait a bit.";
				yield break;
			}
			yield return null;
			controlBtn.OnInteract();
        }
		else if (rgxSubmit.Success)
        {
			var cmdLastPart = intCmd.Split().Last();
			if (!possibleVoltages.Contains(cmdLastPart))
            {
				yield return string.Format("sendtochaterror I cannot submit a voltage of \"{0}\" onto the module.", cmdLastPart);
				yield break;
            }
			if (!interactable || !activated)
			{
				yield return "sendtochaterror I cannot interact with the button right now. Wait a bit.";
				yield break;
			}
			var idxToSet = 1 + possibleVoltages.IndexOf(cmdLastPart);
			yield return null;
			while (idxToSet != idxPicked)
            {
				(idxPicked > idxToSet ? leftBtn : rightBtn).OnInteract();
				yield return new WaitForSeconds(0.1f);
            }
			controlBtn.OnInteract();
        }
    }
	IEnumerator TwitchHandleForcedSolve()
    {
		while (!interactable || !activated)
			yield return true;
		while (idxPicked != expectedIdx)
        {
			(idxPicked > expectedIdx ? leftBtn : rightBtn).OnInteract();
			yield return new WaitForSeconds(0.1f);
        }
		controlBtn.OnInteract();
    }

}
