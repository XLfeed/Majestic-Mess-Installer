///////////////////////////////////////////////////////////////////////////////////////
//
//  \file    UIButtonTest.cs
//
//  \brief   Test script for verifying UI mouse events (hover, click, pointer down/up).
//           Attach this to a UI element (Panel/Image) to test raycasting and event
//           propagation. Logs TRUE if the element received the click, FALSE otherwise.
//
//  \author  Pearl Goh [100%]
//
// All content © 2025 DigiPen Institute of Technology Singapore.
// All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////////////

using Engine;

public class UIButtonTest : Entity
{
    private bool clickedOnMe = false;

    public override void OnInit()
    {
    }

    public override void OnUIHoverEnter(UIPointerEventInfo eventInfo)
    {
    }

    public override void OnUIHoverExit(UIPointerEventInfo eventInfo)
    {
    }

    public override void OnUIPointerDown(UIPointerEventInfo eventInfo)
    {
    }

    public override void OnUIPointerUp(UIPointerEventInfo eventInfo)
    {
    }

    public override void OnUIClick(UIPointerEventInfo eventInfo)
    {
        clickedOnMe = true;
    }

    public override void OnUpdate(float deltaTime)
    {
        // Check if mouse was clicked this frame
        if (Input.IsMouseButtonPressed(0))
        {
            if (clickedOnMe)
            {
                Debug.Log("TRUE");
            }
            else
            {
                Debug.Log("FALSE");
            }
            clickedOnMe = false;
        }
    }
}
