using UnityEngine;

public class Visual : MonoBehaviour
{

    static public void Marker(Vector3 pos, float radius)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.transform.position = pos;



        marker.transform.localScale = new Vector3(radius, 100 / 2, radius);

        Destroy(marker, 5f);
    }


    static public void Sphere(Vector3 pos, float radius)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;


        Destroy(sphere, 5f);

    }

}
