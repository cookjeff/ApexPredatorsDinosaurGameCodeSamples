using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class astarpathfind : MonoBehaviour {
	static int markedobs = 0;
	static int astarcalls = 0;
	public int showingUp;
	public static GameObject nodeMarker;
	public static float steepLimit = 1f;
	static float cooldown = 1f;
	static float cooldownfloor = 0.3f;
	static float cooldownrange = 0.6f;

	public GameObject sphereMarker;
	//public GridManager gridManager;
	public void Awake() {

		//nodeMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
		//gridManager = new GridManager();
		//gridManager.aAwake();
		GridManager.Start();

	}

	public void Update() {


		cooldown -= Time.deltaTime;


	}

	public class Node : IComparable {
		public float nodeTotalCost;
		public float estimatedCost;
		public bool bObstacle;
		public Node parent;
		public Vector3 position;
		public GameObject marker;

		//public Node() {
		//	this.estimatedCost = 0.0f;
		//	this.nodeTotalCost = 1.0f;
		//	this.bObstacle = false;
		//	this.parent = null;
		//	//marker = (GameObject)Instantiate(nodeMarker,new Vector3(10f,10f,10f),Quaternion.identity);
		//marker.SetActive(false);

		//}
		public Node(Vector3 pos) {
			this.estimatedCost = 0.0f;
			this.nodeTotalCost = 1.0f;
			this.bObstacle = false;
			this.parent = null;
			this.position = pos;
			//marker = (GameObject)Instantiate(nodeMarker,pos,Quaternion.identity);
			//marker.SetActive(false);


		}

		public void MarkAsObstacle() {
			markedobs++;
			bObstacle = true;
			//Destroy (marker);
			//marker.GetComponent<Renderer> ().enabled = false;
			//marker.SetActive (false);

		}

		public int CompareTo(object obj) {
			Node node = (Node)obj;
			if (this.estimatedCost < node.estimatedCost) {
				return -1;
			} else if (this.estimatedCost > node.estimatedCost) {
				return 1;
			} else {
				return 0;
			}
		}
	}

	public static class GridManager : System.Object {
		// Singleton as fuck
		/*		private static GridManager s_instance;

		public static GridManager instance {
			get {
				if (s_instance == null) {
					s_instance = new GridManager();
					if (s_instance == null) 	
						Debug.LogError ("Could not locate a gridmanager object");
				}
				return s_instance;
			}
		}
	*/	
		public static int numRows;
		public static int numCols;
		public static float gridCellSize;
		public static bool showGrid = true;
		public static bool showObstacleBlocks = true;

		//private GameObject [] obstacleList;
		public static Node[,] nodes { get; set; }

		private static Vector3[] treePosList;

		public static void Start() {
			int currIndex = 0;
			numRows = 126;
			numCols = 126;
			gridCellSize = 4f;
			treePosList = new Vector3[Terrain.activeTerrain.terrainData.treeInstanceCount];
			foreach (TreeInstance tree in Terrain.activeTerrain.terrainData.treeInstances) {
				treePosList[currIndex] = Vector3.Scale(tree.position, Terrain.activeTerrain.terrainData.size) + Terrain.activeTerrain.transform.position;
				currIndex++;
			}
			CalculateObstacles ();
		}

		public static bool isInBounds(int row, int col) {
			//if (row >= 0 && row < numRows && col >= 0 && col < numCols && nodes [row, col].bObstacle)
			//					return false;
			return (row >= 0 && row < numRows && col >= 0 && col < numCols && !nodes[row, col].bObstacle);
		}

		public static void getNeighborNodes(Node node, List<Node> neighbors, int skip) {
			int row = getRow(GetGridIndex(node.position));
			int col = getColumn(GetGridIndex (node.position));

			if (isInBounds (row - (1+skip), col) ) {
				if ((nodes[row-(1+skip),col].position.y-node.position.y) < steepLimit)
					neighbors.Add (nodes[row-(1+skip),col]);
			}
			if (isInBounds (row + (1+skip), col) ) {
				if ((nodes[row+(1+skip),col].position.y-node.position.y) < steepLimit)
					neighbors.Add (nodes[row+(1+skip),col]);
			}

			if (isInBounds (row , col+(1+skip) )) {
				if ((nodes[row,col+(skip+1)].position.y-node.position.y) < steepLimit)
					neighbors.Add (nodes[row,col+(1+skip)]);
			}
			if (isInBounds (row , col-(1+skip)) ) {
				if ((nodes[row,col-(skip+1)].position.y-node.position.y) < steepLimit)
					neighbors.Add (nodes[row,col-(1+skip)]);
			}


			if (isInBounds (row - (1+skip) , col + (1+skip)) ) {
				if ((nodes[row-(1+skip),col+(1+skip)].position.y-node.position.y) < steepLimit)
					neighbors.Add (nodes[row - (1+skip),col+(1+skip)] );
			}
			if (isInBounds (row + (1+skip), col - (1+skip)) ) {
				if ((nodes[row+(1+skip),col-(1+skip)].position.y-node.position.y) < steepLimit)
					neighbors.Add (nodes[row + (1+skip), col-(1+skip)]);
			}
			if (isInBounds (row - (1+skip), col - (1+skip)) ) {
				if ((nodes[row-(1+skip),col-(1+skip)].position.y-node.position.y) < steepLimit)
					neighbors.Add (nodes[row - (1+skip),col-(1+skip)] );
			}
			if (isInBounds (row + (1+skip), col + (1+skip)) ) {
				if ((nodes[row+(1+skip),col+(1+skip)].position.y-node.position.y) < steepLimit)
					neighbors.Add (nodes[row + (1+skip), col+(1+skip)]);
			}	


		}

		static void CalculateObstacles() {
			// Create the matrix of nodes and populate it
			nodes = new Node[numCols,numRows];
			int index = 0;
			for (int i = 0; i < numCols; i++) {
				for (int j = 0; j < numRows; j++) {
					Vector3 cellPos = GetGridCellCenter (index);
					Node node = new Node(cellPos);


					nodes[i,j] = node;
					index++;
				}			
			}
			int obstacles = 0;
			// Block out all the nodes that are occupied
			foreach(Vector3 data in treePosList) {

				int indexCell = GetGridIndex(data);
				int col = getColumn(indexCell);
				int row = getRow (indexCell);
				//print ("indxcell: " + indexCell);

				if (col < numCols && row < numRows) {
					nodes[row, col].MarkAsObstacle();
					obstacles++;
				}

				/*

				if (isInBounds(row-1, col))
					nodes[ row-1, col ].MarkAsObstacle();
				if (isInBounds(row+1, col))
					nodes[ row+1, col ].MarkAsObstacle();
				if (isInBounds(row, col-1))
					nodes[ row, col-1 ].MarkAsObstacle();
				if (isInBounds(row, col+1))
					nodes[ row, col+1 ].MarkAsObstacle();

				if (isInBounds(row-1, col-1))
					nodes[ row-1, col-1 ].MarkAsObstacle();
				if (isInBounds(row-1, col+1))
					nodes[ row-1, col+1 ].MarkAsObstacle();
				if (isInBounds(row+1, col-1))
					nodes[ row+1, col-1 ].MarkAsObstacle();
				if (isInBounds(row+1, col+1))
					nodes[ row+1, col+1 ].MarkAsObstacle();
				*/

			}
			print (obstacles + " base obstacles marked");
			int postcheck = 0;

			for (int ind = 0; ind < nodes.Length ; ind++) 
				if (nodes[getRow(ind),getColumn(ind)].bObstacle)
					postcheck++;
			print (postcheck + "obstacles remain from iterative count");

			// Go ahead and block the fuck out of other obstacles 


		}

		public static Node GetNearestNode(Vector3 pos) {
			int index = GetGridIndex(pos);
			return nodes[getRow(index),getColumn (index)];
		}

		public static int getRow (int index ){
			int row = index / numCols;
			return row;
		}
		public static int getColumn (int index) {
			int col = index % numCols;
			return col;
		}
		public static Vector3 GetGridCellPosition(int index) {
			int col = index%numCols;
			int row = index/numCols;
			Vector3 posWithHeight = new Vector3(col * gridCellSize, 100.0f, row * gridCellSize);
			posWithHeight = new Vector3(posWithHeight.x,Terrain.activeTerrain.SampleHeight(posWithHeight)+1.2f,posWithHeight.z);
			return posWithHeight;
		}

		public static Vector3 GetGridCellCenter(int index) {
			Vector3 cellPosition = GetGridCellPosition(index);
			cellPosition.x += (gridCellSize / 2.0f);
			cellPosition.z += (gridCellSize / 2.0f);
			return cellPosition;
		}

		public static int GetGridIndex (Vector3 pos) {
			int col = (int)(pos.x/gridCellSize);
			int row = (int)(pos.z/gridCellSize);
			return (row * numCols + col);

		}

	}
	public static List<astarpathfind.Node> findPath(Vector3 start, Vector3 end) {
		return findPath (start, end, 50, null);
	}
	public static List<astarpathfind.Node> findPath(Vector3 start, Vector3 end, int limit, List<Node> lastPath) {
		astarcalls++;
		print (astarcalls);


			


		Node startNode = GridManager.GetNearestNode(start);
		Node endNode = GridManager.GetNearestNode(end);
		//List<Node> nodepath = 
		return findPath(startNode,endNode,limit,lastPath);	

		//List<astarpathfind.Node> returner = new List<astarpathfind.Node> ();

		//for (int i = 0; i < nodepath.Count; i++)
		//	returner.Add (nodepath [i]);

		//return returner;
	}

	public static List<Node>  findPath(Node start, Node goal, int limit, List<Node> lastPath) {

		if (cooldown > 0)
			return null;
		else {
			cooldown = cooldownfloor + UnityEngine.Random.Range (0f, cooldownrange);
			Console.WriteLine ("cooldown: " + cooldown.ToString ());
		}


		List<Node>  openList = new List<Node>();
		//print ("echo 2");
		openList.Add (start);
		//start.marker.SetActive(true);
		start.nodeTotalCost = 0.0f;
		start.estimatedCost = (start.position - goal.position).magnitude;
		List<Node> closedList = new List<Node>();
		List<Node> path = null;
		Node node = null;


		//foreach (Node dpNode in GridManager.nodes) {
		//	dpNode.parent = null;
		//}
		GridManager.getNeighborNodes (start, openList, 0);

		while (openList.Count!=0) {
			//openList.Sort ();
			node = (Node)openList[0];
			for (int i = 0; i < openList.Count; i++) {
				if (((Node)openList[i]).estimatedCost < node.estimatedCost)
					node = (Node)openList[i];
			}
			//print ("openlist size " + openList.Count );

			int steps = 0;
			Node stepCounter = node;
			while (stepCounter!=null) {
				stepCounter = stepCounter.parent;
				steps ++;

			}
			bool bGoal = false;
			//			print (node.position);
			// See if the current node is our goal
			if (node.position == goal.position || steps > limit) {
				// we're done, get and return the path
				path = new List<Node>();
				while (node != null) {
					path.Add (node);
					//node.marker.SetActive(true);
					node = node.parent;
					//node.parent;
				}
				path.Reverse();
				bGoal = true;
			} 

			if (bGoal) {
				foreach (Node dpNode in path)
					dpNode.parent = null;
				foreach (Node dpNode in openList)
					dpNode.parent = null;
				foreach (Node dpNode in closedList)
					dpNode.parent = null;

				return path;
			}
			if (lastPath==null)
				lastPath = new List<Node>();
			List<Node> neighbors = new List<Node>();
			GridManager.getNeighborNodes (node,neighbors,0);
			for (int i = 0; i < neighbors.Count; i++ ) {
				Node neighborNode = (Node)neighbors[i];
				if (!closedList.Contains (neighborNode) && !lastPath.Contains(neighborNode)) {

					float cost = (node.position - neighborNode.position).magnitude;
					float totalCost = node.nodeTotalCost + cost;
					float neighborNodeEstCost = (neighborNode.position - goal.position).magnitude;

					neighborNode.nodeTotalCost = totalCost;
					neighborNode.parent = node;
					neighborNode.estimatedCost = totalCost + neighborNodeEstCost;

					if (!openList.Contains(neighborNode)) {
						//print ("echo 4");
						if (!neighborNode.bObstacle)
							openList.Add(neighborNode);


					}
				}
			}

			//openList.Sort();
			//print ("echo 5");

			closedList.Add (node);
			openList.Remove (node);

		}

		if (node.position != goal.position) {
			//Debug.LogError ("Goal not found!");
			return null;
		}
		return path;
	}

}
