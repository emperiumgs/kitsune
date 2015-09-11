using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressBar : MonoBehaviour
{
    private GameObject background
    {
        get { return transform.FindChild("Background").gameObject; }
    }
    private GameObject progressBar
    {
        get { return transform.FindChild("CompletionBar").gameObject; }
    }
    private Text progressText
    {
        get { return GetComponentInChildren<Text>(); }
    }
    private RectTransform rect(GameObject source)
    {
        return source.GetComponent<RectTransform>();
    }
    private Image image(GameObject source)
    {
        return source.GetComponent<Image>();
    }

    public Color progressColor
    {
        get { return image(progressBar).color; }
        set { image(progressBar).color = value; }
    }
    public Vector2 totalSize
    {
        get { return rect(background).sizeDelta; }
    }
    public Vector2 curSize
    {
        get { return rect(progressBar).sizeDelta; }
        set { rect(progressBar).sizeDelta = value; }
    }
    public string text
    {
        get { return progressText.text; }
        set { progressText.text = value; }
    }

    private void Awake()
    {
        GetComponent<RectTransform>().SetParent(FindObjectOfType<Canvas>().transform, false);
    }

    /// <summary>
    /// Auto runs the progress bar during the given time
    /// </summary>
    /// <param name="time">The amount of time to run</param>
    public void AutoRun(float time)
    {
        StartCoroutine(Running(time));
    }

    private IEnumerator Running(float time)
    {
        float curTime = 0;
        while (curTime < time)
        {
            curTime += Time.deltaTime;
            curSize = new Vector2(curTime/time * totalSize.x, totalSize.y);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
    }
}
