using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Algorithm : MonoBehaviour
{
    //算法类型
    public int AlgorithmType = 1;
    //参考物体预设体
    public GameObject reference;
    public static Algorithm instance;
    //地图大小
    private int size;
    //格子数组
    public Grid[,] grids;
	public int[,,,] allgrids;
	//格子数组对应的参考物（方块）对象
	public GameObject[,] objs;
    //基础物体
	private Transform plane;
	private Transform start;
	private Transform end;
	private Transform barriers;
    //开启列表
	public ArrayList openList;
	//关闭列表
	public ArrayList closeList;
	//目标点坐标
	public int targetX;
	public int targetY;
	//起始点坐标
	public int startX;
	public int startY;
	//结果栈
	private Stack<string> parentList;
    private Stack<string> parentlists;
    private int Pathlength;
    //流颜色参数
    private float alpha = 0;
	private float incrementPer = 0;
    //展示信息
    private Text Timetext;
    private Text Pathtext;
    private Text AType;
    //时间差值
    TimeSpan ts;
    double timelast;
    bool done = false;
    //监听按钮
	bool findingstart = false;
    public GameObject startbutton;


    //选定
    void Awake()
    {
        instance = this;
        openList = new ArrayList ();
		closeList = new ArrayList ();
		plane = GameObject.Find ("Plane").transform;
		start = GameObject.Find ("Start").transform;
		end = GameObject.Find ("End").transform;
		//barriers = GameObject.Find ("Barriers").transform;
		parentList = new Stack<string> ();
		Timetext = GameObject.Find ("TimeResult").GetComponent<Text> ();
		Pathtext = GameObject.Find ("Length").GetComponent<Text> ();
		AType = GameObject.Find ("AlgorithmType").GetComponent<Text> ();
    }

	//初始化
    void Init()
    {
        //计算行列数
        size = 20;
		grids = new Grid[size, size];
        allgrids = new int[size, size, size, size];
        objs = new GameObject[size, size];
		//起始坐标
		Vector3 startPos = 
			new Vector3 (plane.localScale.x * -5, 0, (float)plane.localScale.z * -5);
		//生成参考物体（Cube）
		for (int i = 0; i < size; i++) 
		{
			for (int j = 0; j < size; j++) 
			{
				grids [i, j] = new Grid (i, j);
				GameObject item = (GameObject)Instantiate (reference, 
					                  new Vector3 (i * 0.5f, 0, j * 0.5f) + startPos + new Vector3((float)0.2, (float)0.0, (float)0.2), 
					                  Quaternion.identity);
                item.transform.GetChild (0).GetComponent<Reference> ().x = i;
				item.transform.GetChild (0).GetComponent<Reference> ().y = j;
				objs [i, j] = item;
			}
		}
    }

	//生成结果
	void GenerateResult (Grid currentGrid)
	{
		//如果当前格子有父格子
		if (currentGrid.parent != null) 
		{
			//添加到父对象栈（即结果栈）
			parentList.Push (currentGrid.x + "|" + currentGrid.y);
			//递归获取
			GenerateResult (currentGrid.parent);
		}
	}

	//计算曼哈顿距离
	int Manhattan (int x, int y)
	{
		return (int)(Mathf.Abs (targetX - x) + Mathf.Abs (targetY - y)) * 10;
	}


	//A*
	IEnumerator Astar ()
	{
        //等待前面操作完成
        yield return new WaitForSeconds (0.1f);
		//记录开始时间
		DateTime tAstar1 = DateTime.Now;
        DateTime t_new1 = new DateTime();
		//添加起始点
		openList.Add (grids [startX, startY]);
		//声明当前格子变量，并赋初值
		Grid currentGrid = openList [0] as Grid;
		//循环遍历路径最小F的点
		while (openList.Count > 0 && currentGrid.type != GridType.End) 
		{
			//获取此时最小F点
			currentGrid = openList [0] as Grid;
			//如果当前点就是目标
			if (currentGrid.type == GridType.End) 
			{
				Debug.Log ("Find");
				//生成结果
				GenerateResult (currentGrid);
                break;
            }
			//上下左右，左上左下，右上右下，遍历
			for (int i = -1; i <= 1; i++) 
			{
				for (int j = -1; j <= 1; j++) 
				{
					if (i != 0 || j != 0)
					{
						//计算坐标
						int x = currentGrid.x + i;
						int y = currentGrid.y + j;
						//点亮访问
						objs [x, y].transform.GetChild (0).GetComponent<MeshRenderer> ().material.color
						= new Color (0.4f, 0.4f, 0.4f, 1);
						//如果未超出所有格子范围，不是障碍物，不是重复点
						if (x >= 0 && y >= 0 && x < size && y < size && grids [x, y].type != GridType.Barrier && !closeList.Contains (grids [x, y])) 
						{
							//计算G值
							int g = currentGrid.g + (int)(Mathf.Sqrt ((Mathf.Abs (i) + Mathf.Abs (j))) * 10);;
							//与原G值对照
							if (grids [x, y].g == 0 || grids [x, y].g > g) 
							{
								//更新G值
								grids [x, y].g = g;
								//更新父格子
								grids [x, y].parent = currentGrid;
							}
							
							//计算H值
							grids [x, y].h = Manhattan(x, y);
							//计算F值
							grids [x, y].f = grids [x, y].g + grids [x, y].h;
							//如果未添加到开启列表
							if (!openList.Contains(grids [x, y])) 
							{
								//添加
								openList.Add(grids [x, y]);
							}
							//重新排序
							openList.Sort();
						}
					}
				}
			}
			//完成遍历添加该点到关闭列表
			closeList.Add(currentGrid);
			//从开启列表中移除
			openList.Remove(currentGrid);
			//如果开启列表空，未能找到路径
			if (openList.Count == 0) 
			{
				Debug.Log ("Can not Find");
			}
		}
		//获取结束时间
        t_new1= DateTime.Now;
		ts = (t_new1 - tAstar1);
        timelast = (int)ts.Milliseconds;
		Pathlength = parentList.Count;
        done = true;
    }

	//Dijkstra
	IEnumerator Dijkstra()
	{
		//等待前面操作完成
        yield return new WaitForSeconds (0.1f);
		//记录开始时间
		DateTime tAstar1 = DateTime.Now;
        DateTime t_new1 = new DateTime();
		//赋值
		Grid currentGrid;//记录当前遍历点
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                grids[i, j].f = 99;
                openList.Add(grids[i, j]);
            }
        }
        grids[startX, startY].f = 0;
		
		while(openList.Count > 0)
		{
            openList.Sort();
			//将最小点移到关闭表
			currentGrid = openList [0] as Grid;
            closeList.Add(currentGrid);
            openList.Remove(currentGrid);
            //如果当前点就是目标
            if(closeList.Contains(grids[targetX,targetY]))
			{
                Debug.Log ("Find");
				//生成结果
				GenerateResult (currentGrid);
                break;
            }
            //更新距离
            //上下左右，左上左下，右上右下，遍历
            for (int i = -1; i <= 1; i++) 
			{
                for (int j = -1; j <= 1; j++)
                {
                    if (i != 0 || j != 0)
                    {
						//计算坐标
						int x = currentGrid.x + i;
						int y = currentGrid.y + j;
                        //如果未超出所有格子范围，不是障碍物，不是重复点
                        if (x >= 0 && y >= 0 && x < size && y < size && grids[x, y].type != GridType.Barrier && !closeList.Contains(grids[x, y]))
                        {
							//点亮访问
							objs [x, y].transform.GetChild (0).GetComponent<MeshRenderer> ().material.color
							= new Color (0.4f, 0.4f, 0.4f, 1);
							if(currentGrid.f+1 < grids[x,y].f)
							{
                                grids[x, y].f = currentGrid.f + 1;
								//更新父格子
								grids[x, y].parent = currentGrid;
                            }
                        }
                    }
                }
            }
        }
		//获取结束时间
        t_new1= DateTime.Now;
		ts = (t_new1 - tAstar1);
        timelast = (int)ts.Milliseconds;
        //timelast = Math.Round(timelast / 10000, 2);
		Pathlength = parentList.Count;
        //Debug.Log(timelast + "/" + Pathlength);
        done = true;
    }

	//Floyd
	IEnumerator Floyd()
	{
		//等待前面操作完成
        yield return new WaitForSeconds (0.1f);
		//记录开始时间
		DateTime tAstar1 = DateTime.Now;
        DateTime t_new1 = new DateTime();
        //点亮访问
        for (int i = 0; i < size;i++)
        {
            for (int j = 0; j < size;j++)
            {
				objs [i, j].transform.GetChild (0).GetComponent<MeshRenderer> ().material.color
							= new Color (0.4f, 0.4f, 0.4f, 1);
            }
        }
        for (int m = 0; m < size; m++)
        {
            for (int n = 0; n < size; n++)
            {
				allgrids[m, n, m, n] = 0;
                for (int i = 0; i <size; i++)
                {
                    for (int j = 0; j <size; j++)
                    {
						if(grids [i, j].type == GridType.Barrier)
						{
							allgrids[m, n, i, j] = 99;
						}
						else if (Mathf.Abs(m-i)+Mathf.Abs(n-j)==1)
						{
                            allgrids[m, n, i, j] = 1;
                        }
						else if(Mathf.Abs(m-1)==1&&Mathf.Abs(n-j)==1)
						{
							allgrids[m, n, i, j] = 1;
						}
						else
						{
							allgrids[m, n, i, j] = 99;
						}
                    }
                }
            }
        }
        for (int k1 = 0; k1 < size; k1++)
		{
			for (int k2 = 0; k2 < size; k2++)
        	{
            	for (int m = 0; m < size; m++)
            	{
                	for (int n = 0; n < size; n++)
                	{
						for (int x = 0; x < size; x++)
           	 			{
                			for (int y = 0; y < size; y++)
                			{
                                allgrids[m, n, x, y] = Math.Min(allgrids[m, n, x, y], allgrids[m, n, k1, k2] + allgrids[k1, k2, x, y]);
                            }
            			}		
                	}
            	}
        	}
		}
		//Debug.Log(allgrids[startX,startY,targetX,targetY]);

        //计算路径
        //添加起始点
        openList.Add(grids[startX, startY]);
        //声明当前格子变量，并赋初值
        Grid currentGrid = openList [0] as Grid;
		//循环遍历路径最小F的点
		while (openList.Count > 0 && currentGrid.type != GridType.End) 
		{
			//获取此时最小F点
			currentGrid = openList [0] as Grid;
			//如果当前点就是目标
			if (currentGrid.type == GridType.End) 
			{
				Debug.Log ("Find");
				//生成结果
				GenerateResult (currentGrid);
                break;
            }
			//上下左右，左上左下，右上右下，遍历
			for (int i = -1; i <= 1; i++) 
			{
				for (int j = -1; j <= 1; j++) 
				{
					if (i != 0 || j != 0)
					{
						//计算坐标
						int x = currentGrid.x + i;
						int y = currentGrid.y + j;
						//如果未超出所有格子范围，不是障碍物，不是重复点
						if (x >= 0 && y >= 0 && x < size && y < size && grids [x, y].type != GridType.Barrier && !closeList.Contains (grids [x, y])) 
						{
							//计算G值
							int g = currentGrid.g + (int)(Mathf.Sqrt ((Mathf.Abs (i) + Mathf.Abs (j))) * 10);;
							//与原G值对照
							if (grids [x, y].g == 0 || grids [x, y].g > g) 
							{
								//更新G值
								grids [x, y].g = g;
								//更新父格子
								grids [x, y].parent = currentGrid;
							}
							
							//计算H值
							grids [x, y].h = Manhattan(x, y);
							//计算F值
							grids [x, y].f = grids [x, y].g + grids [x, y].h;
							//如果未添加到开启列表
							if (!openList.Contains(grids [x, y])) 
							{
								//添加
								openList.Add(grids [x, y]);
							}
							//重新排序
							openList.Sort();
						}
					}
				}
			}
			//完成遍历添加该点到关闭列表
			closeList.Add(currentGrid);
			//从开启列表中移除
			openList.Remove(currentGrid);
			//如果开启列表空，未能找到路径
			if (openList.Count == 0) 
			{
				Debug.Log ("Can not Find");
			}
		}
		//获取结束时间
        t_new1= DateTime.Now;
		ts = (t_new1 - tAstar1);
        timelast = (int)ts.Milliseconds;
        //timelast = Math.Round(timelast / 10000, 2);
		Pathlength = parentList.Count;
        //Debug.Log(timelast + "/" + Pathlength);
        done = true;
	}
	



	//展示结果
	IEnumerator ShowResult ()
	{
		//等待前面计算完成
		yield return new WaitForSeconds (0.3f);
        //计算每帧颜色值增量
        incrementPer = 1 / (float)parentList.Count;
        
        //展示结果
        while (parentList.Count != 0) 
		{
            //出栈
            string str = parentList.Pop ();
			//等0.1秒
			yield return new WaitForSeconds (0.1f);
			//拆分获取坐标
			string[] xy = str.Split (new char[]{ '|' });
			int x = int.Parse (xy [0]);
			int y = int.Parse (xy [1]);
			//当前颜色值
			alpha += incrementPer;
			//以颜色方式绘制路径
			objs [x, y].transform.GetChild (0).GetComponent<MeshRenderer> ().material.color
			= new Color (1, 1-alpha, 0, 1);
		}
    }

	


    void Start()
    {
        Init();
        startbutton.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            findingstart = true;
        });
    }

	private void Update() 
	{
		if(done==true)
		{
			//展示时间
        	Timetext.text = "时间：" + timelast + "ms";
        	Pathtext.text = "路径长度:" + Pathlength;
		}

		if(findingstart==true)
		{
			if(AlgorithmType==1)
			{
				StartCoroutine(Astar());
				StartCoroutine(ShowResult());
            	AType.text = "标准Astar";
        	}
			if(AlgorithmType==2)
			{	
            	StartCoroutine(Dijkstra());
            	StartCoroutine(ShowResult());
            	AType.text = "Dijkstra";
        	}
			if(AlgorithmType==3)
			{
            	StartCoroutine(Floyd());
            	StartCoroutine(ShowResult());
				AType.text = "Floyd";
			}
			if(AlgorithmType==4)
			{
				StartCoroutine(ShowResult());
				AType.text = "改进Astar";
			}	
		}
	}

	
	/*
	//AdvancedAstar
	IEnumerator AdvancedAstar()
	{
		//等待前面操作完成
        yield return new WaitForSeconds (0.1f);
	}
	*/
}



	