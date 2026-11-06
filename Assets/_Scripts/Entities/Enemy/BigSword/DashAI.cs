using UnityEngine;
using UnityEngine.AI;

public class DashAI : MonoBehaviour
{
    [SerializeField] BigSwordEnemy enemy;
    [SerializeField] float minDashRange = 4f;   // now measured along the navmesh path
    [SerializeField] float maxDashRange = 8f;
    [SerializeField] float checkInterval = 0.5f;
    [SerializeField] float snapRadius = 2f;     // how far to search for the navmesh when snapping positions

    Transform playerTransform;
    Transform myTransform;
    float maxSqr;
    float nextCheckTime;

    // reused every check so nothing is allocated
    NavMeshPath path;
    Vector3[] corners = new Vector3[64];

    void Start()
    {
        playerTransform = EntityManager.Instance.GetPlayerObject().transform;
        myTransform = enemy.transform;

        maxSqr = maxDashRange * maxDashRange;
        path = new NavMeshPath();

        nextCheckTime = Time.time + Random.value * checkInterval;
    }

    void Update()
    {
        if (Time.time < nextCheckTime)
            return;
        float random = Random.Range(0, 1f);
        if (random > .7f)
            return;
        nextCheckTime = Time.time + checkInterval;

        // cheap pre-check: a path can never be shorter than the straight line,
        // so if the straight line is already past max range, skip the path calculation
        Vector3 flat = playerTransform.position - myTransform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude > maxSqr)
            return;

        // expensive part: real walking distance
        float pathDist = GetPathDistance(myTransform.position, playerTransform.position);
        if (pathDist < 0f)
            return;   // no complete path

        if (pathDist >= minDashRange && pathDist <= maxDashRange)
        {
            enemy.Dash(playerTransform.position);
        }
    }

    // returns the walking distance along the navmesh, or -1 if there's no complete path
    float GetPathDistance(Vector3 from, Vector3 to)
    {
        if (!NavMesh.SamplePosition(from, out NavMeshHit fromHit, snapRadius, NavMesh.AllAreas))
            return -1f;
        if (!NavMesh.SamplePosition(to, out NavMeshHit toHit, snapRadius, NavMesh.AllAreas))
            return -1f;

        if (!NavMesh.CalculatePath(fromHit.position, toHit.position, NavMesh.AllAreas, path))
            return -1f;
        if (path.status != NavMeshPathStatus.PathComplete)
            return -1f;

        int count = path.GetCornersNonAlloc(corners);
        float length = 0f;
        for (int i = 1; i < count; i++)
        {
            length += Vector3.Distance(corners[i - 1], corners[i]);
            if (length > maxDashRange)
                return length;   // already too far, stop adding
        }
        return length;
    }
}