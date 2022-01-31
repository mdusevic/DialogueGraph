using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
     private readonly Vector2 defaultNodeSize = new Vector2(150, 200);
     
     public DialogueGraphView()
     {
          this.AddManipulator(new ContentDragger());
          this.AddManipulator(new SelectionDragger());
          this.AddManipulator(new RectangleSelector());

          AddElement(GenerateEntryPointNode());
     }

     public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
     {
          List<Port> compatiblePorts = new List<Port>();

          ports.ForEach((port) =>
          {
               if (startPort != port && startPort.node != port.node)
               {
                    compatiblePorts.Add(port);
               }
          });

          return compatiblePorts;
     }

     private Port GeneratePort(DialogueNode node, Direction portDirection, 
          Port.Capacity capacity = Port.Capacity.Single)
     {
          return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); // Arbitrary Type
     }

     private DialogueNode GenerateEntryPointNode()
     {
          DialogueNode node = new DialogueNode
          {
               GUID = Guid.NewGuid().ToString(),
               title = "START",
               dialogueText = "ENTRYPOINT",
               entryPoint = true
          };

          Port generatedPort = GeneratePort(node, Direction.Output);
          generatedPort.portName = "Next";
          node.outputContainer.Add(generatedPort);

          node.RefreshExpandedState();
          node.RefreshPorts();

          node.SetPosition(new Rect(100, 200, 100, 150));
          return node;
;    }

     public void CreateNode(string nodeName)
     {
          AddElement(CreateDialogueNode(nodeName));
     }

     public DialogueNode CreateDialogueNode(string nodeName)
     {
          DialogueNode dialogueNode = new DialogueNode
          {
               GUID = Guid.NewGuid().ToString(),
               title = nodeName,
               dialogueText = nodeName
          };
          
          Port inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
          inputPort.portName = "Input";
          dialogueNode.inputContainer.Add(inputPort);
          
          Button button = new Button(() => { AddChoicePort(dialogueNode); })
          {
               text = "New Choice"
          };
          dialogueNode.titleContainer.Add(button);
          
          dialogueNode.RefreshExpandedState();
          dialogueNode.RefreshPorts();
          
          dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));
          return dialogueNode;
     }

     private void AddChoicePort(DialogueNode dialogueNode)
     {
          Port generatedPort = GeneratePort(dialogueNode, Direction.Output);
          int outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
          
          generatedPort.portName = $"Choice {outputPortCount}";
          
          dialogueNode.outputContainer.Add(generatedPort);
          dialogueNode.RefreshExpandedState();
          dialogueNode.RefreshPorts();
     }
}
