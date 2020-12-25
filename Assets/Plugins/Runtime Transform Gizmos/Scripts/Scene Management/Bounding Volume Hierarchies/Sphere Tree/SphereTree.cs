using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public class SphereTree<T>
    {
        #region Protected Variables
        protected SphereTreeNode<T> _rootNode;
        protected int _numberOfChildNodesPerNode;
        protected Queue<SphereTreeNode<T>> _terminalNodesPendingIntegration = new Queue<SphereTreeNode<T>>();
        protected Queue<SphereTreeNode<T>> _nodesPendingRecomputation = new Queue<SphereTreeNode<T>>();
        #endregion

        #region Constructors
        public SphereTree(int numberOfChildNodesPerNode)
        {
            _numberOfChildNodesPerNode = Mathf.Max(2, numberOfChildNodesPerNode);
            CreateRootNode();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Performs a raycast using the specified ray and returns a list of hit information
        /// for each terminal node that was hit.
        /// </summary>
        public List<SphereTreeNodeRayHit<T>> RaycastAll(Ray ray)
        {
            var hitTerminalNodes = new List<SphereTreeNodeRayHit<T>>(20);
            RaycastAllRecurse(ray, _rootNode, hitTerminalNodes);

            return hitTerminalNodes;
        }

        /// <summary>
        /// Returns a list that contains all terminal nodes that intersect or are fully
        /// encapsulated by the specified sphere.
        /// </summary>
        public List<SphereTreeNode<T>> OverlapSphere(Sphere3D sphere)
        {
            var overlappedTerminalNodes = new List<SphereTreeNode<T>>(20);
            OverlapSphereRecurse(sphere, _rootNode, overlappedTerminalNodes);
            return overlappedTerminalNodes;
        }

        /// <summary>
        /// Returns a list that contains all terminal nodes that intersect or are fully
        /// encapsulated by the specified box.
        /// </summary>
        public List<SphereTreeNode<T>> OverlapBox(OrientedBox box)
        {
            var overlappedTerminalNodes = new List<SphereTreeNode<T>>(20);
            OverlapBoxRecurse(box, _rootNode, overlappedTerminalNodes);
            return overlappedTerminalNodes;
        }

        /// <summary>
        /// Returns a list that contains all terminal nodes that intersect or are fully
        /// encapsulated by the specified box.
        /// </summary>
        public List<SphereTreeNode<T>> OverlapBox(Box box)
        {
            var overlappedTerminalNodes = new List<SphereTreeNode<T>>(20);
            OverlapBoxRecurse(box.ToOrientedBox(), _rootNode, overlappedTerminalNodes);
            return overlappedTerminalNodes;
        }

        /// <summary>
        /// The client code must call this function during each frame update in order
        /// to ensure that any pending node updates are performed.
        /// </summary>
        public void PerformPendingUpdates()
        {
            // First, ensure that all nodes are recomputed accordingly
            while(_nodesPendingRecomputation.Count != 0)
            {
                SphereTreeNode<T> node = _nodesPendingRecomputation.Dequeue();

                // Note: If the node has any children left, we will recompute its volume. Otherwise,
                //       we will remove the node from the tree.
                if (node.HasChildren) node.RecomputeCenterAndRadius();
                else RemoveNode(node);
            }

            // At this point, all super sphere nodes have had their volume updated accordingly.
            // In the next step, we will integrate any necessary terminal nodes.
            while (_terminalNodesPendingIntegration.Count != 0)
            {
                SphereTreeNode<T> node = _terminalNodesPendingIntegration.Dequeue();
                IntegrateTerminalNode(node);
            }
        }

        /// <summary>
        /// Adds a terminal node to the tree.
        /// </summary>
        /// <remarks>
        /// The function does not integrate the node inside the sphere hierarchy. It
        /// will only add it to the integration pending queue. The actual integration
        /// process will be performed inside 'PerformPendingUpdates'.
        /// </remarks>
        /// <param name="sphere">
        /// The node's sphere.
        /// </param>
        /// <param name="data">
        /// The node's data.
        /// </param>
        /// <returns>
        /// The node which was added to the tree.
        /// </returns>
        public SphereTreeNode<T> AddTerminalNode(Sphere3D sphere, T data)
        {
            // Create a new node and mark it as terminal
            var newTerminalNode = new SphereTreeNode<T>(sphere, this, data);
            newTerminalNode.SetFlag(SphereTreeNodeFlags.Terminal);

            // Add the node to the integration queue
            AddNodeToIntegrationQueue(newTerminalNode);

            return newTerminalNode;
        }

        /// <summary>
        /// Removes the specified node from the tree.
        /// </summary>
        public void RemoveNode(SphereTreeNode<T> node)
        {
            // If the node is not the root node and if it has a parent...
            if(!node.IsRoot && node.Parent != null)
            {
                // Remove the node from its parent
                SphereTreeNode<T> parentNode = node.Parent;
                parentNode.RemoveChild(node);

                // Move up the hierarhcy and remove all parents which don't have any children any more.
                // Note: We will always stop at the root node. The root node is allowed to exist even
                //       if it has no children.
                while (parentNode.Parent != null && parentNode.HasNoChildren && !parentNode.IsRoot)
                {
                    SphereTreeNode<T> newParent = parentNode.Parent;
                    newParent.RemoveChild(parentNode);
                    parentNode = newParent;
                }

                // At this point 'parentNode' references the deepest parent which has at least one child.
                // Because we have removed children from it, its volume must be recalculated, so we add
                // it to the recomputation queue.
                // Note: Even if this function was called from 'PerformPendingUpdates' we still get correct
                //       results because the node will be added to the recomputation queue and it will be 
                //       processed inside the recomputation 'while' loop from where this method is called.
                AddNodeToRecomputationQueue(parentNode);
            }
        }

        /// <summary>
        /// Updates the center of the specified terminal node.
        /// </summary>
        public void UpdateTerminalNodeCenter(SphereTreeNode<T> terminalNode, Vector3 newCenter)
        {
            terminalNode.Center = newCenter;
            OnTerminalNodeSphereUpdated(terminalNode);
        }

        /// <summary>
        /// Updates the radius of the specified terminal node.
        /// </summary>
        public void UpdateTerminalNodeRadius(SphereTreeNode<T> terminalNode, float newRadius)
        {
            terminalNode.Radius = newRadius;
            OnTerminalNodeSphereUpdated(terminalNode);
        }

        /// <summary>
        /// Updates the center and radius of the specified terminal node.
        /// </summary>
        public void UpdateTerminalNodeCenterAndRadius(SphereTreeNode<T> terminalNode, Vector3 newCenter, float newRadius)
        {
            terminalNode.Center = newCenter;
            terminalNode.Radius = newRadius;
            OnTerminalNodeSphereUpdated(terminalNode);
        }

        /// <summary>
        /// Returns a dictionary which maps each terminal node data to the actual terminal node.
        /// </summary>
        public Dictionary<T, SphereTreeNode<T>> GetDataToTerminalNodeDictionary()
        {
            var dictionary = new Dictionary<T, SphereTreeNode<T>>();
            GetDataToTerminalNodeDictionaryRecurse(_rootNode, dictionary);
            return dictionary;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Creates the root node of the tree.
        /// </summary>
        private void CreateRootNode()
        {
            // Create the root node with some sensible default values. It doesn't really matter what
            // initial size the root node has. It will grow as needed when new nodes are added to the
            // tree.
            _rootNode = new SphereTreeNode<T>(Vector3.zero, 10.0f, this);
            _rootNode.SetFlag(SphereTreeNodeFlags.Root | SphereTreeNodeFlags.SuperSphere);
        }

        /// <summary>
        /// Integrates the specified node into the tree hierarchy. Integrating a nod means placing it
        /// inside the tree hierarchy in the correct spot. Because it is a terminal node, the method
        /// will have to search for the correct super-sphere node which can act as a parent of this node.
        /// </summary>
        private void IntegrateTerminalNode(SphereTreeNode<T> nodeToIntegrate)
        {
            // Start a recursive process from the root of the hierarchy. After the integration
            // is finished, we will clear the node's integration flag because it no loger needs
            // to be integrated.
            IntegrateTerminalNodeRecurse(nodeToIntegrate, _rootNode);
            nodeToIntegrate.ClearFlag(SphereTreeNodeFlags.MustIntegrate);
        }

        /// <summary>
        /// This is a recursive method which is responsible for integration the specified
        /// node inside the tree.
        /// </summary>
        private void IntegrateTerminalNodeRecurse(SphereTreeNode<T> nodeToIntegrate, SphereTreeNode<T> parentNode)
        {
            // If this node still has room for children, we will add the integration node here. This 'if' statement
            // will also handle the special case in which only the root node currently exists inside the tree.
            if(parentNode.NumberOfChildren < _numberOfChildNodesPerNode)
            {
                // Add the node as a child of the parent node and ensure that the root node encapsulates it
                parentNode.AddChild(nodeToIntegrate);
                //parentNode.EncapsulateChildNode(nodeToIntegrate);
                parentNode.RecomputeCenterAndRadius();
            }
            else
            {
                // If there is no more room, we will proceed by choosing one of the parent's children which
                // is closest to the node that we want to integrate. We choose the closest node because when
                // the node will be added as a child of it, we want the parent to grow as little as possible.
                List<SphereTreeNode<T>> children = parentNode.Children;
                SphereTreeNode<T> closestChild = FindClosestNode(children, nodeToIntegrate);
                if (closestChild == null) return;

                // If the closest child is not a terminal node, recurse.
                if (!closestChild.IsTerminal) IntegrateTerminalNodeRecurse(nodeToIntegrate, closestChild);
                else
                {
                    SphereTreeNode<T> newParentNode = new SphereTreeNode<T>(closestChild.Sphere, this, default(T));
                    newParentNode.SetFlag(SphereTreeNodeFlags.SuperSphere);

                    // Replace 'closestChild' with the new parent node and add both 'closestChild' and 'nodeToIntegrate' as children of it
                    parentNode.RemoveChild(closestChild);
                    parentNode.AddChild(newParentNode);
                    newParentNode.AddChild(nodeToIntegrate);
                    newParentNode.AddChild(closestChild);

                    // Encapsulate the children inside the new node
                    //newParentNode.EncapsulateChildNode(closestChild);
                    //newParentNode.EncapsulateChildNode(nodeToIntegrate);

                    // Ensure that the new node is fully contained inside the parent node
                    //parentNode.EncapsulateChildNode(newParentNode);

                    newParentNode.RecomputeCenterAndRadius();
                    parentNode.RecomputeCenterAndRadius();
                }
            }
        }

        /// <summary>
        /// Finds and returns the node inside 'nodes' which is closest to 'node'.
        /// </summary>
        private SphereTreeNode<T> FindClosestNode(List<SphereTreeNode<T>> nodes, SphereTreeNode<T> node)
        {
            float minDistanceSq = float.MaxValue;
            SphereTreeNode<T> closestNode = null;

            // We will choose the node whose center is closest to 'node'
            foreach(SphereTreeNode<T> potentialNode in nodes)
            {
                // Calculate the squared distance between the node centers
                float distanceBetweenNodesSq = potentialNode.GetDistanceBetweenCentersSq(node);
                if(distanceBetweenNodesSq < minDistanceSq)
                {
                    // Smaller than what we have so far?
                    minDistanceSq = distanceBetweenNodesSq;
                    closestNode = potentialNode;

                    // If we somehow managed to find a node which has the same position as 'node', we can exit 
                    if (minDistanceSq == 0.0f) return closestNode;
                }
            }

            return closestNode;
        }

        /// <summary>
        /// Adds the specified node to the recomputation queue.
        /// </summary>
        private void AddNodeToRecomputationQueue(SphereTreeNode<T> node)
        {
            // Only non-terminal, non-root nodes are allowed. We also have to ensure that
            // the node hasn't already been added to the recomputation queue.
            if (node.IsTerminal || node.IsRoot || node.MustRecompute) return;
            if (node.IsSuperSphere)
            {
                node.SetFlag(SphereTreeNodeFlags.MustRecompute);
                _nodesPendingRecomputation.Enqueue(node);
            }
        }

        /// <summary>
        /// Adds the specified node to the integration queue.
        /// </summary>
        private void AddNodeToIntegrationQueue(SphereTreeNode<T> node)
        {
            // Only terminal, non-root nodes are allowed. We also have to ensure that
            // the node hasn't already been added to the integration queue.
            if (node.IsSuperSphere || node.IsRoot || node.MustIntegrate) return;
            if (node.IsTerminal)
            {
                node.SetFlag(SphereTreeNodeFlags.MustIntegrate);
                _terminalNodesPendingIntegration.Enqueue(node);
            }
        }

        /// <summary>
        /// This method is called when the sphere of a terminal node has been updated.
        /// </summary>
        private void OnTerminalNodeSphereUpdated(SphereTreeNode<T> terminalNode)
        {
            // If the node is already marked for reintegration, there is nothing left for us to do
            if (terminalNode.MustIntegrate) return;

            // If the node is now outside of its parent, it may now be closer to another parent and associating
            // it with that new parent may provide better space/volume savings. So we remove the node from its
            // parent and add it to the integration queue so that it can be reintegrated. During the integration
            // process, the algorithm may find a more optimal parent or the same one if a more optimal one doesn't
            // exist.
            // Note: We are only removing the child from its parent if it went outside of its parent volume. It may
            //       probably be a better idea to always check the surrounding parents and see if a more optimal one
            //       exists even if the node doesn't pierce its parent's skin. For the moment however, we will only
            //       remove the child from its parent if it pierced its skin.
            SphereTreeNode<T> parentNode = terminalNode.Parent;
            float distanceToParentNodeExitPoint = parentNode.GetDistanceToNodeExitPoint(terminalNode);
            if (distanceToParentNodeExitPoint > parentNode.Radius)
            {
                // Note: It may be a good idea to check if the node contains only one child after removal. In that
                //       case the node itself can be remove and its child moved up the hierarchy. However, for the
                //       moment we'll keep things simple.
                // Remove the child from its parent and add it to the integration queue. Adding it to
                // the integration queue is necessary in order to ensure that it gets reassigned to
                // the correct parrent based on its current position.
                parentNode.RemoveChild(terminalNode);
                AddNodeToIntegrationQueue(terminalNode);
            }

            // Whenever a terminal node is updated, it's parent must gave its volume recomputed. We always do this
            // regardless of whether or not the node was removed from the parent or not.
            AddNodeToRecomputationQueue(parentNode);
        }

        /// <summary>
        /// Recursive function which renders the specified node in the scene view and then
        /// steps down the hierarchy for each child of the node.
        /// </summary>
        private void RenderGizmosDebugRecurse(SphereTreeNode<T> node)
        {
            // Draw the sphere for the specified node and then recurse for each child node
            Gizmos.DrawSphere(node.Sphere.Center, node.Sphere.Radius);
            foreach (var child in node.Children) RenderGizmosDebugRecurse(child);
        }

        /// <summary>
        /// Recursive method which is used to detect which terminal nodes in the tree are intersected
        /// by 'ray'. Information about the intersected nodes is stored inside 'terminalNodeHitInfo'.
        /// </summary>
        private void RaycastAllRecurse(Ray ray, SphereTreeNode<T> parentNode, List<SphereTreeNodeRayHit<T>> terminalNodeHitInfo)
        {
            // If the parent node is not hit by the ray, there is no need to go further
            float t;
            if (!parentNode.Sphere.Raycast(ray, out t)) return;
            else
            {
                // If the parent node was hit, we have 2 choices:
                //  a) if the node is a terminal node, we add it to the 'hitNodes' list and exit;
                //  b) if the node is a super-sphere, we will recurse for eacoh if its children.
                if(parentNode.IsTerminal)
                {
                    terminalNodeHitInfo.Add(new SphereTreeNodeRayHit<T>(ray, t, parentNode));
                    return;
                }
                else
                {
                    // Recurse for each child node
                    List<SphereTreeNode<T>> childNodes = parentNode.Children;
                    foreach (SphereTreeNode<T> childNode in childNodes) RaycastAllRecurse(ray, childNode, terminalNodeHitInfo);
                }
            }
        }

        /// <summary>
        /// Recursive method which is used to step down the tree hierarchy collecting all terminal nodes
        /// which are overlapped by the specified sphere.
        /// </summary>
        private void OverlapSphereRecurse(Sphere3D sphere, SphereTreeNode<T> parentNode, List<SphereTreeNode<T>> overlappedTerminalNodes)
        {
            // If the parent is not overlapped there is no need to go any further
            if (!parentNode.Sphere.OverlapsFullyOrPartially(sphere)) return;
            else
            {
                // If this is a terminal node, add it to the output list and return
                if(parentNode.IsTerminal)
                {
                    overlappedTerminalNodes.Add(parentNode);
                    return;
                }
                else
                {
                    // Recurs for each child node
                    List<SphereTreeNode<T>> childNodes = parentNode.Children;
                    foreach (SphereTreeNode<T> childNode in childNodes) OverlapSphereRecurse(sphere, childNode, overlappedTerminalNodes);
                }
            }
        }

        /// <summary>
        /// Recursive method which is used to step down the tree hierarchy collecting all terminal nodes
        /// which are overlapped by the specified box.
        /// </summary>
        private void OverlapBoxRecurse(OrientedBox box, SphereTreeNode<T> parentNode, List<SphereTreeNode<T>> overlappedTerminalNodes)
        {
            // If the parent is not overlapped there is no need to go any further
            if (!parentNode.Sphere.OverlapsFullyOrPartially(box)) return;
            else
            {
                // If this is a terminal node, add it to the output list and return
                if (parentNode.IsTerminal)
                {
                    overlappedTerminalNodes.Add(parentNode);
                    return;
                }
                else
                {
                    // Recurs for each child node
                    List<SphereTreeNode<T>> childNodes = parentNode.Children;
                    foreach (SphereTreeNode<T> childNode in childNodes) OverlapBoxRecurse(box, childNode, overlappedTerminalNodes);
                }
            }
        }

        /// <summary>
        /// Recursive method which steps down the tree hierarchy and adds pairs of node-data/node to 'dictionary'.
        /// When this method returns, all the terminal nodes and their data will be stored inside 'dictionary'.
        /// </summary>
        private void GetDataToTerminalNodeDictionaryRecurse(SphereTreeNode<T> parentNode, Dictionary<T, SphereTreeNode<T>> dictionary)
        {
            if (parentNode.IsTerminal) dictionary.Add(parentNode.Data, parentNode);
            else
            {
                List<SphereTreeNode<T>> children = parentNode.Children;
                foreach (var child in children) GetDataToTerminalNodeDictionaryRecurse(child, dictionary);
            }
        }
        #endregion
    }
}