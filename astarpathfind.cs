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

        // The nodes for our grid
		public Node(Vector3 pos) {
			this.estimatedCost = 0.0f;
			this.nodeTotalCost = 1.0f;
			this.bObstacle = false;
			this.parent = null;
			this.position = pos;

		}

        // Mark a node as obstacle
		public void MarkAsObstacle() {
			markedobs++;
			bObstacle = true;

		}

        // Compare the estimated cost to another node's
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

        // Get and track the neighbor nodes
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

        // Determine which cells rae blocked by trees, etc.
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

		}

        // Get the nearest node to an arbitrary position
		public static Node GetNearestNode(Vector3 pos) {
			int index = GetGridIndex(pos);
			return nodes[getRow(index),getColumn (index)];
		}

        // Get the row of a 1D index
		public static int getRow (int index ){
			int row = index / numCols;
			return row;
		}
        // Get the column of a 1D index
		public static int getColumn (int index) {
			int col = index % numCols;
			return col;
		}

        // Get the position of a grid cell including calculating its height
		public static Vector3 GetGridCellPosition(int index) {
			int col = index%numCols;
			int row = index/numCols;
			Vector3 posWithHeight = new Vector3(col * gridCellSize, 100.0f, row * gridCellSize);
			posWithHeight = new Vector3(posWithHeight.x,Terrain.activeTerrain.SampleHeight(posWithHeight)+1.2f,posWithHeight.z);
			return posWithHeight;
		}

        // Get the center of a grid cell
		public static Vector3 GetGridCellCenter(int index) {
			Vector3 cellPosition = GetGridCellPosition(index);
			cellPosition.x += (gridCellSize / 2.0f);
			cellPosition.z += (gridCellSize / 2.0f);
			return cellPosition;
		}

        // Get the 1D index of a position
		public static int GetGridIndex (Vector3 pos) {
			int col = (int)(pos.x/gridCellSize);
			int row = (int)(pos.z/gridCellSize);
			return (row * numCols + col);

		}

	}

    // Find a path from start to end without length limit
	public static List<astarpathfind.Node> findPath(Vector3 start, Vector3 end) {
		return findPath (start, end, 50, null);
	}

    // Find a path specifying the lenght limit and the last path
	public static List<astarpathfind.Node> findPath(Vector3 start, Vector3 end, int limit, List<Node> lastPath) {
		astarcalls++;
		print (astarcalls);

		Node startNode = GridManager.GetNearestNode(start);
		Node endNode = GridManager.GetNearestNode(end);
		return findPath(startNode,endNode,limit,lastPath);

	}

    /// <summary>
    /// Perform a tweaked A* search to satisfy pathfinding need
    /// </summary>
    /// <param name="start">start position</param>
    /// <param name="goal">goal position</param>
    /// <param name="limit">the longest path we can return</param>
    /// <param name="lastPath">the last path returned so we can avoid returning identical/nearly identical paths by
    /// not reusing nodes from the last search</param>
    /// <returns>A list of nodes in order that form the shortest path from start to goal</returns>
	public static List<Node>  findPath(Node start, Node goal, int limit, List<Node> lastPath) {

		if (cooldown > 0)
			return null;
		else {
			cooldown = cooldownfloor + UnityEngine.Random.Range (0f, cooldownrange);
			Console.WriteLine ("cooldown: " + cooldown.ToString ());
		}


        // Initialization and creating open and closed lists
		List<Node>  openList = new List<Node>();
		openList.Add (start);
		start.nodeTotalCost = 0.0f;
		start.estimatedCost = (start.position - goal.position).magnitude;
		List<Node> closedList = new List<Node>();
		List<Node> path = null;
		Node node = null;


		GridManager.getNeighborNodes (start, openList, 0);

        // While we are still exploring in the open list
		while (openList.Count!=0) {

            // Get the node with the least estimated cost
			node = (Node)openList[0];
			for (int i = 0; i < openList.Count; i++) {
				if (((Node)openList[i]).estimatedCost < node.estimatedCost)
					node = (Node)openList[i];
			}

            // For counting the steps/distance in nodes from origin
			int steps = 0;
			Node stepCounter = node;
			while (stepCounter!=null) {
				stepCounter = stepCounter.parent;
				steps ++;

			}
			bool bGoal = false;
			
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
            // If we are done, we need to reset the parents in each node to empty
            // to make this instance reusable
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

            // Get the neighbors
			List<Node> neighbors = new List<Node>();
			GridManager.getNeighborNodes (node,neighbors,0);

            // Go through all the neighbors - this is wher classic A* is
			for (int i = 0; i < neighbors.Count; i++ ) {
				Node neighborNode = (Node)neighbors[i];
                // If the neighbor is one of the already-explored ones this will not execute
				if (!closedList.Contains (neighborNode) && !lastPath.Contains(neighborNode)) {

                    // Get the total cost so far and estimated cost
					float cost = (node.position - neighborNode.position).magnitude;
					float totalCost = node.nodeTotalCost + cost;
					float neighborNodeEstCost = (neighborNode.position - goal.position).magnitude;

					neighborNode.nodeTotalCost = totalCost;
					neighborNode.parent = node;
					neighborNode.estimatedCost = totalCost + neighborNodeEstCost;

                    // Add the neighbor to the open list and continue the search
					if (!openList.Contains(neighborNode)) {
						//print ("echo 4");
						if (!neighborNode.bObstacle)
							openList.Add(neighborNode);


					}
				}
			}

			//openList.Sort();
			//print ("echo 5");

            // Move this from the open list to the closed list
			closedList.Add (node);
			openList.Remove (node);

		}

        // Return a null path if the goal was not found
		if (node.position != goal.position) {
			//Debug.LogError ("Goal not found!");
			return null;
		}
		return path;
	}

}
