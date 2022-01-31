using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
     public DialogueGraphView()
     {
          this.AddManipulator(new ContentDragger());
          this.AddManipulator(new SelectionDragger());
          this.AddManipulator(new RectangleSelector());

          AddElement(GenerateEntryPointNode());
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

          node.SetPosition(new Rect(100, 200, 100, 150));
          return node;
;     }
}
