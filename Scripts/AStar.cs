using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class AStar : MonoBehaviour
{
    private GridLogic gridLogic;

    private Node targetNode;
    private Node startNode;

    private PriorityQueue fringe;
    private List<Node> visited;

	// Use this for initialization
	void Start ()
	{
        //Init all the values
	    fringe = null;
	    visited = null;
	    GameObject gameGrid = GameObject.Find("GameGrid");
	    gridLogic = gameGrid.GetComponent<GridLogic>();
	}
	
	// Update is called once per frame
	void Update ()
	{
        
    }

    //Primary AStar controller
    public List<Node> DoAStar(Node start, Node target)
    {
        startNode = start;
        targetNode = target;
        //Init variables used

        fringe = new PriorityQueue();
        //Add the first node as a single node path to start from
        AddStartAsFirstPath(); 
        //Run the algorithm
        Path shortest = Run();
        //Once we are done empty the fringe and visited list before quitting
        fringe.Empty();
        visited.Clear();
        return shortest.GetNodeList();
    }

    private Path Run()
    {
        Path shortestPath = null;
        bool pathFound = false;
        //Run until we find a path
        while (!pathFound)
        {
            //Dequeue the shortest path in the fringe priority queue
            Path exploreThis = fringe.Dequeue();
            //If there is nothing in the fringe it will be null which means no path found
            if (exploreThis == null)
            {
                Debug.Log("No path found...");
                pathFound = true;
                shortestPath = null; //No path found... return null.
            }
            //If the last node on the dequeued path is the target we are done
            if (exploreThis.GetLastNode().Equals(targetNode))
            {
                //Console log we are done
                Node finalNode = exploreThis.GetLastNode().GetComponent<Node>();
                Debug.Log("Arrived at: (" + finalNode.GetX() + ", " + finalNode.GetY() + ")");
                shortestPath = exploreThis;
                pathFound = true;
            }
            //If we haven't found the end and there are still things to explore,
            //Add all children paths to the fringe
            else
            {
                AddChildrenPathsToFringe(exploreThis);
            }
        }
        return shortestPath;
    }

    //Create an initial 'Path' object from the starting node and add it to fringe
    private void AddStartAsFirstPath()
    {
        float heuristic = CalculateEuclidean(startNode);
        List<Node> empty = new List<Node>();
        Path beginning = new Path(heuristic, empty, startNode);
        //Init the fringe and visited list
        visited = new List<Node>();
        visited.Add(startNode);
        fringe = new PriorityQueue();
        fringe.Enqueue(beginning);
    }

    //Get all children of the passed in path and add them to the fringe.
    //Also add them to the visited list
    private void AddChildrenPathsToFringe(Path parent)
    {
        //Get the last node in the passed in path
        Node parentNode = parent.GetLastNode().GetComponent<Node>();
        Debug.Log("Searching: (" + parentNode.GetX() + ", " + parentNode.GetY() + ")");
        //Get all neighbors of the last node in the path
        GameObject[] children = gridLogic.GetNeighbors(parentNode.GetX(), parentNode.GetY());
        foreach (GameObject child in children)
        {
            //If the child is not null
            if (child != null)
            {
                Node node = child.GetComponent<Node>();
                //If the node is accessible and has not been visited
                if (node.IsAccessible() && !IsVisited(node))
                {
                    //Create a new path
                    float heuristic = CalculateEuclidean(node);
                    Path newPath = new Path(heuristic, parent.GetNodeList(), node);
                    //Add the new path to the fringe
                    fringe.Enqueue(newPath);
                    //Add the child to the visited list
                    visited.Add(node);
                }
            }
        }
    }

    //Check if the passed node has been visited
    private bool IsVisited(Node node)
    {
        for(int i = 0; i < visited.Count; i++)
        {
            if (visited[i].Equals(node))
            {
                return true;
            }
        }
        return false;
    }

    //This 'euclidean' is in units of nodes not game distance.
    private float CalculateEuclidean(Node current)
    {
        int squared = 2;
        //The nodes are on a grid so determine sides of the triangle
        float xDiff = Mathf.Abs(targetNode.GetX() - current.GetX());
        float yDiff = Mathf.Abs(targetNode.GetY() - current.GetY());
        //Then use pythagorean to determine the distance in 'Node' coordinates
        return Mathf.Sqrt(Mathf.Pow(xDiff, squared) + Mathf.Pow(yDiff, squared));
    }

    //PriorityQueue for the fringe
    public class PriorityQueue
    {
        //Use an array list as the queue
        List<Path> queue;

        //Ctor
        public PriorityQueue()
        {
            //Init the queue
            queue = new List<Path>();
        }

        //Add items into the queue where they belong based on priority
        public void Enqueue(Path newPath)
        {
            //If the queue is empty just add the item
            if (queue.Count == 0)
            {
                queue.Add(newPath);
            }
            else
            {
                bool inserted = false;
                //Iterate through the queue
                for (int i = 0; i < queue.Count; i++)
                {
                    Path current = (Path) queue[i];
                    //If current path has a cost less then or equal to the current position,
                    if (current != null && (newPath.GetCost() <= current.GetCost()))
                    {
                        //Then insert and break
                        queue.Insert(i, newPath);
                        inserted = true;
                        break;
                    }
                }
                //If we go through the whole thing and the path hasn't been inserted
                if (!inserted)
                {   //Add the node at the end
                    queue.Add(newPath);
                }
            }
        }

        //Simply remove the head node and return it.
        //If the queue is empty return null
        public Path Dequeue()
        {
            if (queue.Count == 0)
            {
                return null;
            }
            Path head = (Path)queue[0];
            queue.RemoveAt(0);
            return head;
        }

        //Clear the whole queue
        public void Empty()
        {
            queue.Clear();
        }
    }

    //Path object
    public class Path
    {
        private readonly float _cost;
        private readonly List<Node> _nodeList;

        //Ctor
        public Path(float heuristic, List<Node> previousPath, Node newNode)
        {
            //Duplicate the old list then add the new node to it for this path
            _nodeList = DuplicateList(previousPath);
            _nodeList.Add(newNode);
            //Calculate and set the cost of this path
            _cost = _nodeList.Count + heuristic;
        }

        //Simply duplicate the list passed in
        private List<Node> DuplicateList(List<Node> previous)
        {
            List<Node> newPath = new List<Node>();
            if (previous != null && previous.Count != 0)
            {
                for (int i = 0; i < previous.Count; i++)
                {
                    newPath.Add(previous[i]);
                }
            }
            return newPath;
        }

        public int Length()
        {
            return _nodeList.Count;
        }

        public float GetCost()
        {
            return _cost;
        }

        public List<Node> GetNodeList()
        {
            return _nodeList;
        }

        public Node GetLastNode()
        {
            return _nodeList[_nodeList.Count - 1];
        }
    }
}
