using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BallManager : MonoBehaviour {

    public enum Mode
    {
        Flash,
        Disappear
    };

    public Mode mode;
    public Stack<GameObject> unassigned;
    public List<GameObject> targets;
    public List<GameObject> distractors;

    public int numTotal = 5;
    public int numTargets = 2;
    public int numRemove = 2;
    public int numFlash = 2;

    int numDistractors;
    public Color distractorColor = Color.white;
    public Color targetColor = Color.red;
    public Color flashColor = Color.yellow;
    public GameObject Prefab;

    public int[] sendInfo;
    public ToLSL LSLManager;

    // Use this for initialization
    void Start () {
        numDistractors = numTotal - numTargets;
        sendInfo = new int[1];
        unassigned = new Stack<GameObject>();
        targets = new List<GameObject>();
        distractors = new List<GameObject>();
        InstantiateBalls();
        if (mode == Mode.Disappear)
            StartCoroutine(Disappear());
        else if (mode == Mode.Flash)
            StartCoroutine(Flash());
    }
	
	// Update is called once per frame
	void Update () {

	}

    private void InstantiateBalls()
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

    private IEnumerator Disappear()
    {

        AddTargets(numTargets);
        sendInfo[0] = 99;
        LSLManager.pushSample();
        AddDistractors();
        yield return new WaitForSeconds(3f);
        HideTargets();
        sendInfo[0] = numTargets*10+numDistractors;
        LSLManager.pushSample();
        yield return new WaitForSeconds(8f);
        for(int i=1; i<numRemove; i++)
        {
            RemoveObject();
            sendInfo[0] = numTargets * 10 + numDistractors;
            LSLManager.pushSample();
            yield return new WaitForSeconds(3f);
        }
        sendInfo[0] = 100;
        LSLManager.pushSample();
    }

    private IEnumerator Flash()
    {
        AddTargets(numTargets);
        sendInfo[0] = 99;
        LSLManager.pushSample();
        AddDistractors();
        yield return new WaitForSeconds(3f);
        HideTargets();
        sendInfo[0] = numTargets * 10 + numDistractors;
        LSLManager.pushSample();
        yield return new WaitForSeconds(2f);
        for(int i=0; i<numFlash; i++)
        {
            int index = FlashObject();
            if (index / 10 == 1)
            {
                sendInfo[0] = 1;
                LSLManager.pushSample();
                yield return new WaitForSeconds(0.5f);
                targets[index % 10].GetComponent<Renderer>().material.color = distractorColor;
            }
            else
            {
                sendInfo[0] = 0;
                LSLManager.pushSample();
                yield return new WaitForSeconds(0.5f);
                distractors[index % 10].GetComponent<Renderer>().material.color = distractorColor;
            }
            yield return new WaitForSeconds(Random.Range(1.8f, 2.2f));
        }
        sendInfo[0] = 100;
        LSLManager.pushSample();
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
        {
            distractors.Add(unassigned.Pop());
            distractors.Last().tag = "Distractor";
        }
    }

    private void HideTargets()
    { 
        foreach(GameObject target in targets)
            target.GetComponent<Renderer>().material.color = distractorColor;
    }

    private void RemoveTarget()
    {
        Destroy(targets.Last());
        targets.Remove(targets.Last());
        numTargets--;
    }

    private void RemoveDistractor()
    {
        Destroy(distractors.Last());
        distractors.Remove(distractors.Last());
        numDistractors--;
    }

    private void RemoveObject()
    {
        if (Random.Range(0, 2) == 0)
            RemoveDistractor();
        else
            RemoveTarget();
    }

    private int FlashDistractor()
    {
        int index = Random.Range(0, distractors.Count());
        distractors[index].GetComponent<Renderer>().material.color = flashColor;
        return index+20;
    }
    private int FlashTarget()
    {
        int index = Random.Range(0, targets.Count());
        distractors[index].GetComponent<Renderer>().material.color = flashColor;
        return index+10;
    }
    private int FlashObject()
    {
        if (Random.value > 0.5f)
            return FlashTarget();
        else
            return FlashDistractor();
    }
}
