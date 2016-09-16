using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BFS : MonoBehaviour {
    //Lists for the Nodes in the BFS
    private Queue<BFSHelper> fringe;
    public List<Node> walkable;
    public List<Node> attackable; 
    private List<BFSHelper> visited;

    private Node startNode;

    private GridLogic gridLogic;

    // Use this for initialization
    void Start ()
	{
	    fringe = new Queue<BFSHelper>();
	    walkable = new List<Node>();
        attackable = new List<Node>();
	    visited = new List<BFSHelper>();

        GameObject points = GameObject.Find("GameGrid");
        gridLogic = points.GetComponent<GridLogic>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public void GetWalkableNodes(Node start, int moveDist)
    {
        if (startNode != null && startNode.Equals(start))
        {
            ClearAllLists();
            startNode = null;
            return;
        }
        ClearAllLists();
        startNode = start;
        gridLogic.GetNode(4,6).GetComponent<Node>().SetCost(2);
        gridLogic.GetNode(4,7).GetComponent<Node>().SetCost(2);
        gridLogic.GetNode(4,8).GetComponent<Node>().SetCost(2);
        BFSHelper startNodeHelper = new BFSHelper(startNode, 0);
        //The cost to the first node should be 0 total
        startNodeHelper.SetCostHere(0);
        fringe.Enqueue(startNodeHelper);
        do
        {
            BFSHelper current = fringe.Dequeue();
            CheckNeighbors(current, moveDist);
        } while (fringe.Count > 0);
    }

    public void ClearAllLists()
    {
        startNode = null;
        fringe.Clear();
        walkable.Clear();
        attackable.Clear();
        visited.Clear();
    }

    private void CheckNeighbors(BFSHelper current, int maxMoveDist)
    {
//        if (current.GetCostHere() >= maxMoveDist)
//        {
//            return;
//        }
        GameObject[] neighbors = gridLogic.GetNeighbors(current.GetX(), current.GetY());
        foreach (GameObject nodeObject in neighbors)
        {
            if (nodeObject != null)
            {
                Node node = nodeObject.GetComponent<Node>();
                BFSHelper bfsNode = new BFSHelper(node, current.GetCostHere());
                if (NotVisited(bfsNode) && InRange(bfsNode, maxMoveDist))
                {
                    visited.Add(bfsNode);
                    walkable.Add(node);
                    fringe.Enqueue(bfsNode);
                    if(!attackable.Contains(node))
                        attackable.Add(node);
                }
                else
                {
                    attackable.Add(node);
                }
            }
        }

    }

    private bool NotVisited(BFSHelper node)
    {
        if (visited.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < visited.Count; i++)
        {
            //If the nodes are equal then this has been visited
            if (NodesEqual(visited[i], node))
            {
                //If we are visiting this node but at a cheaper cost,
                //remove the old instance and we will add the new instance
                if (IsCheaperCost(visited[i], node))
                {
                    visited.RemoveAt(i);
                    return true;
                }
                return false;
            }
        }
        return true;

    }

    public List<Node> GetAttackableList()
    {
        return attackable;
    } 

    private bool NodesEqual(BFSHelper visitedNode, BFSHelper checkThis)
    {
        return visitedNode.GetNode().Equals(checkThis.GetNode());
    }

    private bool IsCheaperCost(BFSHelper visitedNode, BFSHelper checkThis)
    {
        return visitedNode.GetCostHere() > checkThis.GetCostHere();
    }

    private bool InRange(BFSHelper node, int maxMoveDist)
    {
        if (node.GetCostHere() > maxMoveDist)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void ColorWalkable()
    {
        foreach (Node node in walkable)
        {
            node.SetColor(Color.blue);
        }
    }

    public void ColorAttackable()
    {
        foreach (Node node in attackable)
        {
            node.SetColor(Color.red);
        }
    }

    public bool IsNodeWalkable(Node node)
    {
        return walkable.Contains(node);
    }

    //This is a class to hold nodes as well as the cost so far to get to the held node
    private class BFSHelper
    {
        private int costHere;
        private Node node;

        public BFSHelper(Node thisNode, int cost)
        {
            //Cost to move here is equal to 
            //cost passed in (cost to the node before this)
            // +
            //cost of this node
            costHere = cost + thisNode.GetCost();
            node = thisNode;
        }

        public Node GetNode()
        {
            return node;
        }

        public int GetCostHere()
        {
            return costHere;
        }

        public int GetX()
        {
            return node.GetX();            
        }

        public int GetY()
        {
            return node.GetY();
        }

        public void SetCostHere(int newCostHere)
        {
            costHere = newCostHere;
        }
    }
}
