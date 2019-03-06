using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Reference : MonoBehaviour
{
    //格子坐标
    public int x;
    public int y;
    //颜色材质区分
	public Material startMat;
	public Material endMat;
	public Material barrierMat;

    //判断当前格子的类型
	void OnTriggerEnter (Collider other)
	{
		if (other.name == "Start") 
		{
			GetComponent<MeshRenderer> ().material = startMat;
			Algorithm.instance.grids [x, y].type = GridType.Start;
			Algorithm.instance.openList.Add (Algorithm.instance.grids [x, y]);
			Algorithm.instance.startX = x;
			Algorithm.instance.startY = y;
		} 
		else if (other.name == "End") 
		{
			GetComponent<MeshRenderer> ().material = endMat;
			Algorithm.instance.grids [x, y].type = GridType.End;
			Algorithm.instance.targetX = x;
			Algorithm.instance.targetY = y;
		} 
		else if (other.name == "Barriers") 
		{
			GetComponent<MeshRenderer> ().material = barrierMat;
			Algorithm.instance.grids [x, y].type = GridType.Barrier;
		}
	}

	void OnMouseDown ()
	{
		GetComponent<MeshRenderer> ().material = barrierMat;
		Algorithm.instance.grids [x, y].type = GridType.Barrier;
	}
}
