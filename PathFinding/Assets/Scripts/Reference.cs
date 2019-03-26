using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Reference : MonoBehaviour
{
    //格子坐标
    public int x;
    public int y;
    //颜色材质区分
    public Material normalMat;
    public Material startMat;
	public Material endMat;
	public Material barrierMat;
    public Material barrierMat1;
	public Material barrierMat2;
    //G值
    private int count = 0;

    void OnMouseDown ()
	{
        //Debug.Log("(" + x + "," + y + ")" + Algorithm.instance.grids[x, y].g+","+Algorithm.instance.grids[x,y].f);
        if(Algorithm.choice<1)
		{
			GetComponent<MeshRenderer> ().material = startMat;
			Algorithm.instance.grids [x, y].type = GridType.Start;
			Algorithm.instance.openList.Add (Algorithm.instance.grids [x, y]);
			Algorithm.instance.startX = x;
			Algorithm.instance.startY = y;
            Algorithm.choice += 1;
        }
		else if(Algorithm.choice<2)
		{
			GetComponent<MeshRenderer> ().material = endMat;
			Algorithm.instance.grids [x, y].type = GridType.End;
			Algorithm.instance.targetX = x;
			Algorithm.instance.targetY = y;
			Algorithm.choice += 1;
		}
		else if(count < 1 && Algorithm.instance.grids [x,y].type != GridType.Start && Algorithm.instance.grids [x,y].type != GridType.End)
		{
			GetComponent<MeshRenderer> ().material = barrierMat;
			Algorithm.instance.grids [x, y].type = GridType.Barrier;
            Algorithm.choice += 1;
            count += 1;
        }
		else if(count < 2 && Algorithm.instance.grids [x,y].type != GridType.Start && Algorithm.instance.grids [x,y].type != GridType.End)
		{
			GetComponent<MeshRenderer> ().material = barrierMat1;
			Algorithm.instance.grids [x, y].type = GridType.Barrier1;
            Algorithm.choice += 1;
            count += 1;
		}
		else if(count < 3 && Algorithm.instance.grids [x,y].type != GridType.Start && Algorithm.instance.grids [x,y].type != GridType.End)
		{
			GetComponent<MeshRenderer> ().material = barrierMat2;
			Algorithm.instance.grids [x, y].type = GridType.Barrier2;
            Algorithm.choice += 1;
            count += 1;
		}
		else if(Algorithm.instance.grids [x,y].type != GridType.Start && Algorithm.instance.grids [x,y].type != GridType.End)
		{
			GetComponent<MeshRenderer> ().material = normalMat;
			Algorithm.instance.grids [x, y].type = GridType.Normal;
            Algorithm.choice += 1;
            count =0;
		}
	}
}
