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
    protected class ClearColor : DialogueElem
    {
        public Color color;

        public override void Run(DialogueSystem DS)
        {
            FindAnyObjectByType<Camera>().backgroundColor = color;
        }
    }

    protected class Wait : DialogueElem
    {
        public float waitTime;

        public override bool shouldCoroutine => true;

        public override IEnumerator RunCR(DialogueSystem DS)
        {
            yield return DS.StartCoroutine(DS.FadeToCR(DS.textDisplay, 0, DS.fadeInTime));

            float t = 0.0f;
            while (t < waitTime)
            {
                t += Time.deltaTime;
                if (Input.anyKeyDown)
                {
                    if (DS.skipSound != null) SoundManager.PlaySound(DS.skipSound, 0.5f, Random.Range(0.75f, 1.25f));
                    break;
                }
                
                yield return null;
            }
        }
    }

    protected class ChangeScene : DialogueElem
    {
        public BackgroundDesc scene;
        public bool           transition = true;

        public override bool shouldCoroutine => true;

        public override IEnumerator RunCR(DialogueSystem DS)
        {
            if (transition)
            {
                DS.StartCoroutine(DS.FadeToCR(DS.textDisplay, 0, DS.fadeInTime));
                yield return DS.StartCoroutine(DS.FadeToCR(DS.backgroundImage.GetComponent<CanvasGroup>(), 0, DS.fadeInTime));

                DS.backgroundImage.sprite = scene.image;

                yield return DS.StartCoroutine(DS.FadeToCR(DS.backgroundImage.GetComponent<CanvasGroup>(), 1, DS.fadeInTime));
            }
            else
            {
                DS.backgroundImage.sprite = scene.image;
            }
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
                        if (DS.skipSound != null) SoundManager.PlaySound(DS.skipSound, 0.5f, Random.Range(0.75f, 1.25f));
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
    [SerializeField] private AudioClip          skipSound;
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
                bool transition = true;
                if (tokens.Length > 2)
                {
                    if (tokens[2].ToLower() == "false") transition = false;
                }

                var scene = GetBackground(tokens[1]);
                if (scene == null)
                {
                    Debug.LogError($"Can't find scene {tokens[1]}!");
                }
                var newElem = new ChangeScene() { scene = scene, transition = transition };
                elems.Add(newElem);
                if (startBackgroundSprite == null)
                {
                    startBackgroundSprite = newElem.scene?.image;
                }
            }
            else if (tokens[0] == "clear_color")
            {
                ColorUtility.TryParseHtmlString(tokens[1], out Color color);
                var newElem = new ClearColor() { color = color };
                elems.Add(newElem);
            }
            else if (tokens[0] == "wait")
            {                
                var newElem = new Wait() { waitTime = float.Parse(tokens[1]) };
                elems.Add(newElem);
            }
            else
            {
                var actor = GetActor(tokens[0]);
                if (actor != null)
                {
                    // Get text 
                    string text;
                    var startIndex = str.IndexOf('\"');
                    if (startIndex != -1)
                    {
                        var endIndex = str.IndexOf('\"', startIndex + 1);
                        if (endIndex == -1)
                        {
                            text = str.Substring(startIndex + 1);
                        }
                        else
                        {
                            text = str.Substring(startIndex + 1, endIndex - startIndex - 1);
                        }
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
