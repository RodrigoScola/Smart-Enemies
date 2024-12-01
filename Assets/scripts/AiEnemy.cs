using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;


public class AiEnemy : MonoBehaviour {

    public NavMeshLink currentTarget;

    public NavMeshAgent agent;
    
    

    private void Start() {


        // Vector3 startPointWorld = transform.TransformPoint(currentTarget.startPoint);
        // Vector3 endPointWorld = transform.TransformPoint(currentTarget.endPoint);
        

    }
    private void Update() {
        Vector3 pos = currentTarget.transform.position;
        Vector3 start = currentTarget.startPoint;
        Vector3 end = currentTarget.endPoint;

        Debug.Log($"end: {end}");
        Debug.Log($"pos: {pos}");
        Debug.Log($"start: {start}");

        Vector3 direction = currentTarget.transform.position +
                            end;

        var inverseRotation = Quaternion.Inverse(transform.rotation);

        var transformed = direction;

        transformed.x = currentTarget.transform.position.x;
        transformed.y = transformed.y * inverseRotation.y;
        transformed.z = transformed.z * inverseRotation.z;
        
        Debug.Log($"myPos: {transformed}");


        transform.position = transformed;

        // transform.position = new Vector3(
        //     -0.1008614f + 0.8595443f,
        //     // 7.2499f + -0.1008614f,
        //     //Mathf.Abs( currentTarget.transform.position.x + start.x),
        //     
        //     2.264818f + 1.398302f,
        //     //Mathf.Abs( currentTarget.transform.position.y + start.y), 
        //     4.332209f + 3.589845f
        //     // Mathf.Abs( currentTarget.transform.position.z + start.z)
        //     );


    }
}