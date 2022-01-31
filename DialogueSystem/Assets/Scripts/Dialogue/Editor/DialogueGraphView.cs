using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Edge = UnityEditor.Experimental.GraphView.Edge;

public class DialogueGraphView : GraphView
{
     public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
     
     public DialogueGraphView()
     {
          // Style sheet to update graph visuals
          styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
          
          // Allows the user to zoom in and out
          SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
          
          // Basic graph functions
          this.AddManipulator(new ContentDragger()); // Allows content to be moved
          this.AddManipulator(new SelectionDragger()); // Allows sections to be dragged
          this.AddManipulator(new RectangleSelector()); // Creates a rectangular selection box
          
          // Creates a basic grid background
          GridBackground grid = new GridBackground();
          Insert(0, grid);
          grid.StretchToParentSize(); // Stretches to fill window

          AddElement(GenerateEntryPointNode()); // Adds the start node
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

          node.capabilities &= ~Capabilities.Movable;
          node.capabilities &= ~Capabilities.Deletable;

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
          
          dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
          
          Button button = new Button(() => { AddChoicePort(dialogueNode); })
          {
               text = "New Choice"
          };
          dialogueNode.titleContainer.Add(button);

          TextField textField = new TextField(string.Empty);
          textField.RegisterValueChangedCallback(evt =>
          {
               dialogueNode.dialogueText = evt.newValue;
               dialogueNode.title = evt.newValue;
          });
          textField.SetValueWithoutNotify(dialogueNode.title);
          dialogueNode.mainContainer.Add(textField);
          
          dialogueNode.RefreshExpandedState();
          dialogueNode.RefreshPorts();
          
          dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));
          return dialogueNode;
     }

     public void AddChoicePort(DialogueNode dialogueNode, string overridenPortName = "")
     {
          Port generatedPort = GeneratePort(dialogueNode, Direction.Output);
          
          // Removes port label from being seen in the editor
          Label oldLabel = generatedPort.contentContainer.Q<Label>("type");
          generatedPort.contentContainer.Remove(oldLabel);

          int outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
          
          generatedPort.portName = $"Choice {outputPortCount}";

          string choicePortName = string.IsNullOrEmpty(overridenPortName)
               ? $"Choice {outputPortCount + 1}"
               : overridenPortName;

          TextField textField = new TextField
          {
               name = string.Empty,
               value = choicePortName
          };

          textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
          generatedPort.contentContainer.Add(new Label("   "));
          generatedPort.contentContainer.Add(textField);
          Button deleteButton = new Button(() => RemovePort(dialogueNode, generatedPort))
          {
               text = "X"
          };
          generatedPort.contentContainer.Add(deleteButton);
          
          generatedPort.portName = choicePortName;
          dialogueNode.outputContainer.Add(generatedPort);
          dialogueNode.RefreshPorts();
          dialogueNode.RefreshExpandedState();
     }

     private void RemovePort(DialogueNode dialogueNode, Port generatedPort)
     {
          IEnumerable<Edge> targetEdge = edges.ToList().Where(x => 
               x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

          if (targetEdge.Any()) // Remove edge if port is connected
          {
               var edge = targetEdge.First();
               edge.input.Disconnect(edge);
               RemoveElement(targetEdge.First());
          }

          // Delete the port whether or not connected
          dialogueNode.outputContainer.Remove(generatedPort);
          dialogueNode.RefreshPorts();
          dialogueNode.RefreshExpandedState();
     }
}
