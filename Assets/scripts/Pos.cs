using UnityEngine;

[System.Serializable]
public class Pos {
    public int Id;
    public Vector3 rayPos;
    private static int Ids;
    public float interest;
    public float danger;


    public Pos(Vector3 pos) {
        rayPos = pos;
    }

    public bool selected;

    public float GetScore() {
        // if (interest > danger) {
            return interest;
            // return interest - danger;
        // }
        //
        // return 0;

        // return -danger;

    }


    public Color GetColor() {
        Color color = Color.red;

        if (selected) {
            color = Color.green;
        }
        else {
            color = Color.red;
        }

        return color;
    }


    public Pos() {
        Ids++;
        Id = Ids;
    }
}