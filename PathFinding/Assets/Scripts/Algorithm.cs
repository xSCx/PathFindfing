using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Algorithm : MonoBehaviour
{
    //调参
    public float omega = 0.05f;
    public int g1 = 10;
    public int g2 = 20;
    public int g3 = 30;
    //算法类型
    private int AlgorithmType = 1;
    private int gettype;
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
	public GameObject Dijkstrabutton;
	public GameObject Floydbutton;
	public GameObject Astarbutton;
	public GameObject AdAstarbutton;
	public GameObject resetbutton;
	//choice
    public static int choice = 0;
	private bool ran = false;
	private bool recorded = false;



    //选定
    void Awake()
    {
        instance = this;
        openList = new ArrayList ();
		closeList = new ArrayList ();
		plane = GameObject.Find ("Plane").transform;
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
            //Debug.Log(currentGrid.x + "," + currentGrid.y);
            //递归获取
            GenerateResult (currentGrid.parent);
		}
	}

	//计算曼哈顿距离
	int Manhattan (int x, int y)
	{
		return (int)(Mathf.Abs (targetX - x) + Mathf.Abs (targetY - y)) * 10;
	}

	//改进启发函数
	int CosH(int startx, int starty, int m, int n, int endx, int endy)
	{
        int coss;
        float cos
        = ((endx - startx) * (m - startx) + (endy - starty) * (n - starty)) 
		/ (Mathf.Sqrt((startx - m) * (startx - m) + (starty - n) * (starty - n))) * Mathf.Sqrt((startx - endx) * (startx - endx) + (starty - endy) * (starty - endy));
        coss = (int)Mathf.Round(cos);
        return coss;
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
        closeList.Add(grids[startX, startY]);
        //声明当前格子变量，并赋初值
        Grid currentGrid = openList [0] as Grid;
        Grid lastgrid = new Grid(startX, startY);
        //获取此时最小F点
        int compare = 999;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grids[i, j].f < compare)
                {
                    compare = (int)grids[i, j].f;
                }
            }
        }
        //循环遍历路径最小F的点
        while (currentGrid.type != GridType.End)
        {
        	currentGrid = lastgrid;
			//如果当前点就是目标
        	if (currentGrid.type == GridType.End)
        	{
           	 	Debug.Log("Find");
            	//生成结果
                GenerateResult(currentGrid);
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
                        if (x >= 0 && y >= 0 && x < size && y < size)
                        {
							//点亮访问
            				if (objs[x, y].transform.GetChild(0).GetComponent<MeshRenderer>().material.color == new Color(1.0f, 1.0f, 1.0f))
            				{
                				objs[x, y].transform.GetChild(0).GetComponent<MeshRenderer>().material.color
                				= new Color(0.4f, 0.4f, 0.4f, 1);
            				}
                            //如果未超出所有格子范围，不是障碍物，不是重复点
                            if (!closeList.Contains(grids[x, y]) && grids[x,y].type != GridType.Barrier && grids[x,y] != currentGrid)
                            {
                                //计算H值
                                grids[x, y].h = Manhattan(x, y);
                                //计算F值
                                grids[x, y].f = grids[x, y].g + grids[x, y].h;
                                //如果未添加到开启列表
                                if (!openList.Contains(grids[x, y]))
                                {
                                    //添加
                                    openList.Add(grids[x, y]);
                                }
                                //重新排序
                                openList.Sort();
                            }
                        }
                    }
                }
            }
			//如果开启列表空，未能找到路径
            if (openList.Count == 0)
            {
                Debug.Log("Can not Find");
                break;
            }
            lastgrid = openList[0] as Grid;
            lastgrid.parent = currentGrid;
            //完成遍历添加该点到关闭列表
            closeList.Add(lastgrid);
            //从开启列表中移除
            openList.Remove(lastgrid);
        }

        //获取结束时间
        if(recorded==false)
		{
			t_new1= DateTime.Now;
			ts = (t_new1 - tAstar1);
       	 	timelast = (int)ts.Milliseconds;
        	Pathlength = parentList.Count;
        	done = true;
		}
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
                        	if (objs[x, y].transform.GetChild(0).GetComponent<MeshRenderer>().material.color == new Color(1.0f,1.0f,1.0f))
							{
								objs [x, y].transform.GetChild (0).GetComponent<MeshRenderer> ().material.color
								= new Color (0.4f, 0.4f, 0.4f, 1);
							}
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
		if(recorded==false)
		{
			t_new1= DateTime.Now;
			ts = (t_new1 - tAstar1);
       	 	timelast = (int)ts.Milliseconds;
        	Pathlength = parentList.Count;
        	done = true;
		}
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
				//点亮访问
                if (objs[i, j].transform.GetChild(0).GetComponent<MeshRenderer>().material.color == new Color(1.0f,1.0f,1.0f))
				{
					objs [i, j].transform.GetChild (0).GetComponent<MeshRenderer> ().material.color
					= new Color (0.4f, 0.4f, 0.4f, 1);
				}
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
							int g = currentGrid.g + (int)(Mathf.Sqrt ((Mathf.Abs (i) + Mathf.Abs (j))) * 10);
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
		if(recorded==false)
		{
			t_new1= DateTime.Now;
			ts = (t_new1 - tAstar1);
       	 	timelast = (int)ts.Milliseconds;
        	Pathlength = parentList.Count;
        	done = true;
		}
	}

	//AdvancedAstar
	IEnumerator AdvancedAstar()
	{
		//等待前面操作完成
        yield return new WaitForSeconds (0.1f);
        //记录开始时间
        DateTime tAstar1 = DateTime.Now;
        DateTime t_new1 = new DateTime();
		//添加起始点
        closeList.Add(grids[startX, startY]);
        //声明当前格子变量，并赋初值
        Grid currentGrid = openList [0] as Grid;
        Grid lastgrid = new Grid(startX, startY);
        
        //循环遍历路径最小F的点
        while (openList.Count > 0 && currentGrid.type != GridType.End)
        {
        	currentGrid = lastgrid;
			//如果当前点就是目标
        	if (currentGrid.type == GridType.End)
        	{
           	 	Debug.Log("Find");
            	//生成结果
                GenerateResult(currentGrid);
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
                        if (x >= 0 && y >= 0 && x < size && y < size)
                        {
                            //点亮访问
                            if (objs[x, y].transform.GetChild(0).GetComponent<MeshRenderer>().material.color == new Color(1.0f, 1.0f, 1.0f))
                            {
                                objs[x, y].transform.GetChild(0).GetComponent<MeshRenderer>().material.color
                                = new Color(0.4f, 0.4f, 0.4f, 1);
                            }
                            //如果未超出所有格子范围，不是障碍物，不是重复点
                            if (!closeList.Contains(grids[x, y]))
                            {
                                //计算H值
                                grids[x, y].h = Manhattan(x, y);
                                int hcos = CosH(startX, startY, x, y, targetX, targetY);
								//计算G值
								int ggg = currentGrid.g + (int)(Mathf.Sqrt ((Mathf.Abs (i) + Mathf.Abs (j))) * 10);
								if(currentGrid.type==GridType.Barrier)
								{
                                    ggg += g1;
                                }
								else if(currentGrid.type==GridType.Barrier1)
								{
                                    ggg += g2;
                                }
								else if(currentGrid.type==GridType.Barrier2)
								{
                                    ggg += g3;
                                }
								//与原G值对照
								if (grids [x, y].g == 1 || grids [x, y].g > ggg) 
								{
									//更新G值
									grids [x, y].g = ggg;
									grids [x, y].parent = currentGrid;
								}	
                                //计算F值
                                grids[x, y].f = grids[x, y].g + grids[x, y].h - omega*hcos;
                                //如果未添加到开启列表
                                if (!openList.Contains(grids[x, y]))
                                {
                                    //添加
                                    openList.Add(grids[x, y]);
                                }
                                //重新排序
                                openList.Sort();
                            }
                        }
                    }
                }
            }
            lastgrid = openList[0] as Grid;
            //lastgrid.parent = currentGrid;
            //完成遍历添加该点到关闭列表
            closeList.Add(lastgrid);
            //从开启列表中移除
            openList.Remove(lastgrid);
            //如果开启列表空，未能找到路径
            if (openList.Count == 0)
            {
                Debug.Log("Can not Find");
            }
        }

        //获取结束时间
        if(recorded==false)
		{
			t_new1= DateTime.Now;
			ts = (t_new1 - tAstar1);
       	 	timelast = (int)ts.Milliseconds;
        	Pathlength = parentList.Count;
        	done = true;
		}
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
		//监听按钮
        startbutton.GetComponent<Button>().onClick.AddListener(delegate ()
        {
			if(choice>1)
			{
				findingstart = true;
			}
        });
		resetbutton.GetComponent<Button>().onClick.AddListener(delegate()
		{
            SceneManager.LoadScene(0);
            choice = 0;
            ran = false;

        });
        Astarbutton.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            AlgorithmType = 1;
        });
		Dijkstrabutton.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            AlgorithmType = 2;
        });
		Floydbutton.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            AlgorithmType = 3;
        });
		AdAstarbutton.GetComponent<Button>().onClick.AddListener(delegate ()
        {
            AlgorithmType = 4;
        });
		//
    }

	private void Update() 
	{
        if(done==true)
		{
			//展示时间
        	Timetext.text = "时间：" + timelast + "ms";
        	Pathtext.text = "路径长度:" + Pathlength;
            done = false;
            recorded = false;
        }
		if(findingstart==true)
		{
            gettype = AlgorithmType;
			if(ran==false)
			{
				if(gettype==1)
				{
                    ran = true;
                    StartCoroutine(Astar());
					StartCoroutine(ShowResult());
            		AType.text = "标准Astar";
        		}
				if(gettype==2)
				{	
					ran = true;
            		StartCoroutine(Dijkstra());
            		StartCoroutine(ShowResult());
            		AType.text = "Dijkstra";
        		}
				if(gettype==3)
				{
					ran = true;
            		StartCoroutine(Floyd());
            		StartCoroutine(ShowResult());
					AType.text = "Floyd";
				}
				if(gettype==4)
				{
					ran = true;
                	StartCoroutine(AdvancedAstar());
                	StartCoroutine(ShowResult());
					AType.text = "改进Astar";
				}	
			}
		}
	}
}
