using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BallManagerWithFlicker : MonoBehaviour
{
    public enum Mode
    {
        Flicker,
        Disappear
    };
    public Mode mode;
    public Stack<GameObject> unassigned;
    public List<GameObject> targets;
    public List<GameObject> distractors;

    public int numTotal = 8;
    public int numTargets = 3;
    public Color distractorColor = Color.white;
    public Color targetColor = Color.red;
    public GameObject Prefab;

    public Color normalColor = new Color(1, 1, 1, 1);
    public Color flickColor = new Color(0, 0, 0, 1);
    public Effect Flick = new Effect();
    bool flickNow = false;
    GameObject flickerTarget;
    GameObject flickerDistractor;

    // Use this for initialization
    void Start()
    {
        unassigned = new Stack<GameObject>();
        targets = new List<GameObject>();
        distractors = new List<GameObject>();
        instantiateBalls();
        if (mode == Mode.Disappear)
            StartCoroutine(Disappear());
        else if (mode == Mode.Flicker)
            StartCoroutine(Flicker());
    }

    // Update is called once per frame
    void Update()
    {
        if (mode == Mode.Flicker)
            ColorFlicker();
    }

    private void instantiateBalls()
    {
        List<Vector3> initialPositions = new List<Vector3>();
        while (unassigned.Count < numTotal)
        {
            bool overlap = false;
            var position = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 10);
            foreach (Vector3 pos in initialPositions)
            {
                if (Vector3.Distance(position, pos) <= Prefab.GetComponent<SphereCollider>().radius)
                    overlap = true;
            }
            if (!overlap)
            {
                initialPositions.Add(position);
                GameObject newBall = Instantiate(Prefab, position, Quaternion.identity);
                newBall.transform.parent = gameObject.transform;
                unassigned.Push(newBall);
            }
        }

    }

    private IEnumerator Flicker()
    {
        AddTargets(numTargets);
        AddDistractors();
        yield return new WaitForSeconds(3f);
        flickNow = true;
        yield return new WaitForSeconds(1f);
        flickNow = false;
        yield return new WaitForSeconds(3f);
        flickNow = true;
        yield return new WaitForSeconds(1f);
        flickNow = false;
        yield return new WaitForSeconds(3f);
        flickNow = true;
        yield return new WaitForSeconds(1f);
        flickNow = false;

    }

    private IEnumerator Disappear()
    {

        AddTargets(numTargets);

        AddDistractors();
        yield return new WaitForSeconds(3f);
        HideTargets();
        yield return new WaitForSeconds(8f);
        RemoveTarget();
        yield return new WaitForSeconds(5f);
        RemoveTarget();
        yield return new WaitForSeconds(5f);
        RemoveTarget();
        yield return new WaitForSeconds(5f);
    }

    private void AddTargets(int num)
    {
        for (int i = 0; i < num; i++)
        {
            targets.Add(unassigned.Pop());
            targets.Last().GetComponent<Renderer>().material.color = Color.red;
            targets.Last().tag = "Target";
        }
    }

    private void AddDistractors()
    {
        while (unassigned.Count != 0)
            distractors.Add(unassigned.Pop());
    }

    private void HideTargets()
    {
        foreach (GameObject target in targets)
            target.GetComponent<Renderer>().material.color = distractorColor;
    }

    private void RemoveTarget()
    {
        Destroy(targets.Last());
        targets.Remove(targets.Last());
    }

    private void ColorFlicker()
    {
        if (flickNow)
        {
            bool isTarget = (Random.value > 0.5f);
            if (isTarget)
            {
                flickerTarget = targets[Random.Range(0, targets.Count)];
                flickerTarget.GetComponent<Renderer>().material.color = Color.Lerp(normalColor, flickColor, Mathf.Round(Mathf.PingPong(Time.time * Flick.frequency, Flick.flickColorDuration)));
            }
            else
            {
                flickerDistractor = distractors[Random.Range(0, distractors.Count)];
                flickerDistractor.GetComponent<Renderer>().material.color = Color.Lerp(normalColor, flickColor, Mathf.Round(Mathf.PingPong(Time.time * Flick.frequency, Flick.flickColorDuration)));
            }
        }
        else
        {
            flickerTarget.GetComponent<Renderer>().material.color = normalColor;
            flickerDistractor.GetComponent<Renderer>().material.color = normalColor;
        }
    }

    [System.Serializable]
    public class Effect
    {
        //Parameters to setup
        public float frequency = 7.5f;
        public float normalColorDuration = 1;
        public float flickColorDuration = 1;
    }

}
