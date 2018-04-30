using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BallManager : MonoBehaviour {

    // Drop down menu on the inspector panel to select different modes
    public enum Mode
    {
        Flash,
        Disappear
    };

    public Mode mode;
    public Stack<GameObject> unassigned;
    public List<GameObject> targets;
    public List<GameObject> distractors;

    public int numTrials = 3;   // Number of trials
    public int numTotal = 5;    // Total number of objects;
    public int numTargets = 2;  // Number of targets;
    public int numRemove = 2;   // Number of objects to be removed in Disappear Mode;
    public int numFlash = 3;    // Number of flashes in Flash Mode;

    int numDistractors; // Number of Distractors
    public Color distractorColor = Color.white;
    public Color targetColor = Color.red;
    public Color flashColor = Color.yellow;
    public GameObject Prefab;   // BouncyBall prefab

    public int[] sendInfo;  // The information to be send via LSL
    public ToLSL LSLManager;   
    GameObject[] result;    // Stores the objects that subject selects as the final result.

    // Use this for initialization
    void Start () {
        // Initialize variables
        numDistractors = numTotal - numTargets;
        sendInfo = new int[1];
        unassigned = new Stack<GameObject>();
        targets = new List<GameObject>();
        distractors = new List<GameObject>();
        InstantiateBalls();
        // Check mode
        if (mode == Mode.Disappear)
            StartCoroutine(Disappear());
        else if (mode == Mode.Flash)
            StartCoroutine(Flash());
    }
	
	// Update is called once per frame
	void Update () {

	}

    /**
     *  Instantiate all objects in the scene.
     */
    private void InstantiateBalls()
    {
        // Stores all the generated initial positions
        List<Vector3> initialPositions = new List<Vector3>();
        // Generates initial positions and instantiate a ball at that position
        while (unassigned.Count < numTotal)
        {
            bool overlap = false;
            var position = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 10);
            // Check for overlapping
            foreach (Vector3 pos in initialPositions)
                {
                if (Vector3.Distance(position, pos) <= Prefab.GetComponent<SphereCollider>().radius)
                    overlap = true;
                }
            if (!overlap)
            {
                initialPositions.Add(position);
                GameObject newBall = Instantiate(Prefab, position, Quaternion.identity);
                // Put the ball as a child under Balls
                newBall.transform.parent = gameObject.transform;
                unassigned.Push(newBall);
            }
        }
         
    }

    // Coroutine for Disappear mode. Coroutine is only called once. 
    private IEnumerator Disappear()
    {
        for (int i = 0; i < numTrials; i++)
        {
            // Assign targets and distractors and send the starting timestamp
            AddTargets(numTargets); 
            sendInfo[0] = 99;
            LSLManager.pushSample();
            AddDistractors();
            yield return new WaitForSeconds(3f);
            
            // Hide targets and send a timestamp
            HideTargets();
            sendInfo[0] = numTargets * 10 + numDistractors;
            LSLManager.pushSample();
            yield return new WaitForSeconds(3f);

            // Remove an object each loop and send a timestamp
            for (int j = 0; j < numRemove; j++)
            {
                RemoveObject();
                sendInfo[0] = numTargets * 10 + numDistractors;
                LSLManager.pushSample();
                yield return new WaitForSeconds(3f);
            }

            // Send a timestamp when trial ends
            sendInfo[0] = 100;
            LSLManager.pushSample();

            // Stop the balls from moving and let the user 
            // select the remaining targets
            FreezeBalls();
            UserSelect();
        }
        sendInfo[0] = 1000;
        LSLManager.pushSample();
    }

    // Coroutine for Flash mode. Coroutine is only called once. 
    private IEnumerator Flash()
    {
        for (int j = 0; j < numTrials; j++)
        {
            // Assign targets and distractors and send the starting timestamp
            AddTargets(numTargets);
            sendInfo[0] = 99;
            LSLManager.pushSample();
            AddDistractors();
            yield return new WaitForSeconds(3f);

            // Hide targets and send a timestamp
            HideTargets();
            sendInfo[0] = numTargets * 10 + numDistractors;
            LSLManager.pushSample();
            yield return new WaitForSeconds(2f);

            // Make an object flash each loop
            for (int i = 0; i < numFlash; i++)
            {
                int index = FlashObject();
                if (index / 10 == 1)
                {
                    Debug.Log("Target FLash");
                    sendInfo[0] = 1;
                    LSLManager.pushSample();
                    yield return new WaitForSeconds(0.2f);
                    targets[index % 10].GetComponent<Renderer>().material.color = distractorColor;
                }
                else
                {
                    Debug.Log("Distractor Flash");
                    sendInfo[0] = 0;
                    LSLManager.pushSample();
                    yield return new WaitForSeconds(0.2f);
                    distractors[index % 10].GetComponent<Renderer>().material.color = distractorColor;
                }
                yield return new WaitForSeconds(Random.Range(1.9f, 2.1f));
            }

            // Send a timestamp when trial ends
            sendInfo[0] = 100;
            LSLManager.pushSample();

            // Stop the balls from moving and let the user 
            // select the remaining targets
            FreezeBalls();
            UserSelect();
        }
        sendInfo[0] = 1000;
        LSLManager.pushSample();
    }

    // Assign targets and color them
    private void AddTargets(int num)
    {
        for (int i = 0; i < num; i++)
        {
            targets.Add(unassigned.Pop());
            targets.Last().GetComponent<Renderer>().material.color = targetColor;
            targets.Last().tag = "Target";
        }
    }

    // Assign Distractors
    private void AddDistractors()
    {
        while (unassigned.Count != 0)
        {
            distractors.Add(unassigned.Pop());
            distractors.Last().tag = "Distractor";
        }
    }

    // Hide targets
    private void HideTargets()
    { 
        foreach(GameObject target in targets)
            target.GetComponent<Renderer>().material.color = distractorColor;
    }

    // Remove a target from scene
    private void RemoveTarget()
    {
        Destroy(targets.Last());
        targets.Remove(targets.Last());
        numTargets--;
    }

    // Remove a distractor from scene
    private void RemoveDistractor()
    {
        Destroy(distractors.Last());
        distractors.Remove(distractors.Last());
        numDistractors--;
    }

    // Remove an object from scene (either target or distractor)
    private void RemoveObject()
    {
        if (Random.Range(0, 2) == 0)
            RemoveDistractor();
        else
            RemoveTarget();
    }

    // Make a distractor flash
    private int FlashDistractor()
    {
        int index = Random.Range(0, distractors.Count());
        distractors[index].GetComponent<Renderer>().material.color = flashColor;
        return index+20;
    }

    // Make a target flash
    private int FlashTarget()
    {
        int index = Random.Range(0, targets.Count());
        targets[index].GetComponent<Renderer>().material.color = flashColor;
        return index+10;
    }

    // Make an object flash (either target or distractor
    private int FlashObject()
    {
        if (Random.value > 0.5f)
            return FlashTarget();
        else
            return FlashDistractor();
    }

    // Stop balls from moving
    private void FreezeBalls()
    {
        GameObject[] endTargets;
        GameObject[] endDistractors;
        endTargets = GameObject.FindGameObjectsWithTag("Target");
        endDistractors = GameObject.FindGameObjectsWithTag("Distractor");
        foreach (GameObject target in endTargets)
        {
            target.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        foreach(GameObject distractor in endDistractors)
        {
            distractor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    // Check the result from the user
    private int CheckResult()
    {
        int res = 0;
        foreach(GameObject gb in result)
        {
            if (gb.tag == "target")
                res += 10;
            else
                res += 1;
        }
        return res;
    }

    // Stores the selected objects and send via LSL
    private void UserSelect()
    {
        if (result != null)
        {
            int res = CheckResult();
            sendInfo[0] = res + 100;
            LSLManager.pushSample();
        }
    }
}
