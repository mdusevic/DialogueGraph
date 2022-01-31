using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;

    private List<Edge> edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();
    
    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility()
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        if (!edges.Any())
        {
            return;
        }

        DialogueContainer dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();

        Edge[] connectedPorts = edges.Where(x => x.input.node != null).ToArray();

        for (int i = 0; i < connectedPorts.Length; i++)
        {
            DialogueNode outputNode = connectedPorts[i].output.node as DialogueNode;
            DialogueNode inputNode = connectedPorts[i].input.node as DialogueNode;
            
            dialogueContainer.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGUID = outputNode.GUID,
                portName = connectedPorts[i].output.portName,
                targetNodeGUID = inputNode.GUID
            });
        }

        foreach (DialogueNode dialogueNode in nodes.Where(node => !node.entryPoint))
        {
            dialogueContainer.dialogueNodeData.Add(new DialogueNodeData
            {
                GUID = dialogueNode.GUID,
                dialogueText = dialogueNode.dialogueText,
                position = dialogueNode.GetPosition().position
            });
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }
    
    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<DialogueContainer>(fileName);

        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target dialogue graph file does not exist!", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            List<NodeLinkData> connections =
                _containerCache.nodeLinks.Where(x => x.baseNodeGUID == nodes[i].GUID).ToList();

            for (int j = 0; j < connections.Count; j++)
            {
                string targetNodeGUID = connections[j].targetNodeGUID;
                DialogueNode targetNode = nodes.First(x => x.GUID == targetNodeGUID);
                LinkNodes(nodes[i].outputContainer[j].Q<Port>(), (Port) targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(
                    _containerCache.dialogueNodeData.First(x => x.GUID == targetNodeGUID).position,
                    _targetGraphView.defaultNodeSize
                ));
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        Edge tempEdge = new Edge
        {
            output = output,
            input = input
        };
        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        
        _targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (DialogueNodeData nodeData in _containerCache.dialogueNodeData)
        {
            DialogueNode tempNode = _targetGraphView.CreateDialogueNode(nodeData.dialogueText);
            tempNode.GUID = nodeData.GUID;
            _targetGraphView.AddElement(tempNode);

            List<NodeLinkData> nodePorts = _containerCache.nodeLinks.Where(x => x.baseNodeGUID == nodeData.GUID).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }

    private void ClearGraph()
    {
        // Set entry points GUID back from the save. Discard existing GUID.
        nodes.Find(x => x.entryPoint).GUID = _containerCache.nodeLinks[0].baseNodeGUID;

        foreach (DialogueNode node in nodes)
        {
            if (node.entryPoint)
            {
                continue;
            }

            // Removes edges that are connected to this node
            edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
            
            // Then removes the node from the graph
            _targetGraphView.RemoveElement(node);
        }
    }
}
