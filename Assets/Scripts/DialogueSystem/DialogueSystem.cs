using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
public class DialogueSystem : MonoBehaviour
{
    protected class DialogueElem
    {
        public virtual bool shouldCoroutine => false;
        public virtual void Run(DialogueSystem DS)
        {

        }

        public virtual IEnumerator RunCR(DialogueSystem DS)
        {
            yield break;
        }
    }
    protected class ChangeScene : DialogueElem
    {
        public BackgroundDesc scene;

        public override void Run(DialogueSystem DS)
        {
            DS.backgroundImage.sprite = scene.image;
        }
    }
    protected class Say : DialogueElem
    {
        public ActorDesc    actor;
        public string       text;

        public override bool shouldCoroutine => true;

        public override IEnumerator RunCR(DialogueSystem DS)
        {
            if (DS.portraitImage != null)
            {
                DS.portraitImage.gameObject.SetActive(actor.portraitImage != null);
                DS.portraitImage.sprite = actor.portraitImage;
            }
            DS.mainText.color = actor.colorText;
            DS.mainText.text = text;
            if (DS.actorName)
            {
                DS.actorName.text = actor.displayName;
                DS.actorName.color = actor.displayColor;
            }
            if (text != "")
            {
                yield return DS.StartCoroutine(DS.FadeToCR(DS.textDisplay, 1, DS.fadeInTime));

                while (true)
                {
                    if (Input.anyKeyDown)
                    {
                        break;
                    }

                    yield return null;
                }
            }
        }
    }

    [SerializeField] private bool               runAtStart;
    [SerializeField] private Image              backgroundImage;
    [SerializeField] private CanvasGroup        fader;
    [SerializeField] private CanvasGroup        textDisplay;
    [SerializeField] private Image              portraitImage;
    [SerializeField] private TextMeshProUGUI    actorName;
    [SerializeField] private TextMeshProUGUI    mainText;
    [SerializeField] private float              defaultFadeInTime = 0.5f;
    [SerializeField] private TextAsset          storyText;
    [SerializeField] private BackgroundDesc[]   backgrounds;
    [SerializeField] private ActorDesc[]        actors;
    [SerializeField, Scene] 
    private string nextScene;

    List<DialogueElem>      elems;
    Sprite                  startBackgroundSprite;
    float                   fadeInTime;
    float                   fadeoutTime;
    bool                    parsed;
    Coroutine               playCR;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadeInTime = defaultFadeInTime;
        fadeoutTime = defaultFadeInTime;

        textDisplay.alpha = 0;

        if (runAtStart)
        {
            fader.alpha = 1;
            Play();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Parse()
    {
        if (storyText == null)
        {
            Debug.LogWarning("No text asset to parse!");
            return;
        }

        elems = new();
        startBackgroundSprite = null;

        string[] lines = storyText.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var str = lines[i];
            str = str.Trim();
            str = str.Replace('\t', ' ');
            var tokens = str.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0) continue;
            if (tokens[0] == "scene")
            {
                var newElem = new ChangeScene() { scene = GetBackground(tokens[1]) };
                elems.Add(newElem);
                if (startBackgroundSprite == null)
                {
                    startBackgroundSprite = newElem.scene?.image;
                }
            }
            else
            {
                var actor = GetActor(tokens[0]);
                if (actor != null)
                {
                    // Get text 
                    string  text;
                    var     startIndex = str.IndexOf('\"');
                    if (startIndex != -1)
                    {
                        var endIndex = str.IndexOf('\"', startIndex + 1);

                        text = str.Substring(startIndex + 1, endIndex - startIndex - 1);
                    }
                    else
                    {
                        text = str.Substring(tokens[0].Length).Trim();
                    }

                    Say newElem = new Say() { actor = actor, text = text };
                    elems.Add(newElem);
                }
                else
                {
                    Debug.LogError($"Can't parse [{str}] [{lines[i]}]!");
                }
            }
        }

        parsed = true;
    }

    BackgroundDesc GetBackground(string name)
    {
        foreach (var bg in backgrounds)
        {
            if (bg.name == name) return bg;
        }

        return null;
    }
    ActorDesc GetActor(string name)
    {
        foreach (var actor in actors)
        {
            if (actor.name == name) return actor;
        }

        return null;
    }

    private void Play()
    {
        if (!parsed)
        {
            Parse();
        }
        if (parsed)
        {
            if (playCR != null)
            {
                StopCoroutine(playCR);
            }
            gameObject.SetActive(true);
            playCR = StartCoroutine(PlayCR());
        }
    }

    IEnumerator PlayCR()
    {
        // Setup initial state
        backgroundImage.sprite = startBackgroundSprite;

        yield return StartCoroutine(FadeToCR(fader, 0, fadeInTime));

        foreach (var elem in elems)
        {
            if (elem.shouldCoroutine)
            {
                yield return StartCoroutine(elem.RunCR(this));
            }
            else
            {
                elem.Run(this);
            }
        }

        if (textDisplay.alpha > 0)
        {
            yield return StartCoroutine(FadeToCR(textDisplay, 0, fadeoutTime));
            yield return StartCoroutine(FadeToCR(fader, 1, fadeoutTime));
        }

        GameManager.Instance.GotoScene(nextScene);

        playCR = null;
    }

    IEnumerator FadeToCR(CanvasGroup canvasGroup, float targetAlpha, float fadeTime)
    {
        if (canvasGroup != null)
        {
            if (fadeTime > 0)
            {
                if (targetAlpha < canvasGroup.alpha)
                {
                    while (canvasGroup.alpha > targetAlpha)
                    {
                        canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha - Time.deltaTime / fadeTime);

                        yield return null;
                    }
                }
                else
                {
                    while (canvasGroup.alpha < targetAlpha)
                    {
                        canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha + Time.deltaTime / fadeTime);

                        yield return null;
                    }
                }
            }
            else canvasGroup.alpha = targetAlpha;
        }        
    }

}
