using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressBar : MonoBehaviour
{
    private Slider slider
    {
        get { return GetComponent<Slider>(); }
    }
    private Text progressText
    {
        get { return GetComponentInChildren<Text>(); }
    }

    public string text
    {
        get { return progressText.text; }
        set { progressText.text = value; }
    }

    private IEnumerator Running(float time)
    {
        float curTime = 0;
        while (curTime < time)
        {
            curTime += Time.deltaTime;
            slider.value = curTime / time;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
    }
}
